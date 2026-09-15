$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repo = "F:\Proyecto Builds\Programa"
$w5 = Join-Path $repo ".opencode-wire5"
New-Item -ItemType Directory -Force -Path $w5 | Out-Null
$logPath = Join-Path $w5 "recapture-type-and-spike.log.txt"
function L([string]$s) { $s | Out-File -LiteralPath $logPath -Append -Encoding utf8 }

$sdk400Root = "C:\Users\SORREN~1\AppData\Local\Temp\opencode\sdk400\dotnet"
$dotnet400 = Join-Path $sdk400Root "dotnet.exe"
$env:DOTNET_ROOT = $sdk400Root
$env:DOTNET_MULTILEVEL_LOOKUP = "0"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$env:PATH = "$sdk400Root;$env:PATH"

L "RECAPTURE-TYPE-AND-SPIKE inicio"
L "sdk400-exe=$(Test-Path -LiteralPath $dotnet400)"
$v = & $dotnet400 --version
$ver = ($v | ForEach-Object { $_ }) -join ""
L "sdk400-version=$ver"

$artifactRoot = Join-Path $repo "artifacts\json-everything-source-build"
$sourceRoot = Join-Path $artifactRoot "source"
$lockReviewedDir = Join-Path $repo "spikes\json-everything-source-build\locks"
$spikePs1 = Join-Path $repo "tools\spikes\Test-JsonEverythingSourceBuild.ps1"

$projects = [ordered]@{
    "Json.More"    = @{ rel = "src\Json.More\Json.More.csproj";      lockName = "Json.More.packages.lock.json" }
    "JsonPointer"  = @{ rel = "src\JsonPointer\JsonPointer.csproj";  lockName = "JsonPointer.packages.lock.json" }
    "JsonSchema"   = @{ rel = "src\JsonSchema\JsonSchema.csproj";    lockName = "JsonSchema.packages.lock.json" }
}
$props = @(
    "-p:TargetFrameworks=net10.0"
    "-p:RestorePackagesWithLockFile=true"
    "-p:ManagePackageVersionsCentrally=false"
    "-p:CentralPackageTransitivePinningEnabled=false"
)

foreach ($entry in $projects.GetEnumerator()) {
    $name = $entry.Key
    $rel = $entry.Value.rel
    $proj = Join-Path $sourceRoot $rel
    $lockFolder = Split-Path -Parent $proj
    $lockFile = Join-Path $lockFolder "packages.lock.json"
    $projExists = Test-Path -LiteralPath $proj
    L "project=$name exists=$projExists"
    if (-not $projExists) { continue }
    Push-Location $sourceRoot
    try {
        Remove-Item -LiteralPath $lockFile -Force -ErrorAction SilentlyContinue
        & $dotnet400 restore $proj @props --force --force-evaluate 2>&1 | ForEach-Object { L "  $name-restore: $_" }
        $restoreExit = $LASTEXITCODE
        L "  $name-restore-exit=$restoreExit"
    }
    finally { Pop-Location }
    $lockNow = Test-Path -LiteralPath $lockFile
    L "  $name-lock-generated=$lockNow"
    if ($lockNow) {
        $tg = Select-String -LiteralPath $lockFile -Pattern 'Microsoft.Build.Tasks.Git"\s*:\s*"[^"]+"' | Select-Object -First 1
        if ($tg) { L "  $name-tasksgit=$($tg.Matches[0].Value)" } else { L "  $name-tasksgit=none" }
        Copy-Item -LiteralPath $lockFile -Destination (Join-Path $lockReviewedDir $entry.Value.lockName) -Force
        L "  $name-lock-copied-to-reviewed=Yes"
    }
}

L "spike-run-inicio"
L "spike-ps1-exists=$(Test-Path -LiteralPath $spikePs1)"
Push-Location $repo
try {
    & powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $spikePs1 2>&1 | ForEach-Object { L "  spike: $_" }
    $spikeExit = $LASTEXITCODE
}
finally { Pop-Location }
L "spike-exit=$spikeExit"
L "RECAPTURE-TYPE-AND-SPIKE fin"
