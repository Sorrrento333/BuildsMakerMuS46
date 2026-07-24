using MuOnline.BuildPlanner.CalculationEngine.Progression;
using MuOnline.BuildPlanner.Domain.Progression;

namespace MuOnline.BuildPlanner.Application.Progression;

public sealed class CalculateProgressionPointBudgetUseCase
{
    private readonly ProgressionPointBudgetCalculator _calculator;

    public CalculateProgressionPointBudgetUseCase(ProgressionRulesetCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        _calculator = new ProgressionPointBudgetCalculator(catalog.Classes, catalog.Rules);
    }

    public ProgressionPointBudgetResult Execute(ProgressionPointBudgetRequest request) =>
        _calculator.Calculate(request);
}
