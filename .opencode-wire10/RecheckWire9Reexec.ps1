$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repo = "F:\Proyecto Builds\Programa"
$w10 = Join-Path $repo ".opencode-wire10"
New-Item -ItemType Directory -Force -Path $w10 | Out-Null
$logPath = Join-Path $w10 "reemit-and-spike-wire9-reexec.log.txt"
Remove-Item -LiteralPath $logPath -ErrorAction SilentlyContinue
function L([string]$s) { $s | Out-File -LiteralPath $logPath -Append -Encoding utf8 }

L "== REEMIT-AND-SPIKE-WIRE9 REEXEC (byte-exacto) =="
L "helper-src-exists=$(Test-Path -LiteralPath (Join-Path $repo ".opencode-wire9\ReemitAndSpikeWire9.ps1"))"
# Copiar helper byte-exacto a path SIN espacios para arranque confiable
$tmpRoot = "C:\Users\SORREN~1\AppData\Local\Temp\opencode\nspc"
New-Item -ItemType Directory -Force -Path $tmpRoot | Out-Null
$helperDst = Join-Path $tmpRoot "ReemitAndSpikeWire9.ps1"
Copy-Item -LiteralPath (Join-Path $repo ".opencode-wire9\ReemitAndSpikeWire9.ps1") -Destination $helperDst -Force
L "helper-dst-exists=$(Test-Path -LiteralPath $helperDst)"
L "helper-src-bytes=$((Get-Item -LiteralPath (Join-Path $repo ".opencode-wire9\ReemitAndSpikeWire9.ps1")).Length)"
L "helper-dst-bytes=$((Get-Item -LiteralPath $helperDst).Length)"

$outF = Join-Path $tmpRoot "wire9-reexec-stdout.txt"
$errF = Join-Path $tmpRoot "wire9-reexec-stderr.txt"
Remove-Item -LiteralPath $outF,$errF -ErrorAction SilentlyContinue

$sdk400Root = "C:\Users\SORREN~1\AppData\Local\Temp\opencode\sdk400\dotnet"
$dotnet400 = Join-Path $sdk400Root "dotnet.exe"
$env:DOTNET_ROOT = $sdk400Root
$env:DOTNET_MULTILEVEL_LOOKUP = "0"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$env:PATH = "$sdk400Root;$env:PATH"

$sourceRoot = Join-Path $repo "artifacts\json-everything-source-build\source"
$lockReviewedRoot = Join-Path $repo "spikes\json-everything-source-build\locks"
$spikePs1 = Join-Path $repo "tools\spikes\Test-JsonEverythingSourceBuild.ps1"

$projects = @(
    [ordered]@{ label = "Json.More";   proj = "src\Json.More\Json.More.csproj";   lockName = "Json.More.packages.lock.json" }
    [ordered]@{ label = "JsonPointer"; proj = "src\JsonPointer\JsonPointer.csproj"; lockName = "JsonPointer.packages.lock.json" }
    [ordered]@{ label = "JsonSchema";  proj = "src\JsonSchema\JsonSchema.csproj";   lockName = "JsonSchema.packages.lock.json" }
)

$restoreProps = @(
    "-p:TargetFrameworks=net10.0"
    "-p:RestorePackagesWithLockFile=true"
    "-p:RestoreLockedMode=false"
    "-p:ManagePackageVersionsCentrally=false"
    "-p:CentralPackageTransitivePinningEnabled=false"
)

$sw = [Diagnostics.Stopwatch]::StartNew()
$p = Start-Process -FilePath powershell.exe -ArgumentList @(
    "-NoProfile","-NonInteractive","-ExecutionPolicy","Bypass",
    "-File", $helperDst
) -RedirectStandardOutput $outF -RedirectStandardError $errF -Wait -PassThru -WindowStyle Hidden
$sw.Stop()
L "wire9-reexec-helper-exit=$($p.ExitCode)"
L "wire9-reexec-elapsed-s=$([math]::Round($sw.Elapsed.TotalSeconds,1))"
L "wire9-reexec-log-bytes=$((Get-Item -LiteralPath (Join-Path $repo ".opencode-wire9\reemit-and-spike-wire9.log.txt") -ErrorAction SilentlyContinue).Length)"
L "== REEMIT-AND-SPIKE-WIRE9 REEXEC FIN =="