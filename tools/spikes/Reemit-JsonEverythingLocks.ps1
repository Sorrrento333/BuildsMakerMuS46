param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")),
    [string]$ExpectedSdkVersion = "10.0.400"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$inputRoot = Join-Path $RepositoryRoot "spikes/json-everything-source-build"
$provenancePath = Join-Path $inputRoot "source-build-provenance.json"
if (-not (Test-Path -LiteralPath $provenancePath)) {
    throw "Missing reviewed provenance: $provenancePath"
}
$provenance = Get-Content -LiteralPath $provenancePath -Raw | ConvertFrom-Json
if ($provenance.sdkVersion -ne $ExpectedSdkVersion) {
    throw "Expected reviewed provenance SDK $ExpectedSdkVersion, found $($provenance.sdkVersion)."
}
$repositoryUrl = $provenance.repository
$sourceCommit = $provenance.sourceCommit
$sourceLicenseSha256 = $provenance.sourceLicenseSha256

function Invoke-Checked {
    param([string]$FilePath, [string[]]$Arguments, [string]$WorkingDirectory)
    Push-Location $WorkingDirectory
    try {
        & $FilePath @Arguments
        if ($LASTEXITCODE -ne 0) {
            throw "Command failed ($LASTEXITCODE): $FilePath $($Arguments -join ' ')"
        }
    }
    finally {
        Pop-Location
    }
}

$artifactRoot = Join-Path $RepositoryRoot "artifacts/json-everything-source-build"
$sourceRoot = Join-Path $artifactRoot "source"

if (Test-Path -LiteralPath $artifactRoot) {
    $resolvedArtifactRoot = (Resolve-Path -LiteralPath $artifactRoot).Path
    $expectedRoot = [System.IO.Path]::GetFullPath(
        (Join-Path $RepositoryRoot "artifacts/json-everything-source-build"))
    if ($resolvedArtifactRoot -ne $expectedRoot) {
        throw "Refusing to remove unexpected artifact path: $resolvedArtifactRoot"
    }
    Remove-Item -LiteralPath $resolvedArtifactRoot -Recurse -Force
}
New-Item -ItemType Directory -Path $artifactRoot | Out-Null

Invoke-Checked git @("clone", "--filter=blob:none", "--no-checkout", $repositoryUrl, $sourceRoot) $RepositoryRoot
Push-Location $sourceRoot
try {
    Invoke-Checked git @("config", "core.autocrlf", "false") $sourceRoot
    Invoke-Checked git @("config", "core.eol", "lf") $sourceRoot
    Invoke-Checked git @("checkout", "--detach", $sourceCommit) $sourceRoot
}
finally {
    Pop-Location
}

$lockMappings = [ordered]@{
    "Json.More"   = "src/Json.More/packages.lock.json"
    "JsonPointer" = "src/JsonPointer/packages.lock.json"
    "JsonSchema"  = "src/JsonSchema/packages.lock.json"
}

function Copy-ReviewedLocks {
    param([string]$DestinationSourceRoot)
    foreach ($entry in $lockMappings.GetEnumerator()) {
        $reviewedLock = Join-Path $inputRoot "locks/$($entry.Key.Substring(0,0))$($entry.Key).packages.lock.json"
        $reviewedLock = Join-Path $inputRoot "locks/$($entry.Key).packages.lock.json"
        if (-not (Test-Path -LiteralPath $reviewedLock)) {
            throw "Missing reviewed lock: $reviewedLock"
        }
        Copy-Item -LiteralPath $reviewedLock -Destination (Join-Path $DestinationSourceRoot $entry.Value) -Force
    }
}

function Copy-RegeneratedLocks {
    param([string]$SourceSourceRoot)
    foreach ($entry in $lockMappings.GetEnumerator()) {
        $regenerated = Join-Path $SourceSourceRoot $entry.Value
        if (Test-Path -LiteralPath $regenerated) {
            Copy-Item -LiteralPath $regenerated -Destination (Join-Path $inputRoot "locks/$($entry.Key).packages.lock.json") -Force
        } else {
            throw "Regenerated lock not found: $regenerated"
        }
    }
}

Copy-ReviewedLocks $sourceRoot

$restoreProperties = @(
    "-p:TargetFrameworks=net10.0",
    "-p:RestorePackagesWithLockFile=true",
    "-p:ManagePackageVersionsCentrally=false",
    "-p:CentralPackageTransitivePinningEnabled=false"
)

foreach ($entry in $lockMappings.GetEnumerator()) {
    $csproj = Join-Path $sourceRoot (
        ($entry.Value -replace 'packages\.lock\.json$', '') + "$($entry.Key).csproj"
    )

    Write-Host "== Re-emitting reviewed lock for $($entry.Key) under SDK $ExpectedSdkVersion =="

    Invoke-Checked dotnet (@('restore', $csproj) + $restoreProperties) $sourceRoot
}

Copy-RegeneratedLocks $sourceRoot

foreach ($entry in $lockMappings.GetEnumerator()) {
    $csproj = Join-Path $sourceRoot (($entry.Value -replace "packages.lock.json$", "") + "$($entry.Key).csproj")
    Write-Host "== Verifying locked-mode restore for $($entry.Key) =="
    Invoke-Checked dotnet @("restore", $csproj, "--locked-mode", "-p:RestoreLockedMode=true") $sourceRoot
}

$tasksGit = Get-ChildItem -LiteralPath (Join-Path $inputRoot "locks") -Filter "*.packages.lock.json" |
    ForEach-Object {
        $content = Get-Content -LiteralPath $_.FullName -Raw
        if ($content -match 'Microsoft.Build.Tasks.Git"\s*:\s*"([^"]+)"') { $Matches[1] } else { $null }
    } | Sort-Object -Unique
Write-Host "Reviewed lock Tasks.Git bands: $($tasksGit -join ', ')"
foreach ($band in $tasksGit) {
    if ($band -notmatch '^10\.0\.4') {
        throw "Tasks.Git band $band is not in the .4xx range; re-emission did not take effect."
    }
}

Write-Host "PASS: re-emitted reviewed locks to Tasks.Git .4xx band under SDK $ExpectedSdkVersion."
