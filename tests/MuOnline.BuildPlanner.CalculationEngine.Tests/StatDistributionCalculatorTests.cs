using MuOnline.BuildPlanner.CalculationEngine.Stats;
using MuOnline.BuildPlanner.Domain.Progression;
using MuOnline.BuildPlanner.Domain.Stats;
using Xunit;

namespace MuOnline.BuildPlanner.CalculationEngine.Tests;

public sealed class StatDistributionCalculatorTests
{
    private const string RulesetId = "synthetic-ruleset";
    private const string ClassId = "class-synthetic";
    private const string RuleId = "progression-synthetic";

    private static readonly CharacterProgressionDefinition CharacterClass = new(
        ClassId,
        RulesetId,
        new HashSet<string>(["stat-alpha", "stat-beta"], StringComparer.Ordinal),
        new HashSet<string>(["evolution-synthetic"], StringComparer.Ordinal),
        [RuleId]);

    [Fact]
    public void PartialDistributionCalculatesSpentAndRemainingPoints()
    {
        var result = Calculate(new Dictionary<string, long>
        {
            ["stat-alpha"] = 4,
            ["stat-beta"] = 3,
        });

        Assert.Equal(7, result.SpentPoints);
        Assert.Equal(3, result.RemainingPoints);
        Assert.Equal(10, result.EarnedPoints);
        Assert.Equal(0, result.ResetInputs.ResetCount);
        Assert.Equal(0, result.ResetInputs.PointsPerReset);
        Assert.Equal(0, result.ResetPoints);
        Assert.Equal(10, result.TotalDistributablePoints);
        Assert.Equal(RulesetId, result.RulesetId);
        Assert.Equal(ClassId, result.CharacterClassId);
        Assert.Equal(RuleId, result.ProgressionRuleId);
        Assert.Equal("1.0.0", result.ProgressionRuleVersion);
        Assert.Equal(4, result.Allocations["stat-alpha"]);
        Assert.Equal(3, result.Allocations["stat-beta"]);
    }

    [Fact]
    public void ResetConfigurationAddsPointsToDistributableBudget()
    {
        var result = Calculate(
            new Dictionary<string, long>
            {
                ["stat-alpha"] = 104,
                ["stat-beta"] = 103,
            },
            new ResetPointInputs(2, 100));

        Assert.Equal(10, result.EarnedPoints);
        Assert.Equal(2, result.ResetInputs.ResetCount);
        Assert.Equal(100, result.ResetInputs.PointsPerReset);
        Assert.Equal(200, result.ResetPoints);
        Assert.Equal(210, result.TotalDistributablePoints);
        Assert.Equal(207, result.SpentPoints);
        Assert.Equal(3, result.RemainingPoints);
    }

    [Fact]
    public void ExactDistributionLeavesNoRemainingPoints()
    {
        var result = Calculate(new Dictionary<string, long>
        {
            ["stat-alpha"] = 4,
            ["stat-beta"] = 6,
        });

        Assert.Equal(10, result.SpentPoints);
        Assert.Equal(0, result.RemainingPoints);
    }

    [Fact]
    public void NegativeAllocationIsRejected()
    {
        AssertError(
            new Dictionary<string, long>
            {
                ["stat-alpha"] = -1,
                ["stat-beta"] = 0,
            },
            StatDistributionErrorCodes.AllocationNegative);
    }

    [Fact]
    public void UnavailableStatIsRejected()
    {
        AssertError(
            new Dictionary<string, long>
            {
                ["stat-alpha"] = 0,
                ["stat-beta"] = 0,
                ["stat-gamma"] = 1,
            },
            StatDistributionErrorCodes.StatNotAvailable);
    }

    [Fact]
    public void AllocationAboveBudgetIsRejected()
    {
        AssertError(
            new Dictionary<string, long>
            {
                ["stat-alpha"] = 11,
                ["stat-beta"] = 0,
            },
            StatDistributionErrorCodes.AllocationExceedsEarnedPoints);
    }

    [Fact]
    public void MissingStatAllocationIsRejected()
    {
        AssertError(
            new Dictionary<string, long>
            {
                ["stat-alpha"] = 1,
            },
            StatDistributionErrorCodes.StatAllocationMissing);
    }

    [Theory]
    [InlineData("other-ruleset", ClassId, RuleId)]
    [InlineData(RulesetId, "class-other", RuleId)]
    [InlineData(RulesetId, ClassId, "progression-other")]
    public void BudgetSourceMismatchIsRejected(
        string rulesetId,
        string classId,
        string progressionRuleId)
    {
        var request = new StatDistributionRequest(
            CreateBudget(rulesetId, classId, progressionRuleId),
            CharacterClass,
            new ResetPointInputs(0, 0),
            new Dictionary<string, long>
            {
                ["stat-alpha"] = 0,
                ["stat-beta"] = 0,
            });

        var exception = Assert.Throws<StatDistributionException>(
            () => StatDistributionCalculator.Calculate(request));

        Assert.Equal(StatDistributionErrorCodes.BudgetSourceMismatch, exception.Code);
    }

    [Fact]
    public void AllocationSumOverflowIsRejected()
    {
        var request = new StatDistributionRequest(
            CreateBudget(earnedPoints: long.MaxValue),
            CharacterClass,
            new ResetPointInputs(0, 0),
            new Dictionary<string, long>
            {
                ["stat-alpha"] = long.MaxValue,
                ["stat-beta"] = 1,
            });

        var exception = Assert.Throws<StatDistributionException>(
            () => StatDistributionCalculator.Calculate(request));

        Assert.Equal(StatDistributionErrorCodes.AllocationOverflow, exception.Code);
    }

    [Theory]
    [InlineData(-1, 0, StatDistributionErrorCodes.ResetCountNegative)]
    [InlineData(0, -1, StatDistributionErrorCodes.PointsPerResetNegative)]
    public void NegativeResetInputsAreRejected(
        long resetCount,
        long pointsPerReset,
        string expectedCode)
    {
        var exception = Assert.Throws<StatDistributionException>(
            () => Calculate(
                new Dictionary<string, long>
                {
                    ["stat-alpha"] = 0,
                    ["stat-beta"] = 0,
                },
                new ResetPointInputs(resetCount, pointsPerReset)));

        Assert.Equal(expectedCode, exception.Code);
    }

    [Fact]
    public void ResetPointProductOverflowIsRejected()
    {
        var exception = Assert.Throws<StatDistributionException>(
            () => Calculate(
                new Dictionary<string, long>
                {
                    ["stat-alpha"] = 0,
                    ["stat-beta"] = 0,
                },
                new ResetPointInputs(long.MaxValue, 2)));

        Assert.Equal(StatDistributionErrorCodes.ResetPointsOverflow, exception.Code);
    }

    [Fact]
    public void TotalDistributablePointOverflowIsRejected()
    {
        var request = new StatDistributionRequest(
            CreateBudget(earnedPoints: long.MaxValue),
            CharacterClass,
            new ResetPointInputs(1, 1),
            new Dictionary<string, long>
            {
                ["stat-alpha"] = 0,
                ["stat-beta"] = 0,
            });

        var exception = Assert.Throws<StatDistributionException>(
            () => StatDistributionCalculator.Calculate(request));

        Assert.Equal(
            StatDistributionErrorCodes.TotalDistributablePointsOverflow,
            exception.Code);
    }

    private static StatDistributionResult Calculate(
        IReadOnlyDictionary<string, long> allocations,
        ResetPointInputs? resetInputs = null) =>
        StatDistributionCalculator.Calculate(
            new StatDistributionRequest(
                CreateBudget(),
                CharacterClass,
                resetInputs ?? new ResetPointInputs(0, 0),
                allocations));

    private static void AssertError(
        IReadOnlyDictionary<string, long> allocations,
        string expectedCode)
    {
        var exception = Assert.Throws<StatDistributionException>(
            () => Calculate(allocations));

        Assert.Equal(expectedCode, exception.Code);
    }

    private static ProgressionPointBudgetResult CreateBudget(
        string rulesetId = RulesetId,
        string classId = ClassId,
        string progressionRuleId = RuleId,
        long earnedPoints = 10) =>
        new(
            rulesetId,
            classId,
            progressionRuleId,
            "1.0.0",
            earnedPoints,
            []);
}
