using MuOnline.BuildPlanner.Application.Progression;
using MuOnline.BuildPlanner.CalculationEngine.Stats;
using MuOnline.BuildPlanner.Domain.Progression;
using MuOnline.BuildPlanner.Domain.Stats;

namespace MuOnline.BuildPlanner.Application.Stats;

public sealed class CalculateStatDistributionUseCase
{
    private readonly ProgressionRulesetCatalog _catalog;

    public CalculateStatDistributionUseCase(ProgressionRulesetCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        _catalog = catalog;
    }

    public StatDistributionResult Execute(
        ProgressionPointBudgetResult budget,
        IReadOnlyDictionary<string, long> allocations)
    {
        ArgumentNullException.ThrowIfNull(budget);
        ArgumentNullException.ThrowIfNull(allocations);

        var matchingClasses = _catalog.Classes
            .Where(characterClass =>
                characterClass.Id == budget.CharacterClassId &&
                characterClass.RulesetId == budget.RulesetId)
            .ToArray();
        if (matchingClasses.Length != 1)
        {
            throw new StatDistributionException(
                StatDistributionErrorCodes.BudgetSourceMismatch,
                "The progression budget does not resolve to exactly one character class in the loaded catalog.");
        }

        return StatDistributionCalculator.Calculate(
            new StatDistributionRequest(
                budget,
                matchingClasses[0],
                allocations));
    }
}
