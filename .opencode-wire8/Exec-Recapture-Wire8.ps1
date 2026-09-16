$repo = "F:\Proyecto Builds\Programa"
$w8r = Join-Path $repo ".opencode-wire8"
$repoNoDash = "F:\ProyectoBuildsPrograma"
New-Item -ItemType Directory -Force -Path $repoNoDash | Out-Null

$helperRel = "Recapture-JsonEverythingLocks-Wire8.ps1"
$helperSrc = Join-Path $w8r $helperRel
$helperDst = Join-Path $repoNoDash "Recapture-JsonEverythingLocks-Wire8.ps1"
Copy-Item -LiteralPath $helperSrc -Destination $helperDst -Force

$logWire8r = Join-Path $w8r "recapture-wire8-exec.log.txt"
Remove-Item -LiteralPath $logWire8r -ErrorAction SilentlyContinue
function SW([string]$s) { $s | Out-File -LiteralPath $logWire8r -Append -Encoding utf8 }

$dotnet400 = "C:\Users\SORREN~1\AppData\Local\Temp\opencode\sdk400\dotnet\dotnet.exe"
SW "== EXEC RECAPTURE-WIRE8 =="
SW "helper-exists=$(Test-Path -LiteralPath $helperSrc)"
SW "helper-bytes=$((Get-Item -LiteralPath $helperSrc).Length)"
SW "dotnet400-exe=$(Test-Path -LiteralPath $dotnet400)"
$env:PATH = "C:\Users\SORREN~1\AppData\Local\Temp\opencode\sdk400\dotnet;$env:PATH"
$env:DOTNET_ROOT = "C:\Users\SORREN~1\AppData\Local\Temp\opencode\sdk400\dotnet"
$env:DOTNET_MULTILEVEL_LOOKUP = "0"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$outF = Join-Path $w8r "recapture-wire8-stdout.txt"
$errF = Join-Path $w8r "recapture-wire8-stderr.txt"
Remove-Item -LiteralPath $outF,$errF -ErrorAction SilentlyContinue
$swch = [Diagnostics.Stopwatch]::StartNew()
$p = Start-Process -FilePath powershell.exe -ArgumentList @(
    "-NoProfile","-NonInteractive","-ExecutionPolicy","Bypass",
    "-File", (Join-Path $repoNoDash "Recapture-JsonEverythingLocks-Wire8.ps1")
) -RedirectStandardOutput $outF -RedirectStandardError $errF -Wait -PassThru -WindowStyle Hidden
$swch.Stop()
SW "helper-exit=$($p.ExitCode)"
SW "helper-elapsed-s=$([math]::Round($swch.Elapsed.TotalSeconds,1))"
SW "stdout-bytes=$(Get-Item -LiteralPath $outF -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Length)"
SW "stderr-bytes=$(Get-Item -LiteralPath $errF -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Length)"
$log200 = Join-Path $repo "artifacts\json-everything-source-build\recapture-locks-wire8.log.txt"
SW "wire8-log-exists=$(Test-Path -LiteralPath $log200)"
SW "wire8-log-bytes=$((Get-Item -LiteralPath $log200 -ErrorAction SilentlyContinue).Length)"
SW "== FIN EXEC-RECAPTURE-WIRE8 =="