$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$schemaRoot = Join-Path $repositoryRoot 'packages/schemas/v1'
$exampleRoot = Join-Path $repositoryRoot 'packages/schemas/examples'
$expectedNames = @(
    'evidence',
    'formula',
    'calculation-trace',
    'formula-test-case',
    'character-class',
    'progression-rule',
    'stat-distribution',
    'build-draft',
    'server-profile',
    'build'
)
$expectedVersions = @{
    'formula' = '1.1.0'
    'stat-distribution' = '1.1.0'
    'build-draft' = '1.1.0'
}

foreach ($name in $expectedNames) {
    $schemaPath = Join-Path $schemaRoot "$name.schema.json"
    $schema = Get-Content -Raw -LiteralPath $schemaPath | ConvertFrom-Json

    if ($schema.'$schema' -ne 'https://json-schema.org/draft/2020-12/schema') {
        throw "$name does not declare JSON Schema 2020-12."
    }
    if ($schema.schemaVersion) {
        throw "$name exposes schemaVersion at the schema root instead of as an instance property."
    }
    $expectedVersion = if ($expectedVersions.ContainsKey($name)) {
        $expectedVersions[$name]
    } else {
        '1.0.0'
    }
    if ($schema.properties.schemaVersion.const -ne $expectedVersion) {
        throw "$name does not constrain schemaVersion to $expectedVersion."
    }
    if (-not $schema.'$id') {
        throw "$name does not declare a schema ID."
    }

    foreach ($kind in @('valid', 'invalid')) {
        $examplePath = Join-Path $exampleRoot "$kind/$name.json"
        $null = Get-Content -Raw -LiteralPath $examplePath | ConvertFrom-Json
    }
}

$exampleCount = $expectedNames.Count * 2
Write-Output "PASS: $($expectedNames.Count) schemas and $exampleCount examples are structurally readable."
