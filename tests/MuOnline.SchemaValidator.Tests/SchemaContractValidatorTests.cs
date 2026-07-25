using System.Text.Json;
using System.Text.Json.Nodes;
using MuOnline.SchemaValidator;
using Xunit;

namespace MuOnline.SchemaValidator.Tests;

public sealed class SchemaContractValidatorTests
{
    private static readonly string[] ExpectedCharacterClassIds =
    [
        "class-dark-knight",
        "class-dark-lord",
        "class-dark-wizard",
        "class-fairy-elf",
        "class-magic-gladiator",
        "class-summoner",
    ];

    private static readonly string[] ExpectedProgressionRuleIds =
    [
        "progression-five-per-level-hero-status",
        "progression-seven-per-level",
    ];

    private static readonly string[] ExpectedFormulaIds =
    [
        "formula-hp-dark-wizard",
    ];

    private static readonly string[] ExpectedPositiveFormulaCaseIds =
    [
        "hp-dark-wizard-base",
        "hp-dark-wizard-combined-step",
        "hp-dark-wizard-level-step",
        "hp-dark-wizard-vitality-step",
    ];

    private static readonly string[] ExpectedNegativeFormulaCaseIds =
    [
        "hp-dark-wizard-invalid-family",
        "hp-dark-wizard-invalid-level",
        "hp-dark-wizard-overflow",
        "hp-dark-wizard-vitality-below-base",
    ];

    private static readonly string[] ExpectedValidProgressionCaseIds =
    [
        "progression-case-dark-lord-level-220",
        "progression-case-magic-gladiator-level-220",
        "progression-case-standard-level-1-no-hero-status",
        "progression-case-standard-level-220-no-hero-status",
        "progression-case-standard-level-220-with-hero-status",
        "progression-case-standard-level-221-with-hero-status",
        "progression-case-standard-level-230-with-hero-status",
    ];

    private static readonly Dictionary<string, string[]> ExpectedPublishedRuleCaseIds =
        new(StringComparer.Ordinal)
        {
            ["progression-five-per-level-hero-status"] =
            [
                "progression-case-standard-level-1-no-hero-status",
                "progression-case-standard-level-220-no-hero-status",
                "progression-case-standard-level-220-with-hero-status",
                "progression-case-standard-level-221-with-hero-status",
                "progression-case-standard-level-230-with-hero-status",
            ],
            ["progression-seven-per-level"] =
            [
                "progression-case-magic-gladiator-level-220",
                "progression-case-dark-lord-level-220",
            ],
        };

    [Fact]
    public void AllVersionOneFixturesMatchTheirExpectedValidity()
    {
        var results = SchemaContractValidator.ValidateRepository(FindRepositoryRoot());

        Assert.Equal(20, results.Count);
        Assert.Collection(
            results,
            result => AssertResult(result, "evidence", "valid", expectedValidity: true),
            result => AssertResult(result, "evidence", "invalid", expectedValidity: false),
            result => AssertResult(result, "formula", "valid", expectedValidity: true),
            result => AssertResult(result, "formula", "invalid", expectedValidity: false),
            result => AssertResult(result, "calculation-trace", "valid", expectedValidity: true),
            result => AssertResult(result, "calculation-trace", "invalid", expectedValidity: false),
            result => AssertResult(result, "formula-test-case", "valid", expectedValidity: true),
            result => AssertResult(result, "formula-test-case", "invalid", expectedValidity: false),
            result => AssertResult(result, "character-class", "valid", expectedValidity: true),
            result => AssertResult(result, "character-class", "invalid", expectedValidity: false),
            result => AssertResult(result, "progression-rule", "valid", expectedValidity: true),
            result => AssertResult(result, "progression-rule", "invalid", expectedValidity: false),
            result => AssertResult(result, "stat-distribution", "valid", expectedValidity: true),
            result => AssertResult(result, "stat-distribution", "invalid", expectedValidity: false),
            result => AssertResult(result, "build-draft", "valid", expectedValidity: true),
            result => AssertResult(result, "build-draft", "invalid", expectedValidity: false),
            result => AssertResult(result, "server-profile", "valid", expectedValidity: true),
            result => AssertResult(result, "server-profile", "invalid", expectedValidity: false),
            result => AssertResult(result, "build", "valid", expectedValidity: true),
            result => AssertResult(result, "build", "invalid", expectedValidity: false));
    }

    [Theory]
    [InlineData("empty-applicability")]
    [InlineData("duplicate-applicability")]
    [InlineData("factual-bounds-without-evidence")]
    [InlineData("incomplete-input-source")]
    [InlineData("duplicate-trace-steps")]
    [InlineData("trace-output-outside-step-list")]
    [InlineData("visible-output-not-final")]
    public void FormulaContractRejectsEachRequiredInvalidShape(string invalidShape)
    {
        var repositoryRoot = FindRepositoryRoot();
        var formula = LoadFixture(repositoryRoot, "valid", "formula");

        switch (invalidShape)
        {
            case "empty-applicability":
                formula["applicability"]!["evolutionIds"] = new JsonArray();
                break;
            case "duplicate-applicability":
                formula["applicability"]!["evolutionIds"] = new JsonArray(
                    "evolution-synthetic",
                    "evolution-synthetic");
                break;
            case "factual-bounds-without-evidence":
                var bounds = formula["inputs"]![0]!["numericBounds"]!;
                bounds["classification"] = "FACTUAL";
                bounds["evidenceRefs"] = new JsonArray();
                break;
            case "incomplete-input-source":
                formula["inputs"]![0]!["source"]!.AsObject().Remove("valueId");
                break;
            case "duplicate-trace-steps":
                formula["trace"]!["stepIds"] = new JsonArray(
                    "raw-output",
                    "raw-output");
                break;
            case "trace-output-outside-step-list":
                formula["trace"]!["rawOutputStepId"] = "missing-output";
                break;
            case "visible-output-not-final":
                formula["trace"]!["stepIds"] = new JsonArray(
                    "raw-output",
                    "visible-output",
                    "post-visible-step");
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(invalidShape),
                    invalidShape,
                    "Unknown invalid formula shape.");
        }

        Assert.False(ValidateNode(repositoryRoot, "formula", formula));
    }

    [Fact]
    public void FormulaTestCaseRequiresExactlyOneExpectationBranch()
    {
        var repositoryRoot = FindRepositoryRoot();
        var negativeCase = LoadFixture(
            repositoryRoot,
            "valid",
            "formula-test-case");
        negativeCase.AsObject().Remove("expectedTrace");
        negativeCase["expectedErrorCode"] = "synthetic-error";

        Assert.True(ValidateNode(
            repositoryRoot,
            "formula-test-case",
            negativeCase));

        negativeCase["expectedTrace"] = LoadFixture(
            repositoryRoot,
            "valid",
            "calculation-trace");
        Assert.False(ValidateNode(
            repositoryRoot,
            "formula-test-case",
            negativeCase));

        negativeCase.AsObject().Remove("expectedTrace");
        negativeCase.AsObject().Remove("expectedErrorCode");
        Assert.False(ValidateNode(
            repositoryRoot,
            "formula-test-case",
            negativeCase));
    }

    [Fact]
    public void PositiveFormulaTestCaseResolvesTheCalculationTraceReference()
    {
        var repositoryRoot = FindRepositoryRoot();
        var positiveCase = LoadFixture(
            repositoryRoot,
            "valid",
            "formula-test-case");

        Assert.True(ValidateNode(
            repositoryRoot,
            "formula-test-case",
            positiveCase));

        positiveCase["expectedTrace"]!["formulaRef"]!
            .AsObject()
            .Remove("version");

        Assert.False(ValidateNode(
            repositoryRoot,
            "formula-test-case",
            positiveCase));
    }

    [Fact]
    public void RepositoryValidationCanRunMoreThanOnceInTheSameProcess()
    {
        var repositoryRoot = FindRepositoryRoot();

        var firstResults = SchemaContractValidator.ValidateRepository(repositoryRoot);
        var secondResults = SchemaContractValidator.ValidateRepository(repositoryRoot);

        Assert.All(firstResults, result => Assert.True(result.MatchesExpectation));
        Assert.All(secondResults, result => Assert.True(result.MatchesExpectation));
    }

    [Fact]
    public void CanonicalRulesetRecordsMatchTheirContractsAndStableIds()
    {
        var results = SchemaContractValidator.ValidateRulesetRecords(FindRepositoryRoot());

        Assert.Equal(9, results.Count);
        Assert.All(results, result => Assert.True(
            result.ActualValidity,
            $"{result.RecordId} does not match {result.ContractName}."));
        Assert.Equal(
            ExpectedCharacterClassIds,
            results
                .Where(result => result.ContractName == "character-class")
                .Select(result => result.RecordId)
                .Order(StringComparer.Ordinal));
        Assert.Equal(
            ExpectedProgressionRuleIds,
            results
                .Where(result => result.ContractName == "progression-rule")
                .Select(result => result.RecordId)
                .Order(StringComparer.Ordinal));
        Assert.Equal(
            ExpectedFormulaIds,
            results
                .Where(result => result.ContractName == "formula")
                .Select(result => result.RecordId)
                .Order(StringComparer.Ordinal));
    }

    [Fact]
    public void CanonicalFormulaPreservesApprovedRelationsAndCaseInventory()
    {
        var results = FormulaReferenceCaseValidator.ValidateRepository(
            FindRepositoryRoot());

        var result = Assert.Single(results);
        Assert.Equal("formula-hp-dark-wizard", result.FormulaId);
        Assert.Equal("1.0.0", result.FormulaVersion);
        Assert.Equal("PUBLISHED", result.Status);
        Assert.Equal(ExpectedPositiveFormulaCaseIds, result.PositiveCaseIds);
        Assert.Equal(ExpectedNegativeFormulaCaseIds, result.NegativeCaseIds);
        Assert.True(
            result.IsValid,
            $"{result.FormulaId}: {string.Join(" | ", result.Errors)}");
    }

    [Theory]
    [InlineData("foreign-applicable-evolution")]
    [InlineData("trace-order")]
    [InlineData("trace-provenance")]
    [InlineData("positive-coverage")]
    public void FormulaSemanticGateRejectsBrokenCanonicalRelations(string mutation)
    {
        var repositoryRoot = FindRepositoryRoot();
        var temporaryRoot = Path.Combine(
            Path.GetTempPath(),
            $"mu-formula-validator-{Guid.NewGuid():N}");

        try
        {
            CopyDirectory(
                Path.Combine(repositoryRoot, "packages", "schemas"),
                Path.Combine(temporaryRoot, "packages", "schemas"));
            CopyDirectory(
                Path.Combine(repositoryRoot, "packages", "rulesets"),
                Path.Combine(temporaryRoot, "packages", "rulesets"));

            var formulaPath = Path.Combine(
                temporaryRoot,
                "packages",
                "rulesets",
                "mu-s4-global-reference",
                "v1",
                "formulas",
                "hp-dark-wizard.json");
            var formula = JsonNode.Parse(File.ReadAllText(formulaPath))!.AsObject();

            switch (mutation)
            {
                case "foreign-applicable-evolution":
                    formula["applicability"]!["evolutionIds"]![0] =
                        "evolution-fairy-elf";
                    File.WriteAllText(formulaPath, formula.ToJsonString());
                    break;
                case "trace-order":
                    MutatePositiveFormulaCase(
                        temporaryRoot,
                        referenceCase =>
                        {
                            var steps = referenceCase["expectedTrace"]!["steps"]!.AsArray();
                            var firstStep = steps[0]!.DeepClone();
                            var secondStep = steps[1]!.DeepClone();
                            steps[0] = secondStep;
                            steps[1] = firstStep;
                        });
                    break;
                case "trace-provenance":
                    MutatePositiveFormulaCase(
                        temporaryRoot,
                        referenceCase =>
                            referenceCase["expectedTrace"]!["evidenceRefs"] =
                                new JsonArray("evd-0026"));
                    break;
                case "positive-coverage":
                    formula["testCaseRefs"]!.AsArray().RemoveAt(0);
                    File.WriteAllText(formulaPath, formula.ToJsonString());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(mutation),
                        mutation,
                        "Unknown formula relation mutation.");
            }

            var result = Assert.Single(
                FormulaReferenceCaseValidator.ValidateRepository(temporaryRoot));

            Assert.False(result.IsValid);
            Assert.NotEmpty(result.Errors);
        }
        finally
        {
            if (Directory.Exists(temporaryRoot))
            {
                Directory.Delete(temporaryRoot, recursive: true);
            }
        }
    }

    [Fact]
    public void ProgressionReferenceCasesMatchApprovedValuesAndStableIds()
    {
        var results =
            ProgressionReferenceCaseValidator.ValidateRepository(FindRepositoryRoot());
        var validResults = results.Where(result => result.ExpectedValidity).ToArray();

        Assert.Equal(10, results.Count);
        Assert.All(results, result => Assert.True(
            result.MatchesExpectation,
            $"{result.CaseId}: expected valid={result.ExpectedValidity}, " +
            $"actual valid={result.ActualValidity}, " +
            $"expected points={result.ExpectedEarnedPoints}, " +
            $"actual points={result.ActualEarnedPoints}, " +
            $"expected error={result.ExpectedErrorCode ?? "none"}, " +
            $"actual error={result.ActualErrorCode ?? "none"}."));
        Assert.Equal(
            ExpectedValidProgressionCaseIds,
            validResults
                .Select(result => result.CaseId)
                .Order(StringComparer.Ordinal));
        Assert.Equal(
            [0, 1095, 1095, 1101, 1155, 1533, 1533],
            validResults
                .OrderBy(result => Array.IndexOf(
                    ExpectedValidProgressionCaseIds,
                    result.CaseId))
                .Select(result => result.ActualEarnedPoints!.Value)
                .Order());
    }

    [Fact]
    public void ProgressionSemanticGuardsRequireEligibleEvolutionAndExcludeMgAndDl()
    {
        var invalidResults = ProgressionReferenceCaseValidator
            .ValidateRepository(FindRepositoryRoot())
            .Where(result => !result.ExpectedValidity)
            .ToDictionary(result => result.CaseId, StringComparer.Ordinal);

        Assert.Equal(
            "quest-ineligible-evolution",
            invalidResults["progression-case-invalid-hero-status-base-evolution"]
                .ActualErrorCode);
        Assert.Equal(
            "quest-not-supported",
            invalidResults["progression-case-invalid-hero-status-magic-gladiator"]
                .ActualErrorCode);
        Assert.Equal(
            "quest-not-supported",
            invalidResults["progression-case-invalid-hero-status-dark-lord"]
                .ActualErrorCode);
    }

    [Fact]
    public void PublishedProgressionRulesReferenceOnlyTheirApprovedValidCases()
    {
        var results = ProgressionReferenceCaseValidator
            .ValidateRuleReferences(FindRepositoryRoot())
            .ToDictionary(result => result.RuleId, StringComparer.Ordinal);

        Assert.Equal(ExpectedPublishedRuleCaseIds.Keys.Order(), results.Keys.Order());

        foreach (var expectedRule in ExpectedPublishedRuleCaseIds)
        {
            var result = results[expectedRule.Key];

            Assert.Equal("PUBLISHED", result.Status);
            Assert.True(
                result.IsValid,
                $"{result.RuleId}: {string.Join(" | ", result.Errors)}");
            Assert.Equal(expectedRule.Value, result.TestCaseRefs);
        }
    }

    private static void AssertResult(
        FixtureValidationResult result,
        string expectedContract,
        string expectedKind,
        bool expectedValidity)
    {
        Assert.Equal(expectedContract, result.ContractName);
        Assert.Equal(expectedKind, result.FixtureKind);
        Assert.Equal(expectedValidity, result.ExpectedValidity);
        Assert.Equal(expectedValidity, result.ActualValidity);
        Assert.True(result.MatchesExpectation);
    }

    private static JsonNode LoadFixture(
        string repositoryRoot,
        string kind,
        string contractName)
    {
        var path = Path.Combine(
            repositoryRoot,
            "packages",
            "schemas",
            "examples",
            kind,
            $"{contractName}.json");

        return JsonNode.Parse(File.ReadAllText(path))
            ?? throw new InvalidDataException($"Fixture is empty: {path}");
    }

    private static bool ValidateNode(
        string repositoryRoot,
        string contractName,
        JsonNode instance)
    {
        using var document = JsonDocument.Parse(instance.ToJsonString());
        return SchemaContractValidator.ValidateInstance(
            repositoryRoot,
            contractName,
            document.RootElement);
    }

    private static void MutatePositiveFormulaCase(
        string repositoryRoot,
        Action<JsonObject> mutation)
    {
        var casePath = Path.Combine(
            repositoryRoot,
            "packages",
            "rulesets",
            "mu-s4-global-reference",
            "v1",
            "reference-cases",
            "formulas",
            "valid",
            "hp-dark-wizard-base.json");
        var referenceCase = JsonNode.Parse(File.ReadAllText(casePath))!.AsObject();
        mutation(referenceCase);
        File.WriteAllText(casePath, referenceCase.ToJsonString());
    }

    private static void CopyDirectory(string sourceDirectory, string targetDirectory)
    {
        Directory.CreateDirectory(targetDirectory);

        foreach (var filePath in Directory.GetFiles(sourceDirectory))
        {
            File.Copy(
                filePath,
                Path.Combine(targetDirectory, Path.GetFileName(filePath)));
        }

        foreach (var directoryPath in Directory.GetDirectories(sourceDirectory))
        {
            CopyDirectory(
                directoryPath,
                Path.Combine(targetDirectory, Path.GetFileName(directoryPath)));
        }
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "MUOnline.BuildPlanner.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the repository root.");
    }
}
