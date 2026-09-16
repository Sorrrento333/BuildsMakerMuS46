$ErrorActionPreference = "Stop"
Set-StrictMode -Version 2.0

$repo = "F:\Proyecto Builds\Programa"
$w = Join-Path $repo ".opencode-wire2"
$sdk400Root = "C:\Users\SORREN~1\AppData\Local\Temp\opencode\sdk400\dotnet"
$dotnet400 = Join-Path $sdk400Root "dotnet.exe"

$artifactRoot = Join-Path $repo "artifacts\json-everything-source-build"
$sourceRoot = Join-Path $artifactRoot "source"
$locksInputRoot = Join-Path $repo "spikes\json-everything-source-build\locks"
$provenancePath = Join-Path $repo "spikes\json-everything-source-build\source-build-provenance.json"

$env:DOTNET_ROOT = $sdk400Root
$env:DOTNET_MULTILEVEL_LOOKUP = "0"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:DOTNET_ROOT = $sdk400Root
$env:PATH = "$sdk400Root;$env:PATH"

$logPath = Join-Path $w "recapture400.log.txt"
function L($s) { $s | Out-File -LiteralPath $logPath -Append -Encoding utf8 }

L "== RECAPTURE400 inicio =="
L "sdk400-exe=$(Test-Path -LiteralPath $dotnet400)"
$ver = & $dotnet400 --version 2>&1 | ForEach-Object { $_ }
L "sdk400-version=$($ver -join ';')"

# 1) confirmar source clonado (el spike lo clona)
$srcGit = Test-Path -LiteralPath (Join-Path $sourceRoot ".git")
L "source-git-exists=$srcGit"
if (-not $srcGit) {
    L "ERROR: source no clonado; el spike debe clonar primero (red)."
    exit 2
}

# 2) calcular SDK real requerido por el spike (ExpectedSdkVersion default 10.0.400 ya en el ps1)
#    Re-capturar locks: restore SIN locked-mode bajo SDK .400 en los 3 proyectos
$projects = @(
    "src\Json.More\Json.More.csproj",
    "src\Json.Pointer\Json.Pointer.csproj",
    "src\Json.Schema\Json.Schema.csproj"
)

foreach ($projRel in $projects) {
    $projPath = Join-Path $sourceRoot $projRel
    $projName = Split-Path -Leaf (Split-Path -Parent $projRel)
    if (-not (Test-Path -LiteralPath $projPath)) {
        L "PROJ-MISSING: $projRel"
        continue
    }
    L "RESTORE-NOLOCKED-START: $projRel"
    $restoreOut = & $dotnet400 restore $projPath -p:TargetFrameworks=net10.0 -p:RestorePackagesWithLockFile=true 2>&1 | ForEach-Object { $_ }
    L "restore-exit=$LASTEXITCODE"
    $restoreOut | ForEach-Object { L "  restore: $_" }
    $lockPath = Join-Path (Split-Path -Parent $projPath) "packages.lock.json"
    if (Test-Path -LiteralPath $lockPath) {
        L "LOCK-EXISTS: $lockPath"
        $tasks = Select-String -LiteralPath $lockPath -Pattern 'Microsoft.Build.Tasks.Git"\s*:\s*"[^"]+"' -AllMatches | Select-Object -First 1
        if ($tasks) {
            L "LOCK-TASKSGIT: $($tasks.Matches[0].Value)"
        } else {
            L "LOCK-TASKSGIT: none-found"
        }
    } else {
        L "LOCK-NOT-FOUND-AFTER-RESTORE: $projRel"
    }
}

# 3) ahora el restore locked-mode bajo .400 debe pasar (locks banda .4xx coherentes)
#    verificar con el spike real completo (que es el gate)
L "== verificar: spike real bajo SDK .400 (debe quedar verde) =="
$spike = Join-Path $repo "tools\spikes\Test-JsonEverythingSourceBuild.ps1"
L "spike-ps1-exists=$(Test-Path -LiteralPath $spike)"
if (Test-Path -LiteralPath $spike) {
    $spikeOut = & powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $spike -ExpectedSdkVersion "10.0.400" 2>&1 | ForEach-Object { $_ }
    $spikeCode = $LASTEXITCODE
    L "spike-exit=$spikeCode"
    $spikeOut | ForEach-Object { L "  spike: $_" }
    if ($spikeCode -eq 0) {
        L "SPIKE-GREEN-400: locks re-capturados + provenance emitido byte-exacto"
    } else {
        L "SPIKE-FAIL-400: reintento no resuelve; ver logs"
    }
} else {
    L "ERROR: spike ps1 no existe"
}

L "== RECAPTURE400 fin =="
"RECAPTURE400-DONE"
