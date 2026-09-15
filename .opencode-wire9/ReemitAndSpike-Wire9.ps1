$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

# TODO wire9: re-emit los 3 locks a banda Tasks.Git .4xx bajo SDK .400

$repo = "F:\Proyecto Builds\Programa"
$w9 = Join-Path $repo ".opencode-wire9"
$logPath = Join-Path $w9 "reemit-and-spike-wire9.log.txt"

function L([string]$s) {
    $s | Out-File -LiteralPath $logPath -Append -Encoding utf8
}

$repo
L "== REEMIT-AND-SPIKE-WIRE9 =="
L "logPath=$logPath"

$sdk400Root = "C:\Users\SORREN~1\AppData\Local\Temp\opencode\sdk400\dotnet"
$dotnet400 = Join-Path $sdk400Root "dotnet.exe"
$env:DOTNET_ROOT = $sdk400Root
$env:DOTNET_MULTILEVEL_LOOKUP = "0"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$env:PATH = "$sdk400Root;$env:PATH"

$spikePs1 = Join-Path $repo "tools\spikes\Test-JsonEverythingSourceBuild.ps1"
L "spike-ps1-exists=$(Test-Path -LiteralPath $spikePs1)"

$artifactRoot = Join-Path $repo "artifacts\json-everything-source-build"
$sourceRoot = Join-Path $artifactRoot "source"
$spikeInputRoot = Join-Path $repo "spikes\json-everything-source-build"
$inputLockRoot = Join-Path $spikeInputRoot "locks"
$provenancePath = Join-Path $repo "spikes\json-everything-source-build\source-build-provenance.json"

L "dotnet400-exe=$(Test-Path -LiteralPath $dotnet400)"
$v = & $dotnet400 --version 2>&1 | ForEach-Object { $_ }
L "sdk400-version=$(($v -join '').Trim())"

L "sourceRoot-exists=$(Test-Path -LiteralPath $sourceRoot)"
$projects = @(
    @{ label = "Json.More";    proj = "src\Json.More\Json.More.csproj";              lockName = "Json.More.packages.lock.json" }
    @{ label = "JsonPointer";  proj = "src\JsonPointer\JsonPointer.csproj";        lockName = "JsonPointer.packages.lock.json" }
    @{ label = "JsonSchema";   proj = "src\JsonSchema\JsonSchema.csproj";          lockName = "JsonSchema.packages.lock.json" }
)

$success = $true
foreach ($p in $projects) {
    $projAbs = Join-Path $sourceRoot $p.proj
    $projDir = Split-Path -Parent $projAbs
    $srcLock = Join-Path $projDir "packages.lock.json"
    $destLock = Join-Path $inputLockRoot $p.lockName
    L "== $($p.label) =="
    L "  proj-exists=$(Test-Path -LiteralPath $projAbs)"
    if (-not (Test-Path -LiteralPath $projAbs)) { L "  SKIP: proj missing"; $success = $false; continue }

    # remove old lock + old-props artifacts
    if (Test-Path -LiteralPath $srcLock) { Remove-Item -LiteralPath $srcLock -Force }
    if (Test-Path -LiteralPath $destLock) { Remove-Item -LiteralPath $destLock -Force }

    # no-locked restore under SDK .400 re-emits banda .4xx; then copy back to reviewed root
    $props = @(
        "-p:TargetFrameworks=net10.0"
        "-p:RestorePackagesWithLockFile=true"
        "-p:RestoreLockedMode=false"
        "-p:ManagePackageVersionsCentrally=false"
    )
    $out = & $dotnet400 restore $projAbs @props 2>&1 | ForEach-Object { "$_" }
    $restoreExit = $LASTEXITCODE
    L "  restore-exit=$restoreExit"
    $lockNow = Test-Path -LiteralPath $srcLock
    L "  lock-regenerated=$lockNow"
    if ($lockNow) {
        $tg = Select-String -LiteralPath $srcLock -Pattern 'Microsoft.Build.Tasks.Git"\s*:\s*"([^"]+)"' | Select-Object -First 1
        if ($tg) { L "  tasks-git-band=$($tg.Matches[0].Groups[1].Value)" } else { L "  tasks-git-band=none" }
        $band4 = $false
        if ($tg) {
            $ver = $tg.Matches[0].Groups[1].Value
            if ($ver -match '^10\.0\.4') { $band4 = $true }
        }
        L "  band4xx=$band4"
        if ($band4) {
            Copy-Item -LiteralPath $srcLock -Destination $destLock -Force
            L "  copied-back=$($p.lockName)"
        } else {
            L "  NO-copy: lock not band 4xx"
        }
    } else {
        $success = $false
    }
}

L "reemit-ok=$success"
if ($success) {
    L "== SPIKE bajo SDK .400 =="
    Set-Location $repo
    $spikeOut = & powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $spikePs1 -ExpectedSdkVersion "10.0.400" 2>&1 | ForEach-Object { "  spike: $_" }
    $spikeExit = $LASTEXITCODE
    $spikeOut | ForEach-Object { L $_ }
    L "spike-exit=$spikeExit"
}
L "== REEMIT-AND-SPIKE-WIRE9 FIN =="