using MuOnline.BuildPlanner.CalculationEngine.Formulas;
using MuOnline.BuildPlanner.Domain.Formulas;

namespace MuOnline.BuildPlanner.Application.Formulas;

public sealed class CalculatePublishedFormulaUseCase
{
    private readonly ExecutableFormulaCatalog _catalog;

    public CalculatePublishedFormulaUseCase(ExecutableFormulaCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        _catalog = catalog;
    }

    public FormulaCalculationResult Execute(
        FormulaReference reference,
        FormulaCalculationRequest request)
    {
        ArgumentNullException.ThrowIfNull(reference);
        ArgumentNullException.ThrowIfNull(request);

        var definition = _catalog.Resolve(reference);
        return definition.Program switch
        {
            CheckedIntegerFormulaProgram =>
                CheckedIntegerFormulaInterpreter.Calculate(definition, request),
            CheckedDecimalFormulaProgram =>
                CheckedDecimalFormulaInterpreter.Calculate(definition, request),
            _ => throw new FormulaCalculationException(
                FormulaCalculationErrorCodes.ProgramNotSupported,
                $"Execution model '{definition.Program.ExecutionModel}' is not supported."),
        };
    }
}
