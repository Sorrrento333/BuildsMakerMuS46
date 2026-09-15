$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repo = "F:\Proyecto Builds\Programa"
$w = Join-Path $repo ".opencode-wire"
$sdk400Root = "C:\Users\SORREN~1\AppData\Local\Temp\opencode\sdk400\dotnet"
$env:DOTNET_ROOT = $sdk400Root
$env:DOTNET_MULTILEVEL_LOOKUP = "0"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:PATH = "$sdk400Root;$env:PATH"

function Invoke-Checked {
    param([string]$Exe, [string[]]$Args, [string]$Cwd)
    Push-Location -LiteralPath $Cwd
    try {
        & $Exe @Args
        if ($LASTEXITCODE -ne 0) {
            throw "Command failed ($LASTEXITCODE): $Exe $($Args -join ' ')"
        }
    }
    finally {
        Pop-Location
    }
}

$result = [System.Collections.Generic.List[string]]::new()

$sdkVer = (& (Join-Path $sdk400Root "dotnet.exe") --version).Trim()
$result.Add("sdk-version=$sdkVer")

$spikeInput = Join-Path $repo "spikes\json-everything-source-build"
$artifactRoot = Join-Path $repo "artifacts\json-everything-source-build"
$sourceRoot = Join-Path $artifactRoot "source"

if (-not (Test-Path -LiteralPath (Join-Path $sourceRoot ".git"))) {
    $result.Add("source-git=NO (spike no ha clonado aun; ejecutar spike primero para clonar, o abort)")
} else {
    $result.Add("source-git=YES")
    $sdkGit = (& git -C $sourceRoot rev-parse HEAD).Trim()
    $result.Add("source-head=$sdkGit")
    $result.Add("source-exists=True")
    $result.Add("sdk400-exists=True")
}
