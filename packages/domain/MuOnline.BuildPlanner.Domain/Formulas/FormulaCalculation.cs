using System.Collections.Immutable;

namespace MuOnline.BuildPlanner.Domain.Formulas;

public sealed record FormulaCalculationContext
{
    public FormulaCalculationContext(string characterClassId, string evolutionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterClassId);
        ArgumentException.ThrowIfNullOrWhiteSpace(evolutionId);
        CharacterClassId = characterClassId;
        EvolutionId = evolutionId;
    }

    public string CharacterClassId { get; }

    public string EvolutionId { get; }
}

public sealed class FormulaCalculationRequest
{
    public FormulaCalculationRequest(
        FormulaCalculationContext context,
        IEnumerable<KeyValuePair<string, long>> inputs)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(inputs);

        var inputBuilder = ImmutableDictionary.CreateBuilder<string, long>(
            StringComparer.Ordinal);
        foreach (var input in inputs)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(input.Key);
            if (!inputBuilder.TryAdd(input.Key, input.Value))
            {
                throw new ArgumentException(
                    $"Input ID '{input.Key}' is duplicated.",
                    nameof(inputs));
            }
        }

        Context = context;
        Inputs = inputBuilder.ToImmutable();
    }

    public FormulaCalculationContext Context { get; }

    public ImmutableDictionary<string, long> Inputs { get; }
}

public sealed record FormulaCalculationTraceStep
{
    public FormulaCalculationTraceStep(string stepId, decimal value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stepId);
        StepId = stepId;
        Value = value;
    }

    public string StepId { get; }

    public decimal Value { get; }
}

public sealed class FormulaCalculationTrace
{
    public FormulaCalculationTrace(
        string rulesetId,
        FormulaReference formulaReference,
        FormulaCalculationContext context,
        IEnumerable<KeyValuePair<string, long>> inputs,
        IEnumerable<FormulaCalculationTraceStep> steps,
        FormulaRoundingDefinition rounding,
        decimal rawOutput,
        long visibleOutput,
        IEnumerable<string> evidenceRefs,
        IEnumerable<string>? conflictIds = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rulesetId);
        ArgumentNullException.ThrowIfNull(formulaReference);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(steps);
        ArgumentNullException.ThrowIfNull(rounding);
        ArgumentNullException.ThrowIfNull(evidenceRefs);

        RulesetId = rulesetId;
        FormulaReference = formulaReference;
        Context = context;
        Inputs = inputs.ToImmutableDictionary(
            pair => pair.Key,
            pair => pair.Value,
            StringComparer.Ordinal);
        Steps = steps.ToImmutableArray();
        Rounding = rounding;
        RawOutput = rawOutput;
        VisibleOutput = visibleOutput;
        EvidenceRefs = CopyIds(evidenceRefs);
        ConflictIds = CopyIds(conflictIds);
    }

    public string RulesetId { get; }

    public FormulaReference FormulaReference { get; }

    public FormulaCalculationContext Context { get; }

    public ImmutableDictionary<string, long> Inputs { get; }

    public ImmutableArray<FormulaCalculationTraceStep> Steps { get; }

    public FormulaRoundingDefinition Rounding { get; }

    public decimal RawOutput { get; }

    public long VisibleOutput { get; }

    public ImmutableArray<string> EvidenceRefs { get; }

    public ImmutableArray<string> ConflictIds { get; }

    private static ImmutableArray<string> CopyIds(IEnumerable<string>? ids)
    {
        if (ids is null)
        {
            return [];
        }

        return ids.Select(RequireId).Distinct(StringComparer.Ordinal).ToImmutableArray();
    }

    private static string RequireId(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return id;
    }
}

public sealed record FormulaCalculationResult(
    string OutputId,
    decimal RawOutput,
    long VisibleOutput,
    FormulaCalculationTrace Trace);
