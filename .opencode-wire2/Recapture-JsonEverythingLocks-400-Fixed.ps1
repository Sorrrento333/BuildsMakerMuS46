$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repo = "F:\Proyecto Builds\Programa"
$w = Join-Path $repo ".opencode-wire2"
$sdk400Root = "C:\Users\SORREN~1\AppData\Local\Temp\opencode\sdk400\dotnet"
$dotnet400 = Join-Path $sdk400Root "dotnet.exe"
$artifactRoot = Join-Path $repo "artifacts\json-everything-source-build"
$sourceRoot = Join-Path $artifactRoot "source"
$repoLocksInputRoot = Join-Path $repo "spikes\json-everything-source-build\locks"
$spikePs1 = Join-Path $repo "tools\spikes\Test-JsonEverythingSourceBuild.ps1"

$logPath = Join-Path $w "recapture400-fixed.log.txt"
function L([string]$msg) {
    ("{0:HH:mm:ss.fff} {1}" -f (Get-Date), $msg) | Out-File -LiteralPath $logPath -Append -Encoding utf8
}
L "== RECAPTURE400-FIXED START =="
L "dotnet400-exe=$(Test-Path -LiteralPath $dotnet400)"
L "artifactRoot-exists=$(Test-Path -LiteralPath $artifactRoot)"
L "sourceRoot-exists=$(Test-Path -LiteralPath $sourceRoot)"
L "spike-ps1-exists=$(Test-Path -LiteralPath $spikePs1)"
L "locksInput-count=$((Get-ChildItem -LiteralPath $repoLocksInputRoot -Filter '*.packages.lock.json' -File -ErrorAction SilentlyContinue).Count)"

# Si el source no está clonado, el spike lo clona (necesita red). Forzamos el entorno SDK .400.
$env:DOTNET_ROOT = $sdk400Root
$env:DOTNET_MULTILEVEL_LOOKUP = "0"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:PATH = "$sdk400Root;$($env:PATH)"

$sdkVer = (& $dotnet400 --version 2>&1 | ForEach-Object { $_ })
L "sdk400-version=$($sdkVer -join '|')"

# Ejecutar el spike ajeno REAL bajo SDK .400 — regen locks banda .4xx + verifica provenance
if (Test-Path -LiteralPath $spikePs1) {
    L "== SPIKE400-START (re-captura real) =="
    $spikeOut = & powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $spikePs1 -ExpectedSdkVersion "10.0.400" 2>&1 | ForEach-Object { $_ }
    $spikeExit = $LASTEXITCODE
    L "spike400-exit=$spikeExit"
    $spikeOut | ForEach-Object { L "  spike: $_" }
} else {
    L "ERROR: spike ps1 no encontrado"
}

L "== RECAPTURE400-FIXED END =="
Write-Output "RECAPTURE400-FIXED-DONE"
