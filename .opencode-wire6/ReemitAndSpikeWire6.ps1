$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repo = "F:\Proyecto Builds\Programa"
$w6 = Join-Path $repo ".opencode-wire6"
New-Item -ItemType Directory -Force -Path $w6 | Out-Null
$logPath = Join-Path $w6 "reemit-and-spike-wire6.log.txt"
Remove-Item -LiteralPath $logPath -ErrorAction SilentlyContinue
function L([string]$s) {
    $s | Out-File -LiteralPath $logPath -Append -Encoding utf8
}

$sdk400Root = "C:\Users\SORREN~1\AppData\Local\Temp\opencode\sdk400\dotnet"
$dotnet400 = Join-Path $sdk400Root "dotnet.exe"
$env:DOTNET_ROOT = $sdk400Root
$env:DOTNET_MULTILEVEL_LOOKUP = "0"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"

L "== REEMIT-AND-SPIKE-WIRE6 =="
L "dotnet400-exe=$(Test-Path -LiteralPath $dotnet400)"
$v = & $dotnet400 --version 2>&1 | ForEach-Object { $_ }
L "sdk400-version=$(($v -join '').Trim())"

$artifactRoot = Join-Path $repo "artifacts\json-everything-source-build"
$sourceRoot = Join-Path $artifactRoot "source"
$spikeInputRoot = Join-Path $repo "spikes\json-everything-source-build"
$lockReviewedRoot = Join-Path $spikeInputRoot "locks"
$spikePs1 = Join-Path $repo "tools\spikes\Test-JsonEverythingSourceBuild.ps1"

L "source-root-exists=$(Test-Path -LiteralPath $sourceRoot)"
L "lockReviewedRoot-exists=$(Test-Path -LiteralPath $lockReviewedRoot)"
$lockCount = @(Get-ChildItem -LiteralPath $lockReviewedRoot -Filter '*.packages.lock.json' -File -ErrorAction SilentlyContinue).Count
L "lockReviewed-count=$lockCount"

$projects = [ordered]@{
    "Json.More"   = @{ proj = "src\Json.More\Json.More.csproj";   lockName = "Json.More.packages.lock.json" }
    "JsonPointer" = @{ proj = "src\JsonPointer\JsonPointer.csproj"; lockName = "JsonPointer.packages.lock.json" }
    "JsonSchema"  = @{ proj = "src\JsonSchema\JsonSchema.csproj";   lockName = "JsonSchema.packages.lock.json" }
}

$restoreProps = @(
    "-p:TargetFrameworks=net10.0"
    "-p:RestorePackagesWithLockFile=true"
    "-p:ManagePackageVersionsCentrally=false"
    "-p:CentralPackageTransitivePinningEnabled=false"
    "-p:RestoreLockedMode=false"
)

$success = $true
Push-Location $sourceRoot
try {
    foreach ($entry in $projects.GetEnumerator()) {
        $label = $entry.Key
        $projAbs = Join-Path $sourceRoot $entry.Value.proj
        $projDir = Split-Path -Parent $projAbs
        $lockInSrc = Join-Path $projDir "packages.lock.json"
        L "  [$label] proj-exists=$(Test-Path -LiteralPath $projAbs)"
        if (-not (Test-Path -LiteralPath $projAbs)) { $success = $false; continue }
        if (Test-Path -LiteralPath $lockInSrc) { Remove-Item -LiteralPath $lockInSrc -Force }
        & $dotnet400 restore $projAbs @restoreProps 2>&1 | ForEach-Object { L "    restore: $_" }
        $restoreExit = $LASTEXITCODE
        L "  [$label] restore-exit=$restoreExit"
        if ($restoreExit -ne 0) { $success = $false; continue }
        $lockNow = Test-Path -LiteralPath $lockInSrc
        L "  [$label] lock-regenerated=$lockNow"
        if ($lockNow) {
            $tg = Select-String -LiteralPath $lockInSrc -Pattern 'Microsoft.Build.Tasks.Git"\s*:\s*"[^"]+"' | Select-Object -First 1
            if ($tg) { L "  [$label] tasksgit=$($tg.Matches[0].Value)" } else { L "  [$label] tasksgit=none" }
            Copy-Item -LiteralPath $lockInSrc -Destination (Join-Path $lockReviewedRoot $entry.Value.lockName) -Force
            L "  [$label] copied-back=$($entry.Value.lockName)"
        }
    }
}
finally { Pop-Location }

L "reemit-all-ok=$success"
if ($success) {
    L "== spike real bajo SDK .400 =="
    $env:PATH = "$sdk400Root;$env:PATH"
    & powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $spikePs1 -ExpectedSdkVersion "10.0.400" 2>&1 | ForEach-Object { L "  spike: $_" }
    $spikeExit = $LASTEXITCODE
    L "spike-exit=$spikeExit"
}
L "== REEMIT-AND-SPIKE-WIRE6 FIN =="