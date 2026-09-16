$ErrorActionPreference = "Stop"

$repo = "F:\Proyecto Builds\Programa"
$artifactRoot = Join-Path $repo "artifacts\json-everything-source-build"
$provenancePath = Join-Path $artifactRoot "source-build-provenance.json"
$sourceRoot = Join-Path $artifactRoot "source"
$inputRoot = Join-Path $repo "spikes\json-everything-source-build"
$outputRoot = Join-Path $artifactRoot "output"

$summaryPath = "C:\Users\SORREN~1\AppData\Local\Temp\opencode\nspc\spike-summary.txt"
Remove-Item -LiteralPath $summaryPath -ErrorAction SilentlyContinue
function L([string]$s) { $s | Out-File -LiteralPath $summaryPath -Append -Encoding utf8 }

L "== SPIKE-AJENO-REAL BAJO SDK .401 (valor=true de usuario) =="
L "dotnet=$(Get-Command dotnet.exe -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source)"
$env:DOTNET_MULTILEVEL_LOOKUP = "0"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$env:NUGET_XMLDOC_MODE = "skip"

$dotnetCommand = (Get-Command dotnet.exe -ErrorAction SilentlyContinue).Source
L "dotnet-exe=$dotnetCommand"
if ($dotnetCommand) {
    $v = & $dotnetCommand --version 2>&1 | ForEach-Object { "$_" }
    L "dotnet-version=$($v -join ' ') exit=$LASTEXITCODE"
}

$provenance = Get-Content -LiteralPath $provenancePath -Raw | ConvertFrom-Json
$sdkVersion = $provenance.sdkVersion
$sourceCommit = $provenance.sourceCommit
L "provenance-sdk=$sdkVersion sourceCommit=$sourceCommit"

$expectedSdk = "10.0.401"
$provenance | Add-Member -NotePropertyName ExpectedSdkVersion -NotePropertyValue $expectedSdk -Force

$spikePath = Join-Path $repo "tools\spikes\Test-JsonEverythingSourceBuild.ps1"
L "spike-exists=$(Test-Path -LiteralPath $spikePath)"

$env:ExpectedSdkVersion = $expectedSdk
& $dotnetCommand $spikePath -ExpectedSdkVersion $expectedSdk 2>&1 | ForEach-Object { L "spike-out> $_" }
$spikeExit = $LASTEXITCODE
L "spike-exit=$spikeExit"

if ($spikeExit -eq 0) {
    if (Test-Path -LiteralPath (Join-Path $repo "artifacts\json-everything-source-build\provenance.json")) {
        L "spike-provenance-json-exists=True"
    }
    $shaPath = Join-Path $artifactRoot "SHA256SUMS"
    if (Test-Path -LiteralPath $shaPath) {
        L "sha256sums-lines=$((Get-Content -LiteralPath $shaPath).Count)"
        Get-Content -LiteralPath $shaPath | ForEach-Object { L "  sha> $_" }
    }
}

L "spike-green=$($spikeExit -eq 0)"
L "== FIN SPIKE-AJENO-REAL =="
