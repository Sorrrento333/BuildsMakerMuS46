using MuOnline.BuildPlanner.CalculationEngine.Progression;
using MuOnline.BuildPlanner.Domain.Progression;
using Xunit;

namespace MuOnline.BuildPlanner.CalculationEngine.Tests;

public sealed class ProgressionPointBudgetCalculatorTests
{
    private static readonly CanonicalProgressionRuleset Ruleset =
        CanonicalProgressionRuleset.Load();

    public static TheoryData<ProgressionReferenceCase> ValidCases =>
        CreateTheoryData(Ruleset.ValidCases);

    public static TheoryData<ProgressionReferenceCase> InvalidCases =>
        CreateTheoryData(Ruleset.InvalidCases);

    [Theory]
    [MemberData(nameof(ValidCases))]
    public void PublishedRulesReproduceApprovedReferenceCases(
        ProgressionReferenceCase referenceCase)
    {
        var calculator = CreateCalculator();

        var result = calculator.Calculate(ToRequest(referenceCase));

        Assert.Equal(referenceCase.ExpectedEarnedPoints, result.EarnedPoints);
        Assert.Equal(referenceCase.ProgressionRuleId, result.ProgressionRuleId);
        Assert.Equal(
            referenceCase.ExpectedEarnedPoints,
            result.Contributions.Sum(contribution => contribution.EarnedPoints));
        Assert.Equal("mu-s4-global-reference", result.RulesetId);
        Assert.Contains(
            result.ProgressionRuleId,
            Ruleset.Rules
                .Where(rule => rule.Status == ProgressionRuleStatus.Published)
                .Select(rule => rule.Id));
    }

    [Theory]
    [MemberData(nameof(InvalidCases))]
    public void PublishedRulesReproduceApprovedSemanticRejections(
        ProgressionReferenceCase referenceCase)
    {
        var calculator = CreateCalculator();

        var exception = Assert.Throws<ProgressionPointBudgetException>(
            () => calculator.Calculate(ToRequest(referenceCase)));

        Assert.Equal(referenceCase.ExpectedErrorCode, exception.Code);
    }

    [Fact]
    public void TraceSeparatesLevelAndHeroStatusContributions()
    {
        var referenceCase = Assert.Single(
            Ruleset.ValidCases,
            item => item.Id == "progression-case-standard-level-230-with-hero-status");
        var calculator = CreateCalculator();

        var result = calculator.Calculate(ToRequest(referenceCase));

        Assert.Collection(
            result.Contributions,
            contribution =>
            {
                Assert.Equal(ProgressionPointContributionKind.Level, contribution.Kind);
                Assert.Equal(result.ProgressionRuleId, contribution.SourceId);
                Assert.Equal(229, contribution.AwardedLevelCount);
                Assert.Equal(5, contribution.PointsPerLevel);
                Assert.Equal(1145, contribution.EarnedPoints);
            },
            contribution =>
            {
                Assert.Equal(
                    ProgressionPointContributionKind.QuestBonus,
                    contribution.Kind);
                Assert.Equal("quest-hero-status", contribution.SourceId);
                Assert.Equal(10, contribution.AwardedLevelCount);
                Assert.Equal(1, contribution.PointsPerLevel);
                Assert.Equal(10, contribution.EarnedPoints);
            });
    }

    [Fact]
    public void ResolverDoesNotExecuteAnUnpublishedRule()
    {
        var characterClass = Ruleset.Classes.Single(
            item => item.Id == "class-magic-gladiator");
        var rule = Ruleset.Rules.Single(
            item => item.Id == "progression-seven-per-level") with
        {
            Status = ProgressionRuleStatus.Reviewed,
        };
        var calculator = new ProgressionPointBudgetCalculator(
            [characterClass],
            [rule]);

        var exception = Assert.Throws<ProgressionPointBudgetException>(
            () => calculator.Calculate(new ProgressionPointBudgetRequest(
                characterClass.Id,
                "evolution-magic-gladiator",
                220,
                [])));

        Assert.Equal(
            ProgressionPointBudgetErrorCodes.ProgressionRuleNotFound,
            exception.Code);
    }

    private static ProgressionPointBudgetCalculator CreateCalculator() =>
        new(Ruleset.Classes, Ruleset.Rules);

    private static ProgressionPointBudgetRequest ToRequest(
        ProgressionReferenceCase referenceCase) =>
        new(
            referenceCase.ClassId,
            referenceCase.EvolutionId,
            referenceCase.Level,
            referenceCase.CompletedQuestIds);

    private static TheoryData<ProgressionReferenceCase> CreateTheoryData(
        IEnumerable<ProgressionReferenceCase> cases)
    {
        var data = new TheoryData<ProgressionReferenceCase>();
        foreach (var referenceCase in cases)
        {
            data.Add(referenceCase);
        }

        return data;
    }
}
