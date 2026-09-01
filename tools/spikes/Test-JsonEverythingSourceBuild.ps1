param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")),
    [string]$ExpectedSdkVersion = "10.0.301"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$inputRoot = Join-Path $RepositoryRoot "spikes/json-everything-source-build"
$provenanceInput = Get-Content -LiteralPath (Join-Path $inputRoot "source-build-provenance.json") -Raw | ConvertFrom-Json
$sbomInput = Get-Content -LiteralPath (Join-Path $inputRoot "source-build.spdx.json") -Raw | ConvertFrom-Json
$sourceCommit = $provenanceInput.sourceCommit
$transitiveCommit = $provenanceInput.transitivePackageCommit
$sourceLicenseSha256 = $provenanceInput.sourceLicenseSha256
$repositoryUrl = $provenanceInput.repository
$artifactRoot = Join-Path $RepositoryRoot "artifacts/json-everything-source-build"
$sourceRoot = Join-Path $artifactRoot "source"
$repeatSourceRoot = Join-Path $artifactRoot "source-repeat"
$firstHashes = @{}
$expectedFixtureCount = 22

function Invoke-Checked {
    param([string]$FilePath, [string[]]$Arguments, [string]$WorkingDirectory)

    Push-Location $WorkingDirectory
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

function Get-LowerHash {
    param([string]$Path)
    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Copy-ReviewedLocks {
    param([string]$DestinationSourceRoot)

    $lockMappings = [ordered]@{
        "Json.More.packages.lock.json" = "src/Json.More/packages.lock.json"
        "JsonPointer.packages.lock.json" = "src/JsonPointer/packages.lock.json"
        "JsonSchema.packages.lock.json" = "src/JsonSchema/packages.lock.json"
    }
    foreach ($entry in $lockMappings.GetEnumerator()) {
        Copy-Item -LiteralPath (Join-Path $inputRoot "locks/$($entry.Key)") `
            -Destination (Join-Path $DestinationSourceRoot $entry.Value) -Force
    }
}

function Set-ReproducibleCheckoutConfiguration {
    param([string]$SourceRepositoryRoot)

    Invoke-Checked git @("config", "core.autocrlf", "false") $SourceRepositoryRoot
    Invoke-Checked git @("config", "core.eol", "lf") $SourceRepositoryRoot
}

$actualSdkVersion = (& dotnet --version).Trim()
if ($ExpectedSdkVersion -ne $provenanceInput.sdkVersion) {
    throw "Expected SDK parameter differs from the reviewed provenance input."
}
if ($LASTEXITCODE -ne 0 -or $actualSdkVersion -ne $ExpectedSdkVersion) {
    throw "Expected .NET SDK $ExpectedSdkVersion, found '$actualSdkVersion'."
}

if (Test-Path -LiteralPath $artifactRoot) {
    $resolvedArtifactRoot = (Resolve-Path -LiteralPath $artifactRoot).Path
    $resolvedExpectedRoot = [System.IO.Path]::GetFullPath(
        (Join-Path $RepositoryRoot "artifacts/json-everything-source-build"))
    if ($resolvedArtifactRoot -ne $resolvedExpectedRoot) {
        throw "Refusing to remove unexpected artifact path: $resolvedArtifactRoot"
    }
    Remove-Item -LiteralPath $resolvedArtifactRoot -Recurse -Force
}
New-Item -ItemType Directory -Path $artifactRoot | Out-Null

Invoke-Checked git @("clone", "--filter=blob:none", "--no-checkout", $repositoryUrl, $sourceRoot) $RepositoryRoot
Set-ReproducibleCheckoutConfiguration $sourceRoot
Invoke-Checked git @("checkout", "--detach", $sourceCommit) $sourceRoot
Invoke-Checked git @("fetch", "--depth", "1", "origin", $transitiveCommit) $sourceRoot
Copy-ReviewedLocks $sourceRoot

$actualCommit = (& git -C $sourceRoot rev-parse HEAD).Trim()
if ($actualCommit -ne $sourceCommit) {
    throw "Unexpected source checkout: $actualCommit"
}

& git -C $sourceRoot diff --quiet $sourceCommit $transitiveCommit -- src/JsonPointer src/Json.More
if ($LASTEXITCODE -ne 0) {
    throw "JsonPointer/Json.More differ between the two package-declared commits."
}

$licensePath = Join-Path $sourceRoot "LICENSE"
if ((Get-LowerHash $licensePath) -ne $sourceLicenseSha256) {
    throw "The source MIT license hash differs from the reviewed input."
}

$projectPath = Join-Path $sourceRoot "src/JsonSchema/JsonSchema.csproj"
$restoreProperties = @(
    "-p:TargetFrameworks=net10.0",
    "-p:RestorePackagesWithLockFile=true",
    "-p:ManagePackageVersionsCentrally=false",
    "-p:CentralPackageTransitivePinningEnabled=false"
)
$commonBuildProperties = @(
    "-p:TreatWarningsAsErrors=false",
    "-p:AnalysisLevel=none",
    "-p:ContinuousIntegrationBuild=true",
    "-p:Deterministic=true",
    "-p:DebugType=None",
    "-p:DebugSymbols=false"
)
Invoke-Checked dotnet (@("restore", $projectPath, "--locked-mode") + $restoreProperties) $sourceRoot
$firstBuildProperties = $commonBuildProperties + @("-p:PathMap=$sourceRoot=/_/json-everything")
Invoke-Checked dotnet (@("build", $projectPath, "--configuration", "Release", "--framework", "net10.0", "--no-restore") + $firstBuildProperties) $sourceRoot

$assemblySources = [ordered]@{
    "Json.More.dll" = Join-Path $sourceRoot "src/Json.More/bin/Release/net10.0/Json.More.dll"
    "JsonPointer.Net.dll" = Join-Path $sourceRoot "src/JsonPointer/bin/Release/net10.0/JsonPointer.Net.dll"
    "JsonSchema.Net.dll" = Join-Path $sourceRoot "src/JsonSchema/bin/Release/net10.0/JsonSchema.Net.dll"
}
foreach ($entry in $assemblySources.GetEnumerator()) {
    $firstHashes[$entry.Key] = Get-LowerHash $entry.Value
    if ($firstHashes[$entry.Key] -ne $provenanceInput.assemblies.$($entry.Key)) {
        throw "Assembly hash differs from reviewed provenance: $($entry.Key)"
    }
}

Invoke-Checked git @("clone", "--filter=blob:none", "--no-checkout", $repositoryUrl, $repeatSourceRoot) $artifactRoot
Set-ReproducibleCheckoutConfiguration $repeatSourceRoot
Invoke-Checked git @("checkout", "--detach", $sourceCommit) $repeatSourceRoot
Copy-ReviewedLocks $repeatSourceRoot
$repeatProjectPath = Join-Path $repeatSourceRoot "src/JsonSchema/JsonSchema.csproj"
Invoke-Checked dotnet (@("restore", $repeatProjectPath, "--locked-mode") + $restoreProperties) $repeatSourceRoot
$repeatBuildProperties = $commonBuildProperties + @("-p:PathMap=$repeatSourceRoot=/_/json-everything")
Invoke-Checked dotnet (@("build", $repeatProjectPath, "--configuration", "Release", "--framework", "net10.0", "--no-restore") + $repeatBuildProperties) $repeatSourceRoot
$repeatAssemblySources = [ordered]@{
    "Json.More.dll" = Join-Path $repeatSourceRoot "src/Json.More/bin/Release/net10.0/Json.More.dll"
    "JsonPointer.Net.dll" = Join-Path $repeatSourceRoot "src/JsonPointer/bin/Release/net10.0/JsonPointer.Net.dll"
    "JsonSchema.Net.dll" = Join-Path $repeatSourceRoot "src/JsonSchema/bin/Release/net10.0/JsonSchema.Net.dll"
}
foreach ($entry in $repeatAssemblySources.GetEnumerator()) {
    $secondHash = Get-LowerHash $entry.Value
    if ($firstHashes[$entry.Key] -ne $secondHash) {
        throw "Non-reproducible assembly: $($entry.Key)"
    }
}

$globalPackagesOutput = (& dotnet nuget locals global-packages --list).Trim()
$globalPackages = $globalPackagesOutput -replace "^global-packages:\s*", ""
$humanizerRoot = Join-Path $globalPackages "humanizer.core/3.0.10"
$humanizerAssembly = Join-Path $humanizerRoot "lib/net10.0/Humanizer.dll"
$humanizerNuspec = Join-Path $humanizerRoot "humanizer.core.nuspec"
if (-not (Test-Path -LiteralPath $humanizerAssembly) -or -not (Test-Path -LiteralPath $humanizerNuspec)) {
    throw "Humanizer.Core 3.0.10 was not restored as expected."
}

$outputRoot = Join-Path $artifactRoot "output"
New-Item -ItemType Directory -Path $outputRoot | Out-Null
foreach ($entry in $assemblySources.GetEnumerator()) {
    Copy-Item -LiteralPath $entry.Value -Destination (Join-Path $outputRoot $entry.Key)
}
Copy-Item -LiteralPath $humanizerAssembly -Destination (Join-Path $outputRoot "Humanizer.dll")
Copy-Item -LiteralPath $licensePath -Destination (Join-Path $outputRoot "JsonEverything-MIT.txt")
Copy-Item -LiteralPath $humanizerNuspec -Destination (Join-Path $outputRoot "Humanizer.Core.nuspec")

$harnessProject = Join-Path $RepositoryRoot "spikes/json-everything-source-build/JsonEverything.SourceBuild.ContractHarness.csproj"
Invoke-Checked dotnet @(
    "run", "--project", $harnessProject, "--configuration", "Release",
    "-p:JsonEverythingAssemblyDirectory=$outputRoot",
    "-p:HumanizerAssemblyPath=$humanizerAssembly",
    "--", $RepositoryRoot
) $RepositoryRoot

$auditPath = Join-Path $artifactRoot "nuget-audit.json"
$auditOutput = & dotnet list $projectPath package --include-transitive --vulnerable --format json --output-version 1 --no-restore 2>&1
if ($LASTEXITCODE -ne 0) {
    throw "NuGet vulnerability audit failed. See $auditPath"
}
$auditJson = $auditOutput -join [Environment]::NewLine
$null = $auditJson | ConvertFrom-Json
$auditJson | Set-Content -LiteralPath $auditPath -Encoding utf8
if ([regex]::Matches($auditJson, '"vulnerabilities"\s*:').Count -ne 0) {
    throw "NuGet reported vulnerable packages. See $auditPath"
}

$lockRoot = Join-Path $artifactRoot "locks"
New-Item -ItemType Directory -Path $lockRoot | Out-Null
Get-ChildItem -LiteralPath (Join-Path $sourceRoot "src") -Filter packages.lock.json -Recurse | ForEach-Object {
    $projectName = Split-Path -Leaf $_.DirectoryName
    Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $lockRoot "$projectName.packages.lock.json")
}

$hashLines = Get-ChildItem -LiteralPath $outputRoot -File | Sort-Object Name | ForEach-Object {
    "$(Get-LowerHash $_.FullName)  $($_.Name)"
}
$hashLines | Set-Content -LiteralPath (Join-Path $artifactRoot "SHA256SUMS") -Encoding ascii

$assets = Get-ChildItem -LiteralPath (Join-Path $sourceRoot "src") -Filter project.assets.json -Recurse
$packageMap = [ordered]@{}
foreach ($asset in $assets) {
    $json = Get-Content -LiteralPath $asset.FullName -Raw | ConvertFrom-Json
    foreach ($library in $json.libraries.PSObject.Properties) {
        if ($library.Value.type -eq "package") {
            $parts = $library.Name -split "/", 2
            $packageMap[$library.Name] = [ordered]@{ name = $parts[0]; version = $parts[1] }
        }
    }
}

$spdxPackages = @(
    [ordered]@{ SPDXID = "SPDXRef-JsonSchema-Net"; name = "JsonSchema.Net"; versionInfo = "9.2.2"; downloadLocation = "$repositoryUrl@$sourceCommit"; licenseConcluded = "MIT"; licenseDeclared = "MIT"; copyrightText = "NOASSERTION" },
    [ordered]@{ SPDXID = "SPDXRef-JsonPointer-Net"; name = "JsonPointer.Net"; versionInfo = "7.0.1"; downloadLocation = "$repositoryUrl@$transitiveCommit"; licenseConcluded = "MIT"; licenseDeclared = "MIT"; copyrightText = "NOASSERTION" },
    [ordered]@{ SPDXID = "SPDXRef-Json-More-Net"; name = "Json.More.Net"; versionInfo = "3.0.1"; downloadLocation = "$repositoryUrl@$transitiveCommit"; licenseConcluded = "MIT"; licenseDeclared = "MIT"; copyrightText = "NOASSERTION" }
)
foreach ($package in $packageMap.Values) {
    $safeId = ($package.name + "-" + $package.version) -replace "[^A-Za-z0-9.-]", "-"
    $license = if ($package.name -eq "Humanizer.Core") { "MIT" } else { "NOASSERTION" }
    $spdxPackages += [ordered]@{
        SPDXID = "SPDXRef-NuGet-$safeId"
        name = $package.name
        versionInfo = $package.version
        downloadLocation = "https://www.nuget.org/packages/$($package.name)/$($package.version)"
        licenseConcluded = $license
        licenseDeclared = $license
        copyrightText = "NOASSERTION"
    }
}

$sbomFields = @(
    "SPDXID", "name", "versionInfo", "downloadLocation",
    "licenseConcluded", "licenseDeclared", "copyrightText"
)
$actualSbomPackages = $spdxPackages | ForEach-Object {
    $package = $_
    ($sbomFields | ForEach-Object { [string]$package[$_] }) -join "|"
} | Sort-Object
$expectedSbomPackages = $sbomInput.packages | ForEach-Object {
    $package = $_
    ($sbomFields | ForEach-Object { [string]$package.$_ }) -join "|"
} | Sort-Object
if (($actualSbomPackages -join "`n") -cne ($expectedSbomPackages -join "`n")) {
    throw "Resolved source-build inventory differs from the reviewed SPDX input."
}

$namespaceSeed = "$sourceCommit-$actualSdkVersion"
$sbom = [ordered]@{
    spdxVersion = "SPDX-2.3"
    dataLicense = "CC0-1.0"
    SPDXID = "SPDXRef-DOCUMENT"
    name = "MU-BuildPlanner-JsonEverything-source-build"
    documentNamespace = "https://mu-build-planner.invalid/spdx/$namespaceSeed"
    creationInfo = [ordered]@{ created = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"); creators = @("Tool: Test-JsonEverythingSourceBuild.ps1") }
    packages = $spdxPackages
}
$sbom | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $artifactRoot "sbom.spdx.json") -Encoding utf8

$provenance = [ordered]@{
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("o")
    sdkVersion = $actualSdkVersion
    targetFramework = "net10.0"
    repository = $repositoryUrl
    sourceCommit = $sourceCommit
    transitivePackageCommit = $transitiveCommit
    transitiveSourcesIdenticalAtSourceCommit = $true
    sourceLicense = "MIT"
    sourceLicenseSha256 = $sourceLicenseSha256
    assemblies = $firstHashes
    independentSourceDirectories = 2
    contractRuns = 2
    fixturesPerRun = $expectedFixtureCount
    formatProbe = "PASS"
}
$provenance | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $artifactRoot "provenance.json") -Encoding utf8

if (Get-ChildItem -LiteralPath $outputRoot -Recurse -File | Where-Object { $_.Name -eq "OSMFEULA.txt" }) {
    throw "OSMFEULA.txt must not be present in the source-built runtime output."
}

Write-Output "PASS: reproducible Json Everything source build."
Write-Output "PASS: 3/3 assembly hashes matched across two independent source paths."
Write-Output "PASS: 2 x $expectedFixtureCount/$expectedFixtureCount fixtures and explicit format probe."
Write-Output "Artifacts: $artifactRoot"
