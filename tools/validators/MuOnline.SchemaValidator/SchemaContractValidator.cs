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

public static class SchemaContractValidator
{
    private static readonly string[] ContractNames =
    [
        "evidence",
        "formula",
        "character-class",
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
