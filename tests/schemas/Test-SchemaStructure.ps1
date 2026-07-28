$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$schemaRoot = Join-Path $repositoryRoot 'packages/schemas'
$exampleRoot = Join-Path $repositoryRoot 'packages/schemas/examples'
$contracts = @(
    @{ Name = 'evidence'; SchemaDirectory = 'v1'; SchemaName = 'evidence'; Version = '1.0.0' },
    @{ Name = 'formula'; SchemaDirectory = 'v1'; SchemaName = 'formula'; Version = '1.1.0' },
    @{ Name = 'formula-v2'; SchemaDirectory = 'v2'; SchemaName = 'formula'; Versions = @('2.0.0', '2.1.0') },
    @{ Name = 'calculation-trace'; SchemaDirectory = 'v1'; SchemaName = 'calculation-trace'; Version = '1.0.0' },
    @{ Name = 'formula-test-case'; SchemaDirectory = 'v1'; SchemaName = 'formula-test-case'; Version = '1.0.0' },
    @{ Name = 'character-class'; SchemaDirectory = 'v1'; SchemaName = 'character-class'; Version = '1.0.0' },
    @{ Name = 'progression-rule'; SchemaDirectory = 'v1'; SchemaName = 'progression-rule'; Version = '1.0.0' },
    @{ Name = 'stat-distribution'; SchemaDirectory = 'v1'; SchemaName = 'stat-distribution'; Version = '1.1.0' },
    @{ Name = 'build-draft'; SchemaDirectory = 'v1'; SchemaName = 'build-draft'; Version = '1.1.0' },
    @{ Name = 'server-profile'; SchemaDirectory = 'v1'; SchemaName = 'server-profile'; Version = '1.0.0' },
    @{ Name = 'build'; SchemaDirectory = 'v1'; SchemaName = 'build'; Version = '1.0.0' }
)

foreach ($contract in $contracts) {
    $name = $contract.Name
    $schemaPath = Join-Path $schemaRoot "$($contract.SchemaDirectory)/$($contract.SchemaName).schema.json"
    $schema = Get-Content -Raw -LiteralPath $schemaPath | ConvertFrom-Json

    if ($schema.'$schema' -ne 'https://json-schema.org/draft/2020-12/schema') {
        throw "$name does not declare JSON Schema 2020-12."
    }
    if ($schema.schemaVersion) {
        throw "$name exposes schemaVersion at the schema root instead of as an instance property."
    }
    if ($contract.Versions) {
        $actualVersions = @($schema.properties.schemaVersion.enum)
        if (($actualVersions -join '|') -ne ($contract.Versions -join '|')) {
            throw "$name does not constrain schemaVersion to $($contract.Versions -join ', ')."
        }
    } else {
        $expectedVersion = $contract.Version
        if ($schema.properties.schemaVersion.const -ne $expectedVersion) {
            throw "$name does not constrain schemaVersion to $expectedVersion."
        }
    }
    if (-not $schema.'$id') {
        throw "$name does not declare a schema ID."
    }

    foreach ($kind in @('valid', 'invalid')) {
        $examplePath = Join-Path $exampleRoot "$kind/$name.json"
        $null = Get-Content -Raw -LiteralPath $examplePath | ConvertFrom-Json
    }
}

$exampleCount = $contracts.Count * 2
Write-Output "PASS: $($contracts.Count) schema contracts and $exampleCount examples are structurally readable."
