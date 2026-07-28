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
    string RecordVersion,
    string SchemaVersion,
    bool ActualValidity,
    string SchemaPath,
    string RecordPath);

public static class SchemaContractValidator
{
    private sealed record ContractDefinition(
        string Name,
        string SchemaVersionDirectory,
        string SchemaFileName,
        string FixtureFileName);

    private static readonly ContractDefinition[] Contracts =
    [
        new("evidence", "v1", "evidence", "evidence"),
        new("formula", "v1", "formula", "formula"),
        new("formula-v2", "v2", "formula", "formula-v2"),
        new("calculation-trace", "v1", "calculation-trace", "calculation-trace"),
        new("formula-test-case", "v1", "formula-test-case", "formula-test-case"),
        new("character-class", "v1", "character-class", "character-class"),
        new("progression-rule", "v1", "progression-rule", "progression-rule"),
        new("stat-distribution", "v1", "stat-distribution", "stat-distribution"),
        new("build-draft", "v1", "build-draft", "build-draft"),
        new("server-profile", "v1", "server-profile", "server-profile"),
        new("build", "v1", "build", "build"),
    ];

    public static IReadOnlyList<FixtureValidationResult> ValidateRepository(string repositoryRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryRoot);

        var fullRoot = Path.GetFullPath(repositoryRoot);
        var schemaRoot = Path.Combine(fullRoot, "packages", "schemas");
        var fixtureRoot = Path.Combine(fullRoot, "packages", "schemas", "examples");
        var results = new List<FixtureValidationResult>(Contracts.Length * 2);
        var buildOptions = new BuildOptions
        {
            SchemaRegistry = new SchemaRegistry(),
        };

        foreach (var contract in Contracts)
        {
            var schemaPath = Path.Combine(
                schemaRoot,
                contract.SchemaVersionDirectory,
                $"{contract.SchemaFileName}.schema.json");
            EnsureFileExists(schemaPath, "Schema");
            var schema = JsonSchema.FromFile(schemaPath, buildOptions);

            results.Add(ValidateFixture(
                contract.Name,
                "valid",
                expectedValidity: true,
                schemaPath,
                schema,
                Path.Combine(fixtureRoot, "valid", $"{contract.FixtureFileName}.json")));
            results.Add(ValidateFixture(
                contract.Name,
                "invalid",
                expectedValidity: false,
                schemaPath,
                schema,
                Path.Combine(fixtureRoot, "invalid", $"{contract.FixtureFileName}.json")));
        }

        return results;
    }

    public static bool ValidateInstance(
        string repositoryRoot,
        string contractName,
        JsonElement instance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(contractName);

        var requestedContract = Contracts.SingleOrDefault(
            contract => contract.Name == contractName);
        if (requestedContract is null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(contractName),
                contractName,
                "Unknown schema contract.");
        }

        var schemaRoot = Path.Combine(
            Path.GetFullPath(repositoryRoot),
            "packages",
            "schemas");
        var buildOptions = new BuildOptions
        {
            SchemaRegistry = new SchemaRegistry(),
        };
        JsonSchema? requestedSchema = null;

        foreach (var currentContract in Contracts)
        {
            var schemaPath = Path.Combine(
                schemaRoot,
                currentContract.SchemaVersionDirectory,
                $"{currentContract.SchemaFileName}.schema.json");
            EnsureFileExists(schemaPath, "Schema");
            var schema = JsonSchema.FromFile(schemaPath, buildOptions);

            if (currentContract == requestedContract)
            {
                requestedSchema = schema;
            }
        }

        var evaluation = requestedSchema!.Evaluate(
            instance,
            new EvaluationOptions
            {
                OutputFormat = OutputFormat.List,
                RequireFormatValidation = true,
            });

        return evaluation.IsValid &&
               MatchesContractSemantics(contractName, instance);
    }

    public static IReadOnlyList<RulesetRecordValidationResult> ValidateRulesetRecords(
        string repositoryRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryRoot);

        var fullRoot = Path.GetFullPath(repositoryRoot);
        var schemaRoot = Path.Combine(fullRoot, "packages", "schemas");
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
            (ContractName: "formula", DirectoryName: "formulas"),
        };
        var results = new List<RulesetRecordValidationResult>();

        foreach (var recordSet in recordSets)
        {
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
                var schemaVersion = instance.RootElement
                    .GetProperty("schemaVersion")
                    .GetString()!;
                var contractName = recordSet.ContractName == "formula"
                    ? schemaVersion switch
                    {
                        "1.1.0" => "formula",
                        "2.0.0" => "formula-v2",
                        "2.1.0" => "formula-v2",
                        _ => string.Empty,
                    }
                    : recordSet.ContractName;
                var schemaVersionDirectory = contractName == "formula-v2" ? "v2" : "v1";
                var schemaFileName = recordSet.ContractName;
                var schemaPath = Path.Combine(
                    schemaRoot,
                    schemaVersionDirectory,
                    $"{schemaFileName}.schema.json");
                var recordId = instance.RootElement.TryGetProperty("id", out var idElement)
                    ? idElement.GetString() ?? Path.GetFileNameWithoutExtension(recordPath)
                    : Path.GetFileNameWithoutExtension(recordPath);
                var recordVersion = instance.RootElement.TryGetProperty(
                    "version",
                    out var versionElement)
                    ? versionElement.GetString() ?? string.Empty
                    : string.Empty;
                var actualValidity = false;

                if (contractName.Length > 0)
                {
                    EnsureFileExists(schemaPath, "Schema");
                    var schema = JsonSchema.FromFile(
                        schemaPath,
                        new BuildOptions { SchemaRegistry = new SchemaRegistry() });
                    var evaluation = schema.Evaluate(
                        instance.RootElement,
                        new EvaluationOptions
                        {
                            OutputFormat = OutputFormat.List,
                            RequireFormatValidation = true,
                        });
                    actualValidity = evaluation.IsValid &&
                        MatchesContractSemantics(contractName, instance.RootElement);
                }

                results.Add(new RulesetRecordValidationResult(
                    recordSet.ContractName,
                    recordId,
                    recordVersion,
                    schemaVersion,
                    actualValidity,
                    schemaPath,
                    recordPath));
            }
        }

        var duplicateIdentities = results
            .GroupBy(
                result => (result.ContractName, result.RecordId, result.RecordVersion))
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToHashSet();

        return results
            .Select(result => duplicateIdentities.Contains(
                    (result.ContractName, result.RecordId, result.RecordVersion))
                ? result with { ActualValidity = false }
                : result)
            .ToArray();
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
            evaluation.IsValid &&
            MatchesContractSemantics(contractName, instance.RootElement),
            schemaPath,
            fixturePath);
    }

    private static bool MatchesContractSemantics(
        string contractName,
        JsonElement instance)
    {
        if (contractName == "formula-v2")
        {
            return MatchesExecutableFormulaSemantics(instance);
        }

        if (contractName != "formula")
        {
            return true;
        }

        var trace = instance.GetProperty("trace");
        var stepIds = trace
            .GetProperty("stepIds")
            .EnumerateArray()
            .Select(step => step.GetString())
            .ToArray();
        var rawOutputStepId = trace.GetProperty("rawOutputStepId").GetString();
        var visibleOutputStepId =
            trace.GetProperty("visibleOutputStepId").GetString();

        return stepIds.Contains(rawOutputStepId, StringComparer.Ordinal) &&
               stepIds.Contains(visibleOutputStepId, StringComparer.Ordinal) &&
               stepIds[^1] == visibleOutputStepId;
    }

    private static bool MatchesExecutableFormulaSemantics(JsonElement instance)
    {
        var executionModel = instance
            .GetProperty("strategy")
            .GetProperty("executionModel")
            .GetString();
        var inputIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var input in instance.GetProperty("inputs").EnumerateArray())
        {
            var inputId = input.GetProperty("id").GetString()!;
            if (!inputIds.Add(inputId) || !HasCoherentBounds(input))
            {
                return false;
            }
        }

        var programStepIds = new List<string>();
        var priorStepIds = new HashSet<string>(StringComparer.Ordinal);
        JsonElement? roundingStep = null;

        foreach (var step in instance
                     .GetProperty("strategy")
                     .GetProperty("steps")
                     .EnumerateArray())
        {
            var stepId = step.GetProperty("id").GetString()!;
            if (priorStepIds.Contains(stepId))
            {
                return false;
            }

            foreach (var operand in step.GetProperty("operands").EnumerateArray())
            {
                var kind = operand.GetProperty("kind").GetString();
                if (kind == "LITERAL")
                {
                    var literal = operand.GetProperty("value");
                    var literalMatchesModel =
                        executionModel == "CHECKED_INT64_V1"
                            ? literal.TryGetInt64(out _)
                            : executionModel == "CHECKED_DECIMAL_V1" &&
                              literal.TryGetDecimal(out _);
                    if (!literalMatchesModel)
                    {
                        return false;
                    }
                }

                if (kind == "INPUT" &&
                    !inputIds.Contains(operand.GetProperty("inputId").GetString()!))
                {
                    return false;
                }

                if (kind == "STEP" &&
                    !priorStepIds.Contains(operand.GetProperty("stepId").GetString()!))
                {
                    return false;
                }
            }

            priorStepIds.Add(stepId);
            programStepIds.Add(stepId);
            if (step.GetProperty("operation").GetString() == "APPLY_ROUNDING")
            {
                if (roundingStep is not null)
                {
                    return false;
                }

                roundingStep = step;
            }
        }

        if (roundingStep is null)
        {
            return false;
        }

        var trace = instance.GetProperty("trace");
        var traceStepIds = trace
            .GetProperty("stepIds")
            .EnumerateArray()
            .Select(step => step.GetString()!)
            .ToArray();
        var rawOutputStepId = trace.GetProperty("rawOutputStepId").GetString()!;
        var visibleOutputStepId =
            trace.GetProperty("visibleOutputStepId").GetString()!;
        var roundingStage = instance
            .GetProperty("rounding")
            .GetProperty("stage")
            .GetString();
        var roundingStepValue = roundingStep.Value;
        var roundingStepId = roundingStepValue.GetProperty("id").GetString();
        var roundedStepId = roundingStepValue
            .GetProperty("operands")[0]
            .GetProperty("stepId")
            .GetString();

        return programStepIds.SequenceEqual(traceStepIds, StringComparer.Ordinal) &&
               programStepIds[^1] == visibleOutputStepId &&
               rawOutputStepId != visibleOutputStepId &&
               roundingStepId == visibleOutputStepId &&
               roundingStage == visibleOutputStepId &&
               roundedStepId == rawOutputStepId;
    }

    private static bool HasCoherentBounds(JsonElement input)
    {
        var bounds = input.GetProperty("numericBounds");
        var hasMinimum = bounds.TryGetProperty("minimum", out var minimum);
        var hasMaximum = bounds.TryGetProperty("maximum", out var maximum);

        if (hasMinimum && hasMaximum)
        {
            var minimumValue = minimum.GetInt64();
            var maximumValue = maximum.GetInt64();
            if (minimumValue > maximumValue)
            {
                return false;
            }

            if (minimumValue == maximumValue &&
                (!bounds.GetProperty("minimumInclusive").GetBoolean() ||
                 !bounds.GetProperty("maximumInclusive").GetBoolean()))
            {
                return false;
            }
        }

        if (input.GetProperty("numericType").GetString() != "INT32")
        {
            return true;
        }

        return (!hasMinimum || minimum.GetInt64() >= int.MinValue) &&
               (!hasMaximum || maximum.GetInt64() <= int.MaxValue);
    }

    private static void EnsureFileExists(string path, string kind)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"{kind} file was not found.", path);
        }
    }
}
