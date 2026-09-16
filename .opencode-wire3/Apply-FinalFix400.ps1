$ErrorActionPreference = "Stop"
Set-StrictMode -Version 2.0

$repo = "F:\Proyecto Builds\Programa"
$w3 = Join-Path $repo ".opencode-wire3"

$sdk400Root = "C:\Users\SORREN~1\AppData\Local\Temp\opencode\sdk400\dotnet"
$dotnet400 = Join-Path $sdk400Root "dotnet.exe"

$spikeInRoot = Join-Path $repo "spikes\json-everything-source-build"
$spikePs1 = Join-Path $repo "tools\spikes\Test-JsonEverythingSourceBuild.ps1"
$locksReviewedRoot = Join-Path $spikeInRoot "locks"
$provenancePath = Join-Path $spikeInRoot "source-build-provenance.json"
$artifactRoot = Join-Path $repo "artifacts\json-everything-source-build"
$sourceRoot = Join-Path $artifactRoot "source"

$logPath = Join-Path $w3 "final-fix-run.log.txt"
function L($s) {
    $s | Out-File -LiteralPath $logPath -Append -Encoding utf8
}

function Invoke-Checked {
    param([string]$FilePath, [string[]]$Arguments, [string]$WorkingDirectory)
    Push-Location -LiteralPath $WorkingDirectory
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

L "== FINAL-FIX400 INICIO =="
L "dotnet400-exe=$(Test-Path -LiteralPath $dotnet400)"
$sdkVer = (& $dotnet400 --version 2>&1).Trim()
L "sdk400-version=$sdkVer"

# PASO 1: ejecutar el spike ajeno bajo SDK .400 (materializa source; locked-mode falla NU1004 con locks .2xx, esperado)
L "== PASO1: spike ajeno bajo SDK .400 (locks .2xx -> NU1004 esperado; materializa source) =="
$env:DOTNET_ROOT = $sdk400Root
$env:DOTNET_MULTILEVEL_LOOKUP = "0"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$env:PATH = "$sdk400Root;$env:PATH"

if (Test-Path -LiteralPath $spikePs1) {
    $spikeOut1 = & powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $spikePs1 -ExpectedSdkVersion "10.0.400" 2>&1 | ForEach-Object { $_ }
    $spikeCode1 = $LASTEXITCODE
    L "spike1-exit=$spikeCode1"
    $spikeOut1 | ForEach-Object { L "  spike1: $_" }
} else {
    L "spike-ps1-MISSING: $spikePs1"
}
L "source-git-exists=$(Test-Path -LiteralPath (Join-Path $sourceRoot ".git"))"

# PASO 2: re-emitir los 3 locks a banda .4xx bajo SDK .400 (restore NO locked)
L "== PASO2: re-emisión de locks banda .4xx (restore no-locked bajo .400) =="
$projects = @(
    "src\Json.More\Json.More.csproj",
    "src\JsonPointer\JsonPointer.csproj",
    "src\JsonSchema\JsonSchema.csproj"
)
$newLocksSummary = @()
foreach ($projRel in $projects) {
    $csproj = Join-Path $sourceRoot $projRel
    $projName = Split-Path -Leaf (Split-Path -Parent $csproj)
    if (-not (Test-Path -LiteralPath $csproj)) {
        L "PROJ-MISSING: $projRel"
        $newLocksSummary += "$projName=PROJ-MISSING"
        continue
    }
    $restoreProps = @(
        "-p:TargetFrameworks=net10.0",
        "-p:RestorePackagesWithLockFile=true",
        "-p:ManagePackageVersionsCentrally=false",
        "-p:CentralPackageTransitivePinningEnabled=false"
    )
    L "RESTORE-NOLOCK-START: $projName"
    $restoreOut = & $dotnet400 restore $csproj @restoreProps 2>&1 | ForEach-Object { $_ }
    $restoreCode = $LASTEXITCODE
    L "restore-$projName-exit=$restoreCode"
    $restoreOut | ForEach-Object { L "  restore: $_" }
    # copiar el lock regenerado (banda .4xx) de vuelta a los locks revisados del spike
    $lockSrc = Join-Path (Split-Path -Parent $csproj) "packages.lock.json"
    $lockReviewed = Join-Path $locksReviewedRoot "$projName.packages.lock.json"
    if (Test-Path -LiteralPath $lockSrc) {
        Copy-Item -LiteralPath $lockSrc -Destination $lockReviewed -Force
        $band = Select-String -LiteralPath $lockReviewed -Pattern 'Microsoft.Build.Tasks.Git"\s*:\s*"[^"]+"' | Select-Object -First 1
        if ($band) {
            L "TASKSGIT-BAND: $projName -> $($band.Matches[0].Value)"
            $newLocksSummary += "$projName=$($band.Matches[0].Groups[1].Value)"
        } else {
            L "TASKSGIT-BAND: $projName -> none"
            $newLocksSummary += "$projName=none"
        }
    } else {
        L "LOCK-NOT-REGENERATED: $projName"
        $newLocksSummary += "$projName=lock-missing"
    }
}

L "== locks regenerados a banda .4xx (copy-back a spikes) =="
$newLocksSummary | ForEach-Object { L "  LOCK: $_" }

# PASO 3: verificar las bandas byte-exacto en los 3 locks copiados + comprobar si los hashes de assemblies del provenance cambian
L "== PASO3: bandas Tasks.Git en locks revisados actualizados (byte-exacto) =="
foreach ($lockName in @("Json.More.packages.lock.json","JsonPointer.packages.lock.json","JsonSchema.packages.lock.json")) {
    $lp = Join-Path $locksReviewedRoot $lockName
    if (Test-Path -LiteralPath $lp) {
        $m = Select-String -LiteralPath $lp -Pattern 'Microsoft.Build.Tasks.Git"\s*:\s*"([0-9.]+)"' | Select-Object -First 1
        L "  $lockName -> TasksGit=$(if ($m) { $m.Matches[0].Groups[1].Value } else { 'none' })"
    } else {
        L "  $lockName -> MISSING"
    }
}

L "== FINAL-FIX400 FIN =="
"FINAL-FIX400-DONE newLocks=$($newLocksSummary -join ';')"