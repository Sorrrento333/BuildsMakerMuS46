$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repo = "F:\Proyecto Builds\Programa"
$w6 = Join-Path $repo ".opencode-wire6"
New-Item -ItemType Directory -Force -Path $w6 | Out-Null
$logPath = Join-Path $w6 "recapture-wire6-summary.txt"
function L([string]$s) { $s | Out-File -LiteralPath $logPath -Append -Encoding utf8 }
L "== RECAPTURE-WIRE6 =="

$sdk400Root = "C:\Users\SORREN~1\AppData\Local\Temp\opencode\sdk400\dotnet"
$dotnet400 = Join-Path $sdk400Root "dotnet.exe"
$env:DOTNET_ROOT = $sdk400Root
$env:DOTNET_MULTILEVEL_LOOKUP = "0"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$env:PATH = "$sdk400Root;" + $env:PATH

$artifactRoot = Join-Path $repo "artifacts\json-everything-source-build"
$sourceRoot = Join-Path $artifactRoot "source"
$lockReviewedRoot = Join-Path $repo "spikes\json-everything-source-build\locks"
$spikePs1 = Join-Path $repo "tools\spikes\Test-JsonEverythingSourceBuild.ps1"
$provenancePath = Join-Path $repo "spikes\json-everything-source-build\source-build-provenance.json"

$dotnetExe = Test-Path -LiteralPath $dotnet400
L "dotnet400-exe=$dotnetExe"
$sdkVersion = (& $dotnet400 --version 2>$null | Select-Object -First 1)
L "sdk400-version=$sdkVersion"
L "source-root=$sourceRoot"
L "source-git-exists=$(Test-Path -LiteralPath (Join-Path $sourceRoot '.git'))"
L "locksReviewedRoot-exists=$(Test-Path -LiteralPath $lockReviewedRoot)"

function Test-NoLockedRestore {
    param([string]$ProjectPath, [string]$Label)
    if (-not (Test-Path -LiteralPath $ProjectPath)) {
        L "  $Label-PROJ-MISSING"
        return $false
    }
    $restoreProps = @(
        "-p:TargetFrameworks=net10.0"
        "-p:RestorePackagesWithLockFile=true"
        "-p:ManagePackageVersionsCentrally=false"
        "-p:CentralPackageTransitivePinningEnabled=false"
    )
    Push-Location $sourceRoot
    try {
        $out = & $dotnet400 restore $ProjectPath @restoreProps 2>&1 | ForEach-Object { "$_" }
        L "  $Label-restore-exit=$LASTEXITCODE"
        $tg = $out | Select-String -Pattern 'Microsoft.Build.Tasks.Git"\s?:\s?"([^"]+)"' | Select-Object -First 1
        if ($tg) { L "  $Label-TASKSGIT=$($tg.Matches[0].Groups[1].Value)" } else { L "  $Label-TASKSGIT=none-in-out" }
    }
    finally { Pop-Location }
    return ($LASTEXITCODE -eq 0)
}

$locksCopy = [ordered]@{
    "Json.More"    = @{ proj = "src\Json.More\Json.More.csproj";    lockName = "Json.More.packages.lock.json" }
    "JsonPointer"  = @{ proj = "src\JsonPointer\JsonPointer.csproj";  lockName = "JsonPointer.packages.lock.json" }
    "JsonSchema"   = @{ proj = "src\JsonSchema\JsonSchema.csproj";    lockName = "JsonSchema.packages.lock.json" }
}

foreach ($entry in $locksCopy.GetEnumerator()) {
    $label = $entry.Key
    $projRel = $entry.Value.proj
    $lockName = $entry.Value.lockName
    $projPath = Join-Path $sourceRoot $projRel
    $origLockInSource = Join-Path (Split-Path -Parent (Split-Path -Parent (Join-Path $sourceRoot $projRel))) (Split-Path -Leaf (Split-Path -Parent $projRel))
    $srcLock = Join-Path (Split-Path -Parent $projPath) "packages.lock.json"
    L "$label-proj-exists=$(Test-Path -LiteralPath $projPath)"
    $ok = Test-NoLockedRestore $projPath $label
    $srcLockNow = Test-Path -LiteralPath $srcLock
    L "  $label-SRCLOCK-NOW=$srcLockNow"
    if ($srcLockNow) {
        $dest = Join-Path $lockReviewedRoot $lockName
        Copy-Item -LiteralPath $srcLock -Destination $dest -Force
        L "  $label-COPY-TO-REVIEWED=$lockName ok"
        $tgInWhole = Select-String -LiteralPath $dest -Pattern 'Microsoft.Build.Tasks.Git"\s?:\s?"([^"]+)"' | Select-Object -First 1
        if ($tgInWhole) { L "  $label-REVIEWED-TASKSGIT=$($tgInWhole.Matches[0].Groups[1].Value)" }
    }
}

L "== SPIKE REAL BAJO SDK400 =="
L "spikePs1-exists=$(Test-Path -LiteralPath $spikePs1)"
Push-Location $repo
try {
    $out = & powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $spikePs1 -ExpectedSdkVersion "10.0.400" 2>&1 | ForEach-Object { "$_" }
    L "spike-exit=$LASTEXITCODE"
    $out | ForEach-Object { L "  spike: $_" }
}
finally { Pop-Location }
L "== FIN RECAPTURE-WIRE6 =="