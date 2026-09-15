$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repo = "F:\Proyecto Builds\Programa"
$w7 = Join-Path $repo ".opencode-wire7"
New-Item -ItemType Directory -Force -Path $w7 | Out-Null
$logPath = Join-Path $w7 "exec-wire6-helper.log.txt"
Remove-Item -LiteralPath $logPath -ErrorAction SilentlyContinue
function L([string]$s) { $s | Out-File -LiteralPath $logPath -Append -Encoding utf8 }

$helper = Join-Path $repo ".opencode-wire6\ReemitAndSpikeWire6.ps1"
L "== EXEC-WIRE6-HELPER =="
L "helper-exists=$(Test-Path -LiteralPath $helper)"
L "helper-bytes=$((Get-Item -LiteralPath $helper -ErrorAction SilentlyContinue).Length)"

$outF = Join-Path $w7 "w6helper-stdout.txt"
$errF = Join-Path $w7 "w6helper-stderr.txt"
Remove-Item -LiteralPath $outF,$errF -ErrorAction SilentlyContinue

$sw = [Diagnostics.Stopwatch]::StartNew()
$p = Start-Process -FilePath powershell.exe -ArgumentList @(
    "-NoProfile","-NonInteractive","-ExecutionPolicy","Bypass",
    "-File", $helper
) -RedirectStandardOutput $outF -RedirectStandardError $errF -Wait -PassThru -WindowStyle Hidden
$sw.Stop()
L "exec-exit=$($p.ExitCode)"
L "elapsed-s=$([math]::Round($sw.Elapsed.TotalSeconds,1))"
L "stdout-bytes=$((Get-Item -LiteralPath $outF -ErrorAction SilentlyContinue).Length)"
L "stderr-bytes=$((Get-Item -LiteralPath $errF -ErrorAction SilentlyContinue).Length)"
L "== EXEC-WIRE6-HELPER FIN =="