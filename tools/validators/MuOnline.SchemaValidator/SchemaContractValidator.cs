using System.Text.Json;
using Json.Schema;

namespace MuOnline.SchemaValidator;

public sealed record FixtureValidationResult(
    string ContractName,
    string FixtureKind,
    bool ExpectedValidity,
    bool ActualValidity,
    string SchemaPath,
    string FixturePath)
{
    public bool MatchesExpectation => ExpectedValidity == ActualValidity;
}

public sealed record RulesetRecordValidationResult(
    string ContractName,
    string RecordId,
    bool ActualValidity,
    string SchemaPath,
    string RecordPath);

public static class SchemaContractValidator
{
    private static readonly string[] ContractNames =
    [
        "evidence",
        "formula",
        "character-class",
        "progression-rule",
        "stat-distribution",
        "server-profile",
        "build",
    ];

    public static IReadOnlyList<FixtureValidationResult> ValidateRepository(string repositoryRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryRoot);

        var fullRoot = Path.GetFullPath(repositoryRoot);
        var schemaRoot = Path.Combine(fullRoot, "packages", "schemas", "v1");
        var fixtureRoot = Path.Combine(fullRoot, "packages", "schemas", "examples");
        var results = new List<FixtureValidationResult>(ContractNames.Length * 2);
        var buildOptions = new BuildOptions
        {
            SchemaRegistry = new SchemaRegistry(),
        };

        foreach (var contractName in ContractNames)
        {
            var schemaPath = Path.Combine(schemaRoot, $"{contractName}.schema.json");
            EnsureFileExists(schemaPath, "Schema");
            var schema = JsonSchema.FromFile(schemaPath, buildOptions);

            results.Add(ValidateFixture(
                contractName,
                "valid",
                expectedValidity: true,
                schemaPath,
                schema,
                Path.Combine(fixtureRoot, "valid", $"{contractName}.json")));
            results.Add(ValidateFixture(
                contractName,
                "invalid",
                expectedValidity: false,
                schemaPath,
                schema,
                Path.Combine(fixtureRoot, "invalid", $"{contractName}.json")));
        }

        return results;
    }

    public static IReadOnlyList<RulesetRecordValidationResult> ValidateRulesetRecords(
        string repositoryRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryRoot);

        var fullRoot = Path.GetFullPath(repositoryRoot);
        var schemaRoot = Path.Combine(fullRoot, "packages", "schemas", "v1");
        var rulesetRoot = Path.Combine(
            fullRoot,
            "packages",
            "rulesets",
            "mu-s4-global-reference",
            "v1");
        var recordSets = new[]
        {
            (ContractName: "character-class", DirectoryName: "character-classes"),
            (ContractName: "progression-rule", DirectoryName: "progression-rules"),
        };
        var results = new List<RulesetRecordValidationResult>();

        foreach (var recordSet in recordSets)
        {
            var schemaPath = Path.Combine(
                schemaRoot,
                $"{recordSet.ContractName}.schema.json");
            EnsureFileExists(schemaPath, "Schema");
            var schema = JsonSchema.FromFile(
                schemaPath,
                new BuildOptions { SchemaRegistry = new SchemaRegistry() });
            var recordDirectory = Path.Combine(rulesetRoot, recordSet.DirectoryName);

            if (!Directory.Exists(recordDirectory))
            {
                throw new DirectoryNotFoundException(
                    $"Ruleset record directory was not found: {recordDirectory}");
            }

            foreach (var recordPath in Directory.GetFiles(recordDirectory, "*.json")
                         .Order(StringComparer.Ordinal))
            {
                using var instance = JsonDocument.Parse(File.ReadAllText(recordPath));
                var evaluation = schema.Evaluate(
                    instance.RootElement,
                    new EvaluationOptions
                    {
                        OutputFormat = OutputFormat.List,
                        RequireFormatValidation = true,
                    });
                var recordId = instance.RootElement.TryGetProperty("id", out var idElement)
                    ? idElement.GetString() ?? Path.GetFileNameWithoutExtension(recordPath)
                    : Path.GetFileNameWithoutExtension(recordPath);

                results.Add(new RulesetRecordValidationResult(
                    recordSet.ContractName,
                    recordId,
                    evaluation.IsValid,
                    schemaPath,
                    recordPath));
            }
        }

        return results;
    }

    private static FixtureValidationResult ValidateFixture(
        string contractName,
        string fixtureKind,
        bool expectedValidity,
        string schemaPath,
        JsonSchema schema,
        string fixturePath)
    {
        EnsureFileExists(fixturePath, "Fixture");

        using var instance = JsonDocument.Parse(File.ReadAllText(fixturePath));
        var evaluation = schema.Evaluate(
            instance.RootElement,
            new EvaluationOptions
            {
                OutputFormat = OutputFormat.List,
                RequireFormatValidation = true,
            });

        return new FixtureValidationResult(
            contractName,
            fixtureKind,
            expectedValidity,
            evaluation.IsValid,
            schemaPath,
            fixturePath);
    }

    private static void EnsureFileExists(string path, string kind)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"{kind} file was not found.", path);
        }
    }
}
