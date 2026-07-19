$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$schemaRoot = Join-Path $repositoryRoot 'packages/schemas/v1'
$exampleRoot = Join-Path $repositoryRoot 'packages/schemas/examples'
$expectedNames = @('evidence', 'formula', 'character-class', 'server-profile', 'build')

foreach ($name in $expectedNames) {
    $schemaPath = Join-Path $schemaRoot "$name.schema.json"
    $schema = Get-Content -Raw -LiteralPath $schemaPath | ConvertFrom-Json

    if ($schema.'$schema' -ne 'https://json-schema.org/draft/2020-12/schema') {
        throw "$name does not declare JSON Schema 2020-12."
    }
    if ($schema.schemaVersion) {
        throw "$name exposes schemaVersion at the schema root instead of as an instance property."
    }
    if ($schema.properties.schemaVersion.const -ne '1.0.0') {
        throw "$name does not constrain schemaVersion to 1.0.0."
    }
    if (-not $schema.'$id') {
        throw "$name does not declare a schema ID."
    }

    foreach ($kind in @('valid', 'invalid')) {
        $examplePath = Join-Path $exampleRoot "$kind/$name.json"
        $null = Get-Content -Raw -LiteralPath $examplePath | ConvertFrom-Json
    }
}

Write-Output "PASS: $($expectedNames.Count) schemas and 10 examples are structurally readable."
