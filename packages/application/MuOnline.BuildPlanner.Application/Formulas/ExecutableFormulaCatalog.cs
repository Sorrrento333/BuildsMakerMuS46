using System.Collections.Immutable;
using MuOnline.BuildPlanner.Domain.Formulas;

namespace MuOnline.BuildPlanner.Application.Formulas;

public sealed class ExecutableFormulaCatalog
{
    private readonly ImmutableDictionary<FormulaReference, FormulaDefinition> _byReference;

    public ExecutableFormulaCatalog(
        string rulesetId,
        IEnumerable<FormulaDefinition> formulas)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rulesetId);
        ArgumentNullException.ThrowIfNull(formulas);

        var normalizedFormulas = formulas.ToImmutableArray();
        if (normalizedFormulas.Length == 0)
        {
            throw new ArgumentException(
                "At least one executable formula is required.",
                nameof(formulas));
        }

        if (normalizedFormulas.Any(
                formula =>
                    formula.RulesetId != rulesetId ||
                    formula.Status != FormulaStatus.Published))
        {
            throw new ArgumentException(
                "Every executable formula must be published in the catalog ruleset.",
                nameof(formulas));
        }

        if (normalizedFormulas
            .GroupBy(formula => formula.Reference)
            .Any(group => group.Count() > 1))
        {
            throw new ArgumentException(
                "Executable formula references must be unique.",
                nameof(formulas));
        }

        RulesetId = rulesetId;
        Formulas = normalizedFormulas;
        _byReference = Formulas.ToImmutableDictionary(
            formula => formula.Reference,
            formula => formula);
    }

    public string RulesetId { get; }

    public ImmutableArray<FormulaDefinition> Formulas { get; }

    public FormulaDefinition Resolve(FormulaReference reference)
    {
        ArgumentNullException.ThrowIfNull(reference);
        if (_byReference.TryGetValue(reference, out var definition))
        {
            return definition;
        }

        throw new FormulaExecutionException(
            FormulaExecutionErrorCodes.FormulaNotExecutable,
            $"Formula '{reference.Id}' version '{reference.Version}' is not executable.");
    }
}
