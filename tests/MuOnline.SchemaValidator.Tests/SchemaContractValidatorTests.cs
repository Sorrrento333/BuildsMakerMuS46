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

        Assert.Equal(14, results.Count);
        Assert.Collection(
            results,
            result => AssertResult(result, "evidence", "valid", expectedValidity: true),
            result => AssertResult(result, "evidence", "invalid", expectedValidity: false),
            result => AssertResult(result, "formula", "valid", expectedValidity: true),
            result => AssertResult(result, "formula", "invalid", expectedValidity: false),
            result => AssertResult(result, "character-class", "valid", expectedValidity: true),
            result => AssertResult(result, "character-class", "invalid", expectedValidity: false),
            result => AssertResult(result, "progression-rule", "valid", expectedValidity: true),
            result => AssertResult(result, "progression-rule", "invalid", expectedValidity: false),
            result => AssertResult(result, "stat-distribution", "valid", expectedValidity: true),
            result => AssertResult(result, "stat-distribution", "invalid", expectedValidity: false),
            result => AssertResult(result, "server-profile", "valid", expectedValidity: true),
            result => AssertResult(result, "server-profile", "invalid", expectedValidity: false),
            result => AssertResult(result, "build", "valid", expectedValidity: true),
            result => AssertResult(result, "build", "invalid", expectedValidity: false));
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

        Assert.Equal(8, results.Count);
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
