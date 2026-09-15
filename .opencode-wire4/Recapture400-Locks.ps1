$ErrorActionPreference = "Stop"
Set-StrictMode -Version 2.0

$repo = "F:\Proyecto Builds\Programa"
$w4 = Join-Path $repo ".opencode-wire4"
New-Item -ItemType Directory -Force -Path $w4 -ErrorAction SilentlyContinue | Out-Null
$logPath = Join-Path $w4 "recapture400-locks-summary.txt"

$dotnet400 = "C:\Users\SORREN~1\AppData\Local\Temp\opencode\sdk400\dotnet\dotnet.exe"

$env:DOTNET_ROOT = Split-Path -Parent $dotnet400
$env:DOTNET_MULTILEVEL_LOOKUP = "0"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$env:PATH = "$(Split-Path -Parent $dotnet400);$env:PATH"

function L($s) {
    $s | Out-File -LiteralPath $logPath -Append -Encoding utf8
}
function Add-LockBandInfo {
    param([string]$lockPath, [string]$label)
    if (-not (Test-Path -LiteralPath $lockPath)) {
        L "  $label-LOCK-MISSING"
        return
    }
    $m = Select-String -LiteralPath $lockPath -Pattern 'Microsoft.Build.Tasks.Git"\s*:\s*"[^"]+"' -AllMatches | Select-Object -First 1
    if ($m) {
        L "  $label-TASKSGIT=$($m.Matches[0].Value)"
    } else {
        L "  $label-TASKSGIT=none-found"
    }
}

L "== RECAPTURE400-LOCKS inicio =="
L "dotnet400-exe=$(Test-Path -LiteralPath $dotnet400)"
$ver = & $dotnet400 --version 2>&1 | ForEach-Object { $_ }
$sdkVersion = ($ver -join '').Trim()
L "sdk400-version=$sdkVersion"

$repoSpikeRoot = Join-Path $repo "spikes\json-everything-source-build"
$spikePs1 = Join-Path $repo "tools\spikes\Test-JsonEverythingSourceBuild.ps1"
L "spike-ps1-exists=$(Test-Path -LiteralPath $spikePs1)"
$expectedSpikeSdk = Select-String -LiteralPath $spikePs1 -Pattern 'ExpectedSdkVersion\s*=\s*"([^"]+)"' | Select-Object -First 1
$expectedSdkValues = @($expectedSpikeSdk | ForEach-Object { $_.Matches[0].Groups[1].Value })
L "spike-expected-sdk-values=$($expectedSdkValues -join ',')"

# Verificar: el spike EJECUTADO bajo SDK .400 con locks revisados (banda .2xx) -> NU1004 (locks incoherentes)
# porque los locks revisados fijan banda Tasks.Git .2xx pero SDK .400 resuelve banda .4xx.
# El fix = re-emitir los 3 locks a banda .4xx y re-verificar que los hashes de provenance NO cambian.

# Diagnóstico byte-exacto: locks revisados del spike ajeno (banda actual)
$locksReviewedRoot = Join-Path $repoSpikeRoot "locks"
L "locks-reviewed-root-exists=$(Test-Path -LiteralPath $locksReviewedRoot)"
Get-ChildItem -LiteralPath $locksReviewedRoot -Filter '*.packages.lock.json' -File -ErrorAction SilentlyContinue | ForEach-Object {
    Add-LockBandInfo $_.FullName $_.Name
}

# ¿El spike ya corrió el restore cerca? Artifacts del spike
$artifactRoot = Join-Path $repo "artifacts\json-everything-source-build"
L "artifact-root-exists=$(Test-Path -LiteralPath $artifactRoot)"
$sourceRoot = Join-Path $artifactRoot "source"
L "source-root-git-exists=$(Test-Path -LiteralPath (Join-Path $sourceRoot '.git'))"

# Si el source NO está materializado, el spike lo clona (necesita red). Probe red byte-exacto:
$netProbe = Test-NetConnection -ComputerName github.com -Port 443 -WarningAction SilentlyContinue
L "net-github-443=$(if ($netProbe.TcpTestSucceeded) { 'OK' } else { 'FAIL' })"
L "== RECAPTURE400-LOCKS fin =="
"RECAPTURE400-DONE"