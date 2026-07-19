param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../.."))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Get-LowerHash {
    param([string]$Path)
    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

$inputRoot = Join-Path $RepositoryRoot "spikes/json-everything-source-build"
$sourceOutput = Join-Path $RepositoryRoot "artifacts/json-everything-source-build/output"
$validatorProject = Join-Path $RepositoryRoot "tools/validators/MuOnline.SchemaValidator/MuOnline.SchemaValidator.csproj"
$validatorLock = Join-Path $RepositoryRoot "tools/validators/MuOnline.SchemaValidator/packages.lock.json"
$publishRoot = Join-Path $RepositoryRoot "artifacts/schema-validator-source-integration"
$provenance = Get-Content -LiteralPath (Join-Path $inputRoot "source-build-provenance.json") -Raw | ConvertFrom-Json
$lock = Get-Content -LiteralPath $validatorLock -Raw | ConvertFrom-Json
$sourceAssemblyNames = @("JsonSchema.Net", "JsonPointer.Net", "Json.More")

$resolvedPackages = @($lock.dependencies.'net10.0'.PSObject.Properties.Name)
if ($resolvedPackages.Count -ne 1 -or $resolvedPackages[0] -ne "Humanizer.Core") {
    throw "The validator lock must resolve only Humanizer.Core; found: $($resolvedPackages -join ', ')."
}

foreach ($assembly in $provenance.assemblies.PSObject.Properties) {
    $assemblyPath = Join-Path $sourceOutput $assembly.Name
    if (-not (Test-Path -LiteralPath $assemblyPath)) {
        throw "Missing source-built assembly: $($assembly.Name)"
    }
    if ((Get-LowerHash $assemblyPath) -ne $assembly.Value) {
        throw "Source-built assembly hash differs from reviewed provenance: $($assembly.Name)"
    }
}

if (Test-Path -LiteralPath $publishRoot) {
    $resolvedPublishRoot = (Resolve-Path -LiteralPath $publishRoot).Path
    $expectedPublishRoot = [System.IO.Path]::GetFullPath($publishRoot)
    if ($resolvedPublishRoot -ne $expectedPublishRoot) {
        throw "Refusing to remove unexpected publication path: $resolvedPublishRoot"
    }
    Remove-Item -LiteralPath $resolvedPublishRoot -Recurse -Force
}

& dotnet publish $validatorProject --configuration Release --no-restore --output $publishRoot
if ($LASTEXITCODE -ne 0) {
    throw "Validator publication failed."
}

$publishedFiles = @(Get-ChildItem -LiteralPath $publishRoot -Recurse -File)
$requiredFiles = @(
    "Json.More.dll",
    "JsonPointer.Net.dll",
    "JsonSchema.Net.dll",
    "Humanizer.dll",
    "JsonEverything-MIT.txt"
)
foreach ($requiredFile in $requiredFiles) {
    if ($requiredFile -notin $publishedFiles.Name) {
        throw "Missing required validator publication file: $requiredFile"
    }
}

$forbiddenFiles = @($publishedFiles | Where-Object {
    $_.Name -eq "OSMFEULA.txt" -or
    $_.Name -match "^(JsonSchema\.Net|JsonPointer\.Net|Json\.More\.Net)\.nuspec$"
})
if ($forbiddenFiles.Count -ne 0) {
    throw "Published-package metadata or OSMFEULA.txt reached the validator publication."
}

$deps = Get-Content -LiteralPath (Join-Path $publishRoot "MuOnline.SchemaValidator.deps.json") -Raw | ConvertFrom-Json
foreach ($sourceAssemblyName in $sourceAssemblyNames) {
    $library = @($deps.libraries.PSObject.Properties | Where-Object { $_.Name -like "$sourceAssemblyName/*" })
    if ($library.Count -ne 1 -or $library[0].Value.type -ne "reference") {
        throw "Json Everything must appear only as a direct assembly reference in deps.json: $sourceAssemblyName"
    }
}

Write-Output "PASS: validator lock resolves only Humanizer.Core 3.0.10."
Write-Output "PASS: 3/3 source-built assembly hashes match reviewed provenance."
Write-Output "PASS: publication contains MIT notice and no OSMFEULA/published-package metadata."
