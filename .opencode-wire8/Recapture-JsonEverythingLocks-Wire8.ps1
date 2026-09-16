$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repo = "F:\Proyecto Builds\Programa"
$wire = Join-Path $repo ".opencode-wire8"
New-Item -ItemType Directory -Force -Path $wire | Out-Null
$log = Join-Path $wire "recapture8.log.txt"
Remove-Item -LiteralPath $log -ErrorAction SilentlyContinue
function L([string]$s) { $s | Out-File -LiteralPath $log -Append -Encoding utf8 }

$sdk400Root = "C:\Users\SORREN~1\AppData\Local\Temp\opencode\sdk400\dotnet"
$dotnet400 = Join-Path $sdk400Root "dotnet.exe"
$env:DOTNET_ROOT = $sdk400Root
$env:DOTNET_MULTILEVEL_LOOKUP = "0"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$env:PATH = "$sdk400Root;$env:PATH"

$artifactRoot = Join-Path $repo "artifacts\json-everything-source-build"
$sourceRoot = Join-Path $artifactRoot "source"
$lockReviewedRoot = Join-Path $repo "spikes\json-everything-source-build\locks"
$spikePs1 = Join-Path $repo "tools\spikes\Test-JsonEverythingSourceBuild.ps1"

L "== RECAPTURE-WIRE8 =="
L "dotnet400-exe=$(Test-Path -LiteralPath $dotnet400)"
$v = & $dotnet400 --version 2>&1 | ForEach-Object { $_ }
L "sdk400-version=$(($v -join '').Trim())"
L "source-root-exists=$(Test-Path -LiteralPath $sourceRoot)"
$sourceGit = Test-Path -LiteralPath (Join-Path $sourceRoot '.git')
L "source-git-exists=$sourceGit"

$projects = [ordered]@{
    "Json.More"   = @{ proj = "src\Json.More\Json.More.csproj";   lockName = "Json.More.packages.lock.json" }
    "Json.Pointer"= @{ proj = "src\Json.Pointer\Json.Pointer.csproj"; lockName = "JsonPointer.packages.lock.json" }
    "Json.Schema" = @{ proj = "src\Json.Schema\Json.Schema.csproj";   lockName = "JsonSchema.packages.lock.json" }
}

# Real spike props (byte-exacto del spike ajeno leido): CPM off, locked off durante re-emision
$restoreProps = @(
    "-p:TargetFrameworks=net10.0"
    "-p:RestorePackagesWithLockFile=true"
    "-p:RestoreLockedMode=false"
    "-p:ManagePackageVersionsCentrally=false"
    "-p:CentralPackageTransitivePinningEnabled=false"
)

$success = $true
Push-Location $sourceRoot
try {
    foreach ($entry in $projects.GetEnumerator()) {
        $label = $entry.Key
        $projAbs = Join-Path $sourceRoot $entry.Value.proj
        $projDir = Split-Path -Parent $projAbs
        $srcLock = Join-Path $projDir "packages.lock.json"
        $destLock = Join-Path $lockReviewedRoot $entry.Value.lockName
        L "== $label =="
        L "  proj-exists=$(Test-Path -LiteralPath $projAbs)"
        if (-not (Test-Path -LiteralPath $projAbs)) { $success = $false; continue }
        if (Test-Path -LiteralPath $srcLock) { Remove-Item -LiteralPath $srcLock -Force }
        $out = & $dotnet400 restore $projAbs @restoreProps 2>&1 | ForEach-Object { "$_" }
        $exit = $LASTEXITCODE
        L "  restore-exit=$exit"
        foreach ($ln in $out) {
            if ($ln -match 'NU19\d\d|Tasks.Git|vulnerab') { L "  .. $ln" }
        }
        $srcLockNow = Test-Path -LiteralPath $srcLock
        L "  lock-regenerated=$srcLockNow"
        if ($srcLockNow) {
            $tg = Select-String -LiteralPath $srcLock -Pattern 'Microsoft.Build.Tasks.Git"\s*:\s*"[^"]+"' | Select-Object -First 1
            if ($tg) { L "  tasksgit=$($tg.Matches[0].Value)" } else { L "  tasksgit=none" }
            $okBand = $tg -and ($tg.Matches[0].Value -match '10\.0\.4\d\d')
            L "  banda4xx-ok=$okBand"
            if ($okBand) {
                Copy-Item -LiteralPath $srcLock -Destination $destLock -Force
                L "  copied-back-to-reviewed=$($entry.Value.lockName)"
                $tgDest = Select-String -LiteralPath $destLock -Pattern 'Microsoft.Build.Tasks.Git"\s*:\s*"[^"]+"' | Select-Object -First 1
                if ($tgDest) { L "  dest-tasksgit=$($tgDest.Matches[0].Value)" }
            } else {
                $success = $false
                L "  SKIP-copy (Tasks.Git no banda 4xx)"
            }
        } else {
            $success = $false
            L "  SKIP-copy (lock no regenerado)"
        }
    }
}
finally { Pop-Location }

if ($success) {
    L "== SPIKE REAL BAJO SDK400 (expect .400) =="
    $spikeExit = 0
    try {
        $env:PATH = "$sdk400Root;$env:PATH"
        $env:DOTNET_ROOT = $sdk400Root
        & powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $spikePs1 -ExpectedSdkVersion "10.0.400" 2>&1 | ForEach-Object { L "  spike: $_" }
        $spikeExit = $LASTEXITCODE
    } catch { $spikeExit = -1; L "  spike-EX: $($_.Exception.Message)" }
    L "spike-exit=$spikeExit"
    L "spike-green=$($spikeExit -eq 0)"
}

L "recapture-success=$success"
L "== FIN RECAPTURE-WIRE8 =="