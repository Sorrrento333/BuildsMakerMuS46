using MuOnline.BuildPlanner.Domain.Stats;

namespace MuOnline.BuildPlanner.CalculationEngine.Stats;

public static class StatDistributionCalculator
{
    public static StatDistributionResult Calculate(StatDistributionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Budget);
        ArgumentNullException.ThrowIfNull(request.CharacterClass);
        ArgumentNullException.ThrowIfNull(request.Allocations);

        var budget = request.Budget;
        var characterClass = request.CharacterClass;
        var allocations = request.Allocations;

        if (budget.RulesetId != characterClass.RulesetId ||
            budget.CharacterClassId != characterClass.Id ||
            !characterClass.ProgressionRuleRefs.Contains(
                budget.ProgressionRuleId,
                StringComparer.Ordinal))
        {
            throw Error(
                StatDistributionErrorCodes.BudgetSourceMismatch,
                "The progression budget does not match the character class definition.");
        }

        var unavailableStat = allocations.Keys.FirstOrDefault(
            statId => !characterClass.StatIds.Contains(statId));
        if (unavailableStat is not null)
        {
            throw Error(
                StatDistributionErrorCodes.StatNotAvailable,
                $"Stat '{unavailableStat}' is not available for class '{characterClass.Id}'.");
        }

        var missingStat = characterClass.StatIds.FirstOrDefault(
            statId => !allocations.ContainsKey(statId));
        if (missingStat is not null)
        {
            throw Error(
                StatDistributionErrorCodes.StatAllocationMissing,
                $"Stat '{missingStat}' has no allocation.");
        }

        var negativeAllocation = allocations.FirstOrDefault(
            allocation => allocation.Value < 0);
        if (negativeAllocation.Value < 0)
        {
            throw Error(
                StatDistributionErrorCodes.AllocationNegative,
                $"Stat '{negativeAllocation.Key}' has a negative allocation.");
        }

        long spentPoints;
        try
        {
            spentPoints = allocations.Values.Aggregate(
                0L,
                (sum, allocation) => checked(sum + allocation));
        }
        catch (OverflowException)
        {
            throw Error(
                StatDistributionErrorCodes.AllocationOverflow,
                "The allocation sum exceeds the supported 64-bit integer range.");
        }

        if (spentPoints > budget.EarnedPoints)
        {
            throw Error(
                StatDistributionErrorCodes.AllocationExceedsEarnedPoints,
                $"Allocated points '{spentPoints}' exceed earned points '{budget.EarnedPoints}'.");
        }

        long remainingPoints;
        try
        {
            remainingPoints = checked(budget.EarnedPoints - spentPoints);
        }
        catch (OverflowException)
        {
            throw Error(
                StatDistributionErrorCodes.AllocationOverflow,
                "The remaining allocation cannot be represented as a 64-bit integer.");
        }

        var normalizedAllocations = characterClass.StatIds
            .Order(StringComparer.Ordinal)
            .ToDictionary(
                statId => statId,
                statId => allocations[statId],
                StringComparer.Ordinal);

        return new StatDistributionResult(
            budget.RulesetId,
            budget.CharacterClassId,
            budget.ProgressionRuleId,
            budget.ProgressionRuleVersion,
            budget.EarnedPoints,
            normalizedAllocations,
            spentPoints,
            remainingPoints);
    }

    private static StatDistributionException Error(string code, string message) =>
        new(code, message);
}
