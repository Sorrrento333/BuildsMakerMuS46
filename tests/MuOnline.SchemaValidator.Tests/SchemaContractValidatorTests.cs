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

    private static readonly string[] ExpectedFormulaIdentities =
    [
        "formula-ag-dark-knight@1.0.0",
        "formula-ag-dark-wizard@1.0.0",
        "formula-ag-fairy-elf@1.0.0",
        "formula-ag-magic-gladiator@1.0.0",
        "formula-ag-summoner@1.0.0",
        "formula-hp-dark-knight@1.0.0",
        "formula-hp-dark-lord@1.0.0",
        "formula-hp-dark-wizard@1.0.0",
        "formula-hp-dark-wizard@1.1.0",
        "formula-hp-fairy-elf@1.0.0",
        "formula-hp-magic-gladiator@1.0.0",
        "formula-hp-summoner@1.0.0",
        "formula-mana-dark-knight@1.0.0",
        "formula-mana-dark-lord@1.0.0",
        "formula-mana-dark-wizard@1.0.0",
        "formula-mana-fairy-elf@1.0.0",
        "formula-mana-magic-gladiator@1.0.0",
        "formula-mana-summoner@1.0.0",
    ];

    private static readonly string[] ExpectedDarkWizardAgPositiveFormulaCaseIds =
    [
        "ag-dark-wizard-agility-strength-step",
        "ag-dark-wizard-base",
        "ag-dark-wizard-combined-step",
        "ag-dark-wizard-energy-vitality-step",
    ];

    private static readonly string[] ExpectedDarkWizardAgNegativeFormulaCaseIds =
    [
        "ag-dark-wizard-agility-below-base",
        "ag-dark-wizard-energy-below-base",
        "ag-dark-wizard-invalid-family",
        "ag-dark-wizard-overflow",
        "ag-dark-wizard-strength-below-base",
        "ag-dark-wizard-vitality-below-base",
    ];

    private static readonly string[] ExpectedDarkKnightAgPositiveFormulaCaseIds =
    [
        "ag-dark-knight-agility-strength-step",
        "ag-dark-knight-base",
        "ag-dark-knight-combined-step",
        "ag-dark-knight-energy-vitality-step",
    ];

    private static readonly string[] ExpectedDarkKnightAgNegativeFormulaCaseIds =
    [
        "ag-dark-knight-agility-below-base",
        "ag-dark-knight-energy-below-base",
        "ag-dark-knight-invalid-family",
        "ag-dark-knight-overflow",
        "ag-dark-knight-strength-below-base",
        "ag-dark-knight-vitality-below-base",
    ];

    private static readonly string[] ExpectedFairyElfAgPositiveFormulaCaseIds =
    [
        "ag-fairy-elf-agility-strength-step",
        "ag-fairy-elf-base",
        "ag-fairy-elf-combined-step",
        "ag-fairy-elf-energy-vitality-step",
    ];

    private static readonly string[] ExpectedFairyElfAgNegativeFormulaCaseIds =
    [
        "ag-fairy-elf-agility-below-base",
        "ag-fairy-elf-energy-below-base",
        "ag-fairy-elf-invalid-family",
        "ag-fairy-elf-overflow",
        "ag-fairy-elf-strength-below-base",
        "ag-fairy-elf-vitality-below-base",
    ];

    private static readonly string[] ExpectedSummonerAgPositiveFormulaCaseIds =
    [
        "ag-summoner-base",
        "ag-summoner-combined-step",
        "ag-summoner-strength-agility-step",
        "ag-summoner-vitality-energy-step",
    ];

    private static readonly string[] ExpectedSummonerAgNegativeFormulaCaseIds =
    [
        "ag-summoner-agility-below-base",
        "ag-summoner-energy-below-base",
        "ag-summoner-invalid-family",
        "ag-summoner-strength-below-base",
        "ag-summoner-vitality-below-base",
    ];

    private static readonly string[] ExpectedMagicGladiatorAgPositiveFormulaCaseIds =
    [
        "ag-magic-gladiator-agility-strength-step",
        "ag-magic-gladiator-base",
        "ag-magic-gladiator-combined-step",
        "ag-magic-gladiator-energy-vitality-step",
    ];

    private static readonly string[] ExpectedMagicGladiatorAgNegativeFormulaCaseIds =
    [
        "ag-magic-gladiator-agility-below-base",
        "ag-magic-gladiator-energy-below-base",
        "ag-magic-gladiator-invalid-family",
        "ag-magic-gladiator-strength-below-base",
        "ag-magic-gladiator-vitality-below-base",
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

    private static readonly string[] ExpectedDarkKnightPositiveFormulaCaseIds =
    [
        "hp-dark-knight-base",
        "hp-dark-knight-combined-step",
        "hp-dark-knight-level-step",
        "hp-dark-knight-vitality-step",
    ];

    private static readonly string[] ExpectedDarkKnightNegativeFormulaCaseIds =
    [
        "hp-dark-knight-invalid-family",
        "hp-dark-knight-invalid-level",
        "hp-dark-knight-overflow",
        "hp-dark-knight-vitality-below-base",
    ];

    private static readonly string[] ExpectedDarkLordPositiveFormulaCaseIds =
    [
        "hp-dark-lord-base",
        "hp-dark-lord-combined-step",
        "hp-dark-lord-level-step",
        "hp-dark-lord-vitality-step",
    ];

    private static readonly string[] ExpectedDarkLordNegativeFormulaCaseIds =
    [
        "hp-dark-lord-invalid-family",
        "hp-dark-lord-invalid-level",
        "hp-dark-lord-overflow",
        "hp-dark-lord-vitality-below-base",
    ];

    private static readonly string[] ExpectedFairyElfPositiveFormulaCaseIds =
    [
        "hp-fairy-elf-base",
        "hp-fairy-elf-combined-step",
        "hp-fairy-elf-level-step",
        "hp-fairy-elf-vitality-step",
    ];

    private static readonly string[] ExpectedFairyElfNegativeFormulaCaseIds =
    [
        "hp-fairy-elf-invalid-family",
        "hp-fairy-elf-invalid-level",
        "hp-fairy-elf-overflow",
        "hp-fairy-elf-vitality-below-base",
    ];

    private static readonly string[] ExpectedSummonerPositiveFormulaCaseIds =
    [
        "hp-summoner-base",
        "hp-summoner-combined-step",
        "hp-summoner-level-step",
        "hp-summoner-vitality-step",
    ];

    private static readonly string[] ExpectedSummonerNegativeFormulaCaseIds =
    [
        "hp-summoner-invalid-family",
        "hp-summoner-invalid-level",
        "hp-summoner-overflow",
        "hp-summoner-vitality-below-base",
    ];

    private static readonly string[] ExpectedMagicGladiatorPositiveFormulaCaseIds =
    [
        "hp-magic-gladiator-base",
        "hp-magic-gladiator-combined-step",
        "hp-magic-gladiator-level-step",
        "hp-magic-gladiator-vitality-step",
    ];

    private static readonly string[] ExpectedMagicGladiatorNegativeFormulaCaseIds =
    [
        "hp-magic-gladiator-invalid-family",
        "hp-magic-gladiator-invalid-level",
        "hp-magic-gladiator-overflow",
        "hp-magic-gladiator-vitality-below-base",
    ];

    private static readonly string[] ExpectedDarkWizardManaPositiveFormulaCaseIds =
    [
        "mana-dark-wizard-base",
        "mana-dark-wizard-combined-step",
        "mana-dark-wizard-energy-step",
        "mana-dark-wizard-level-step",
    ];

    private static readonly string[] ExpectedDarkWizardManaNegativeFormulaCaseIds =
    [
        "mana-dark-wizard-energy-below-base",
        "mana-dark-wizard-invalid-family",
        "mana-dark-wizard-invalid-level",
        "mana-dark-wizard-overflow",
    ];

    private static readonly string[] ExpectedDarkKnightManaPositiveFormulaCaseIds =
    [
        "mana-dark-knight-base",
        "mana-dark-knight-combined-step",
        "mana-dark-knight-energy-step",
        "mana-dark-knight-level-step",
    ];

    private static readonly string[] ExpectedDarkKnightManaNegativeFormulaCaseIds =
    [
        "mana-dark-knight-energy-below-base",
        "mana-dark-knight-invalid-family",
        "mana-dark-knight-invalid-level",
        "mana-dark-knight-overflow",
    ];

    private static readonly string[] ExpectedDarkLordManaPositiveFormulaCaseIds =
    [
        "mana-dark-lord-base",
        "mana-dark-lord-combined-step",
        "mana-dark-lord-energy-step",
        "mana-dark-lord-level-step",
    ];

    private static readonly string[] ExpectedDarkLordManaNegativeFormulaCaseIds =
    [
        "mana-dark-lord-energy-below-base",
        "mana-dark-lord-invalid-family",
        "mana-dark-lord-invalid-level",
        "mana-dark-lord-overflow",
    ];

    private static readonly string[] ExpectedFairyElfManaPositiveFormulaCaseIds =
    [
        "mana-fairy-elf-base",
        "mana-fairy-elf-combined-step",
        "mana-fairy-elf-energy-step",
        "mana-fairy-elf-level-step",
    ];

    private static readonly string[] ExpectedFairyElfManaNegativeFormulaCaseIds =
    [
        "mana-fairy-elf-energy-below-base",
        "mana-fairy-elf-invalid-family",
        "mana-fairy-elf-invalid-level",
        "mana-fairy-elf-overflow",
    ];

    private static readonly string[] ExpectedSummonerManaPositiveFormulaCaseIds =
    [
        "mana-summoner-base",
        "mana-summoner-combined-step",
        "mana-summoner-energy-step",
        "mana-summoner-level-step",
    ];

    private static readonly string[] ExpectedSummonerManaNegativeFormulaCaseIds =
    [
        "mana-summoner-energy-below-base",
        "mana-summoner-invalid-family",
        "mana-summoner-invalid-level",
        "mana-summoner-overflow",
    ];

    private static readonly string[] ExpectedMagicGladiatorManaPositiveFormulaCaseIds =
    [
        "mana-magic-gladiator-base",
        "mana-magic-gladiator-combined-step",
        "mana-magic-gladiator-energy-step",
        "mana-magic-gladiator-level-step",
    ];

    private static readonly string[] ExpectedMagicGladiatorManaNegativeFormulaCaseIds =
    [
        "mana-magic-gladiator-energy-below-base",
        "mana-magic-gladiator-invalid-family",
        "mana-magic-gladiator-invalid-level",
        "mana-magic-gladiator-overflow",
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
    public void AllVersionedFixturesMatchTheirExpectedValidity()
    {
        var results = SchemaContractValidator.ValidateRepository(FindRepositoryRoot());

        Assert.Equal(22, results.Count);
        Assert.Collection(
            results,
            result => AssertResult(result, "evidence", "valid", expectedValidity: true),
            result => AssertResult(result, "evidence", "invalid", expectedValidity: false),
            result => AssertResult(result, "formula", "valid", expectedValidity: true),
            result => AssertResult(result, "formula", "invalid", expectedValidity: false),
            result => AssertResult(result, "formula-v2", "valid", expectedValidity: true),
            result => AssertResult(result, "formula-v2", "invalid", expectedValidity: false),
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

    [Theory]
    [InlineData("missing-range-error-code")]
    [InlineData("missing-execution-model")]
    [InlineData("text-definition")]
    [InlineData("unsupported-operation")]
    [InlineData("constant-with-input")]
    [InlineData("binary-operation-with-one-operand")]
    [InlineData("rounding-with-literal")]
    public void FormulaVersionTwoContractRejectsIncompleteProgramShapes(
        string invalidShape)
    {
        var repositoryRoot = FindRepositoryRoot();
        var formula = LoadFixture(repositoryRoot, "valid", "formula-v2");
        var steps = formula["strategy"]!["steps"]!.AsArray();

        switch (invalidShape)
        {
            case "missing-range-error-code":
                formula["inputs"]![0]!.AsObject().Remove("rangeErrorCode");
                break;
            case "missing-execution-model":
                formula["strategy"]!.AsObject().Remove("executionModel");
                break;
            case "text-definition":
                formula["strategy"]!["definition"] = "input-synthetic";
                break;
            case "unsupported-operation":
                steps[0]!["operation"] = "DIVIDE";
                break;
            case "constant-with-input":
                steps[0]!["operands"] = new JsonArray(
                    new JsonObject
                    {
                        ["kind"] = "INPUT",
                        ["inputId"] = "input-synthetic",
                    });
                break;
            case "binary-operation-with-one-operand":
                steps[1]!["operands"]!.AsArray().RemoveAt(1);
                break;
            case "rounding-with-literal":
                steps[^1]!["operands"] = new JsonArray(
                    new JsonObject
                    {
                        ["kind"] = "LITERAL",
                        ["value"] = 1,
                    });
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(invalidShape),
                    invalidShape,
                    "Unknown invalid executable formula shape.");
        }

        Assert.False(ValidateNode(repositoryRoot, "formula-v2", formula));
    }

    [Theory]
    [InlineData("forward-step-reference")]
    [InlineData("undeclared-input-reference")]
    [InlineData("trace-program-order")]
    [InlineData("rounding-stage")]
    [InlineData("raw-output-source")]
    [InlineData("reversed-bounds")]
    [InlineData("int32-bounds-overflow")]
    [InlineData("duplicate-step-id")]
    public void FormulaVersionTwoSemanticGateRejectsInvalidPrograms(string mutation)
    {
        var repositoryRoot = FindRepositoryRoot();
        var formula = LoadFixture(repositoryRoot, "valid", "formula-v2");
        var steps = formula["strategy"]!["steps"]!.AsArray();

        switch (mutation)
        {
            case "forward-step-reference":
                steps[1]!["operands"]![0] = new JsonObject
                {
                    ["kind"] = "STEP",
                    ["stepId"] = "visible-output",
                };
                break;
            case "undeclared-input-reference":
                steps[1]!["operands"]![0]!["inputId"] = "input-unknown";
                break;
            case "trace-program-order":
                var traceSteps = formula["trace"]!["stepIds"]!.AsArray();
                var firstTraceStep = traceSteps[0]!.DeepClone();
                traceSteps[0] = traceSteps[1]!.DeepClone();
                traceSteps[1] = firstTraceStep;
                break;
            case "rounding-stage":
                formula["rounding"]!["stage"] = "raw-output";
                break;
            case "raw-output-source":
                steps[^1]!["operands"]![0]!["stepId"] = "input-contribution";
                break;
            case "reversed-bounds":
                formula["inputs"]![0]!["numericBounds"]!["minimum"] = 101;
                break;
            case "int32-bounds-overflow":
                formula["inputs"]![0]!["numericBounds"]!["maximum"] =
                    (long)int.MaxValue + 1;
                break;
            case "duplicate-step-id":
                steps[1]!["id"] = "constant-contribution";
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(mutation),
                    mutation,
                    "Unknown executable formula mutation.");
        }

        Assert.False(ValidateNode(repositoryRoot, "formula-v2", formula));
    }

    [Fact]
    public void FormulaVersionTwoPointOneRequiresDecimalModelAndAcceptsExactLiteral()
    {
        var repositoryRoot = FindRepositoryRoot();
        var formula = LoadFixture(repositoryRoot, "valid", "formula-v2");
        formula["schemaVersion"] = "2.1.0";
        formula["strategy"]!["executionModel"] = "CHECKED_DECIMAL_V1";
        formula["strategy"]!["steps"]![0]!["operands"]![0]!["value"] = 1.5m;

        Assert.True(ValidateNode(repositoryRoot, "formula-v2", formula));

        formula["strategy"]!["executionModel"] = "CHECKED_INT64_V1";
        Assert.False(ValidateNode(repositoryRoot, "formula-v2", formula));

        formula["schemaVersion"] = "2.0.0";
        Assert.False(ValidateNode(repositoryRoot, "formula-v2", formula));
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

        Assert.Equal(26, results.Count);
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
            ExpectedFormulaIdentities,
            results
                .Where(result => result.ContractName == "formula")
                .Select(result => $"{result.RecordId}@{result.RecordVersion}")
                .Order(StringComparer.Ordinal));
        Assert.Equal(
            [
                "1.1.0",
                "2.0.0",
                "2.0.0",
                "2.0.0",
                "2.0.0",
                "2.0.0",
                "2.0.0",
                "2.0.0",
                "2.1.0",
                "2.1.0",
                "2.1.0",
                "2.1.0",
                "2.1.0",
                "2.1.0",
                "2.1.0",
                "2.1.0",
                "2.1.0",
                "2.1.0",
            ],
            results
                .Where(result => result.ContractName == "formula")
                .Select(result => result.SchemaVersion)
                .Order(StringComparer.Ordinal));
    }

    [Fact]
    public void CanonicalFormulaPreservesApprovedRelationsAndCaseInventory()
    {
        var results = FormulaReferenceCaseValidator.ValidateRepository(
            FindRepositoryRoot());

        Assert.Equal(18, results.Count);

        var darkKnightAg = Assert.Single(
            results,
            result =>
                result.FormulaId == "formula-ag-dark-knight" &&
                result.FormulaVersion == "1.0.0");
        Assert.Equal("PUBLISHED", darkKnightAg.Status);
        Assert.Equal(
            ExpectedDarkKnightAgPositiveFormulaCaseIds,
            darkKnightAg.PositiveCaseIds);
        Assert.Equal(
            ExpectedDarkKnightAgNegativeFormulaCaseIds,
            darkKnightAg.NegativeCaseIds);
        Assert.True(
            darkKnightAg.IsValid,
            $"{darkKnightAg.FormulaId}: {string.Join(" | ", darkKnightAg.Errors)}");

        var darkWizardAg = Assert.Single(
            results,
            result =>
                result.FormulaId == "formula-ag-dark-wizard" &&
                result.FormulaVersion == "1.0.0");
        Assert.Equal("PUBLISHED", darkWizardAg.Status);
        Assert.Equal(
            ExpectedDarkWizardAgPositiveFormulaCaseIds,
            darkWizardAg.PositiveCaseIds);
        Assert.Equal(
            ExpectedDarkWizardAgNegativeFormulaCaseIds,
            darkWizardAg.NegativeCaseIds);
        Assert.True(
            darkWizardAg.IsValid,
            $"{darkWizardAg.FormulaId}: {string.Join(" | ", darkWizardAg.Errors)}");

        var fairyElfAg = Assert.Single(
            results,
            result =>
                result.FormulaId == "formula-ag-fairy-elf" &&
                result.FormulaVersion == "1.0.0");
        Assert.Equal("PUBLISHED", fairyElfAg.Status);
        Assert.Equal(
            ExpectedFairyElfAgPositiveFormulaCaseIds,
            fairyElfAg.PositiveCaseIds);
        Assert.Equal(
            ExpectedFairyElfAgNegativeFormulaCaseIds,
            fairyElfAg.NegativeCaseIds);
        Assert.True(
            fairyElfAg.IsValid,
            $"{fairyElfAg.FormulaId}: {string.Join(" | ", fairyElfAg.Errors)}");

        var magicGladiatorAg = Assert.Single(
            results,
            result =>
                result.FormulaId == "formula-ag-magic-gladiator" &&
                result.FormulaVersion == "1.0.0");
        Assert.Equal("PUBLISHED", magicGladiatorAg.Status);
        Assert.Equal(
            ExpectedMagicGladiatorAgPositiveFormulaCaseIds,
            magicGladiatorAg.PositiveCaseIds);
        Assert.Equal(
            ExpectedMagicGladiatorAgNegativeFormulaCaseIds,
            magicGladiatorAg.NegativeCaseIds);
        Assert.True(
            magicGladiatorAg.IsValid,
            $"{magicGladiatorAg.FormulaId}: {string.Join(" | ", magicGladiatorAg.Errors)}");

        var summonerAg = Assert.Single(
            results,
            result =>
                result.FormulaId == "formula-ag-summoner" &&
                result.FormulaVersion == "1.0.0");
        Assert.Equal("PUBLISHED", summonerAg.Status);
        Assert.Equal(
            ExpectedSummonerAgPositiveFormulaCaseIds,
            summonerAg.PositiveCaseIds);
        Assert.Equal(
            ExpectedSummonerAgNegativeFormulaCaseIds,
            summonerAg.NegativeCaseIds);
        Assert.True(
            summonerAg.IsValid,
            $"{summonerAg.FormulaId}: {string.Join(" | ", summonerAg.Errors)}");

        var published = Assert.Single(
            results,
            result =>
                result.FormulaId == "formula-hp-dark-wizard" &&
                result.FormulaVersion == "1.0.0");
        Assert.Equal("formula-hp-dark-wizard", published.FormulaId);
        Assert.Equal("PUBLISHED", published.Status);
        Assert.Equal(ExpectedPositiveFormulaCaseIds, published.PositiveCaseIds);
        Assert.Equal(ExpectedNegativeFormulaCaseIds, published.NegativeCaseIds);
        Assert.True(
            published.IsValid,
            $"{published.FormulaId}: {string.Join(" | ", published.Errors)}");

        var executable = Assert.Single(
            results,
            result =>
                result.FormulaId == "formula-hp-dark-wizard" &&
                result.FormulaVersion == "1.1.0");
        Assert.Equal("formula-hp-dark-wizard", executable.FormulaId);
        Assert.Equal("PUBLISHED", executable.Status);
        Assert.Equal(ExpectedPositiveFormulaCaseIds, executable.PositiveCaseIds);
        Assert.Equal(ExpectedNegativeFormulaCaseIds, executable.NegativeCaseIds);
        Assert.True(
            executable.IsValid,
            $"{executable.FormulaId}: {string.Join(" | ", executable.Errors)}");

        var darkKnight = Assert.Single(
            results,
            result =>
                result.FormulaId == "formula-hp-dark-knight" &&
                result.FormulaVersion == "1.0.0");
        Assert.Equal("PUBLISHED", darkKnight.Status);
        Assert.Equal(
            ExpectedDarkKnightPositiveFormulaCaseIds,
            darkKnight.PositiveCaseIds);
        Assert.Equal(
            ExpectedDarkKnightNegativeFormulaCaseIds,
            darkKnight.NegativeCaseIds);
        Assert.True(
            darkKnight.IsValid,
            $"{darkKnight.FormulaId}: {string.Join(" | ", darkKnight.Errors)}");

        var darkLord = Assert.Single(
            results,
            result =>
                result.FormulaId == "formula-hp-dark-lord" &&
                result.FormulaVersion == "1.0.0");
        Assert.Equal("PUBLISHED", darkLord.Status);
        Assert.Equal(
            ExpectedDarkLordPositiveFormulaCaseIds,
            darkLord.PositiveCaseIds);
        Assert.Equal(
            ExpectedDarkLordNegativeFormulaCaseIds,
            darkLord.NegativeCaseIds);
        Assert.True(
            darkLord.IsValid,
            $"{darkLord.FormulaId}: {string.Join(" | ", darkLord.Errors)}");

        var fairyElf = Assert.Single(
            results,
            result =>
                result.FormulaId == "formula-hp-fairy-elf" &&
                result.FormulaVersion == "1.0.0");
        Assert.Equal("PUBLISHED", fairyElf.Status);
        Assert.Equal(
            ExpectedFairyElfPositiveFormulaCaseIds,
            fairyElf.PositiveCaseIds);
        Assert.Equal(
            ExpectedFairyElfNegativeFormulaCaseIds,
            fairyElf.NegativeCaseIds);
        Assert.True(
            fairyElf.IsValid,
            $"{fairyElf.FormulaId}: {string.Join(" | ", fairyElf.Errors)}");

        var summoner = Assert.Single(
            results,
            result =>
                result.FormulaId == "formula-hp-summoner" &&
                result.FormulaVersion == "1.0.0");
        Assert.Equal("PUBLISHED", summoner.Status);
        Assert.Equal(
            ExpectedSummonerPositiveFormulaCaseIds,
            summoner.PositiveCaseIds);
        Assert.Equal(
            ExpectedSummonerNegativeFormulaCaseIds,
            summoner.NegativeCaseIds);
        Assert.True(
            summoner.IsValid,
            $"{summoner.FormulaId}: {string.Join(" | ", summoner.Errors)}");

        var magicGladiator = Assert.Single(
            results,
            result =>
                result.FormulaId == "formula-hp-magic-gladiator" &&
                result.FormulaVersion == "1.0.0");
        Assert.Equal("PUBLISHED", magicGladiator.Status);
        Assert.Equal(
            ExpectedMagicGladiatorPositiveFormulaCaseIds,
            magicGladiator.PositiveCaseIds);
        Assert.Equal(
            ExpectedMagicGladiatorNegativeFormulaCaseIds,
            magicGladiator.NegativeCaseIds);
        Assert.True(
            magicGladiator.IsValid,
            $"{magicGladiator.FormulaId}: {string.Join(" | ", magicGladiator.Errors)}");

        var darkWizardMana = Assert.Single(
            results,
            result =>
                result.FormulaId == "formula-mana-dark-wizard" &&
                result.FormulaVersion == "1.0.0");
        Assert.Equal("PUBLISHED", darkWizardMana.Status);
        Assert.Equal(
            ExpectedDarkWizardManaPositiveFormulaCaseIds,
            darkWizardMana.PositiveCaseIds);
        Assert.Equal(
            ExpectedDarkWizardManaNegativeFormulaCaseIds,
            darkWizardMana.NegativeCaseIds);
        Assert.True(
            darkWizardMana.IsValid,
            $"{darkWizardMana.FormulaId}: {string.Join(" | ", darkWizardMana.Errors)}");

        var darkKnightMana = Assert.Single(
            results,
            result =>
                result.FormulaId == "formula-mana-dark-knight" &&
                result.FormulaVersion == "1.0.0");
        Assert.Equal("PUBLISHED", darkKnightMana.Status);
        Assert.Equal(
            ExpectedDarkKnightManaPositiveFormulaCaseIds,
            darkKnightMana.PositiveCaseIds);
        Assert.Equal(
            ExpectedDarkKnightManaNegativeFormulaCaseIds,
            darkKnightMana.NegativeCaseIds);
        Assert.True(
            darkKnightMana.IsValid,
            $"{darkKnightMana.FormulaId}: {string.Join(" | ", darkKnightMana.Errors)}");

        var darkLordMana = Assert.Single(
            results,
            result =>
                result.FormulaId == "formula-mana-dark-lord" &&
                result.FormulaVersion == "1.0.0");
        Assert.Equal("PUBLISHED", darkLordMana.Status);
        Assert.Equal(
            ExpectedDarkLordManaPositiveFormulaCaseIds,
            darkLordMana.PositiveCaseIds);
        Assert.Equal(
            ExpectedDarkLordManaNegativeFormulaCaseIds,
            darkLordMana.NegativeCaseIds);
        Assert.True(
            darkLordMana.IsValid,
            $"{darkLordMana.FormulaId}: {string.Join(" | ", darkLordMana.Errors)}");

        var fairyElfMana = Assert.Single(
            results,
            result =>
                result.FormulaId == "formula-mana-fairy-elf" &&
                result.FormulaVersion == "1.0.0");
        Assert.Equal("PUBLISHED", fairyElfMana.Status);
        Assert.Equal(
            ExpectedFairyElfManaPositiveFormulaCaseIds,
            fairyElfMana.PositiveCaseIds);
        Assert.Equal(
            ExpectedFairyElfManaNegativeFormulaCaseIds,
            fairyElfMana.NegativeCaseIds);
        Assert.True(
            fairyElfMana.IsValid,
            $"{fairyElfMana.FormulaId}: {string.Join(" | ", fairyElfMana.Errors)}");

        var summonerMana = Assert.Single(
            results,
            result =>
                result.FormulaId == "formula-mana-summoner" &&
                result.FormulaVersion == "1.0.0");
        Assert.Equal("PUBLISHED", summonerMana.Status);
        Assert.Equal(
            ExpectedSummonerManaPositiveFormulaCaseIds,
            summonerMana.PositiveCaseIds);
        Assert.Equal(
            ExpectedSummonerManaNegativeFormulaCaseIds,
            summonerMana.NegativeCaseIds);
        Assert.True(
            summonerMana.IsValid,
            $"{summonerMana.FormulaId}: {string.Join(" | ", summonerMana.Errors)}");

        var magicGladiatorMana = Assert.Single(
            results,
            result =>
                result.FormulaId == "formula-mana-magic-gladiator" &&
                result.FormulaVersion == "1.0.0");
        Assert.Equal("PUBLISHED", magicGladiatorMana.Status);
        Assert.Equal(
            ExpectedMagicGladiatorManaPositiveFormulaCaseIds,
            magicGladiatorMana.PositiveCaseIds);
        Assert.Equal(
            ExpectedMagicGladiatorManaNegativeFormulaCaseIds,
            magicGladiatorMana.NegativeCaseIds);
        Assert.True(
            magicGladiatorMana.IsValid,
            $"{magicGladiatorMana.FormulaId}: {string.Join(" | ", magicGladiatorMana.Errors)}");
    }

    [Fact]
    public void ExecutableVersionPreservesPublishedMeaningWithoutRewritingHistory()
    {
        var formulaDirectory = Path.Combine(
            FindRepositoryRoot(),
            "packages",
            "rulesets",
            "mu-s4-global-reference",
            "v1",
            "formulas");
        var published = JsonNode.Parse(File.ReadAllText(
            Path.Combine(formulaDirectory, "hp-dark-wizard.json")))!.AsObject();
        var executable = JsonNode.Parse(File.ReadAllText(
            Path.Combine(formulaDirectory, "hp-dark-wizard-1.1.0.json")))!.AsObject();

        foreach (var propertyName in new[]
                 {
                     "id",
                     "rulesetId",
                     "confidence",
                     "purpose",
                     "applicability",
                     "output",
                     "rounding",
                     "trace",
                     "constraints",
                     "evidenceRefs",
                     "conflictIds",
                 })
        {
            Assert.True(
                JsonNode.DeepEquals(published[propertyName], executable[propertyName]),
                $"Property '{propertyName}' changed between formula versions.");
        }

        var publishedInputs = published["inputs"]!.DeepClone();
        var executableInputs = executable["inputs"]!.DeepClone().AsArray();
        foreach (var input in executableInputs)
        {
            input!.AsObject().Remove("rangeErrorCode");
        }

        Assert.True(JsonNode.DeepEquals(publishedInputs, executableInputs));
        Assert.Equal("2.0.0", executable["schemaVersion"]!.GetValue<string>());
        Assert.Equal("1.1.0", executable["version"]!.GetValue<string>());
        Assert.Equal("PUBLISHED", executable["status"]!.GetValue<string>());
        var testCaseRefs = executable["testCaseRefs"]!.AsArray();
        Assert.Equal(
            ExpectedPositiveFormulaCaseIds,
            testCaseRefs
                .Select(reference => reference!["id"]!.GetValue<string>())
                .Order(StringComparer.Ordinal));
        Assert.All(
            testCaseRefs,
            reference => Assert.Equal(
                "1.1.0",
                reference!["version"]!.GetValue<string>()));
    }

    [Fact]
    public void VersionedFormulaCasesPreserveApprovedContentExactly()
    {
        var caseRoot = Path.Combine(
            FindRepositoryRoot(),
            "packages",
            "rulesets",
            "mu-s4-global-reference",
            "v1",
            "reference-cases",
            "formulas");
        var casePairs = new[]
        {
            ("valid", "hp-dark-wizard-base.json", "hp-dark-wizard-base-1.1.0.json"),
            ("valid", "hp-dark-wizard-combined-step.json", "hp-dark-wizard-combined-step-1.1.0.json"),
            ("valid", "hp-dark-wizard-level-step.json", "hp-dark-wizard-level-step-1.1.0.json"),
            ("valid", "hp-dark-wizard-vitality-step.json", "hp-dark-wizard-vitality-step-1.1.0.json"),
            ("invalid", "hp-dark-wizard-invalid-family.json", "hp-dark-wizard-invalid-family-1.1.0.json"),
            ("invalid", "hp-dark-wizard-invalid-level.json", "hp-dark-wizard-invalid-level-1.1.0.json"),
            ("invalid", "hp-dark-wizard-overflow.json", "hp-dark-wizard-overflow-1.1.0.json"),
            ("invalid", "hp-dark-wizard-vitality-below-base.json", "hp-dark-wizard-vitality-below-base-1.1.0.json"),
        };

        foreach (var (kind, historicalName, executableName) in casePairs)
        {
            var historical = JsonNode.Parse(File.ReadAllText(
                Path.Combine(caseRoot, kind, historicalName)))!.AsObject();
            var executable = JsonNode.Parse(File.ReadAllText(
                Path.Combine(caseRoot, kind, executableName)))!.AsObject();

            executable["formulaRef"]!["version"] = "1.0.0";
            if (executable["expectedTrace"] is JsonNode trace)
            {
                trace["formulaRef"]!["version"] = "1.0.0";
            }

            Assert.True(
                JsonNode.DeepEquals(historical, executable),
                $"Case '{executableName}' changed beyond formulaRef.version.");
        }
    }

    [Fact]
    public void CanonicalFormulaGateRejectsDuplicateCompositeIdentity()
    {
        var repositoryRoot = FindRepositoryRoot();
        var temporaryRoot = Path.Combine(
            Path.GetTempPath(),
            $"mu-formula-identity-{Guid.NewGuid():N}");

        try
        {
            CopyDirectory(
                Path.Combine(repositoryRoot, "packages", "schemas"),
                Path.Combine(temporaryRoot, "packages", "schemas"));
            CopyDirectory(
                Path.Combine(repositoryRoot, "packages", "rulesets"),
                Path.Combine(temporaryRoot, "packages", "rulesets"));

            var formulaDirectory = Path.Combine(
                temporaryRoot,
                "packages",
                "rulesets",
                "mu-s4-global-reference",
                "v1",
                "formulas");
            File.Copy(
                Path.Combine(formulaDirectory, "hp-dark-wizard-1.1.0.json"),
                Path.Combine(formulaDirectory, "duplicate.json"));

            var contractResults = SchemaContractValidator
                .ValidateRulesetRecords(temporaryRoot)
                .Where(result =>
                    result.ContractName == "formula" &&
                    result.RecordVersion == "1.1.0")
                .ToArray();
            Assert.Equal(2, contractResults.Length);
            Assert.All(contractResults, result => Assert.False(result.ActualValidity));

            var relationResults = FormulaReferenceCaseValidator
                .ValidateRepository(temporaryRoot)
                .Where(result => result.FormulaVersion == "1.1.0")
                .ToArray();
            Assert.Equal(2, relationResults.Length);
            Assert.All(relationResults, result => Assert.False(result.IsValid));
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
    public void CanonicalFormulaGateRejectsDuplicateCompositeCaseIdentity()
    {
        var repositoryRoot = FindRepositoryRoot();
        var temporaryRoot = Path.Combine(
            Path.GetTempPath(),
            $"mu-formula-case-identity-{Guid.NewGuid():N}");

        try
        {
            CopyDirectory(
                Path.Combine(repositoryRoot, "packages", "schemas"),
                Path.Combine(temporaryRoot, "packages", "schemas"));
            CopyDirectory(
                Path.Combine(repositoryRoot, "packages", "rulesets"),
                Path.Combine(temporaryRoot, "packages", "rulesets"));

            var caseDirectory = Path.Combine(
                temporaryRoot,
                "packages",
                "rulesets",
                "mu-s4-global-reference",
                "v1",
                "reference-cases",
                "formulas",
                "valid");
            File.Copy(
                Path.Combine(caseDirectory, "hp-dark-wizard-base-1.1.0.json"),
                Path.Combine(caseDirectory, "duplicate-1.1.0.json"));

            var results = FormulaReferenceCaseValidator
                .ValidateRepository(temporaryRoot)
                .ToArray();

            Assert.Equal(18, results.Length);
            Assert.All(results, result => Assert.False(result.IsValid));
            Assert.All(
                results,
                result => Assert.Contains(
                    result.Errors,
                    error => error.Contains(
                        "Formula case identity 'hp-dark-wizard-base' version '1.1.0' is duplicated.",
                        StringComparison.Ordinal)));
        }
        finally
        {
            if (Directory.Exists(temporaryRoot))
            {
                Directory.Delete(temporaryRoot, recursive: true);
            }
        }
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
                FormulaReferenceCaseValidator.ValidateRepository(temporaryRoot),
                candidate =>
                    candidate.FormulaId == "formula-hp-dark-wizard" &&
                    candidate.FormulaVersion == "1.0.0");

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
