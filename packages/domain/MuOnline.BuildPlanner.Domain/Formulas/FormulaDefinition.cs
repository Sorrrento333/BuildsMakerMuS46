using System.Collections.Immutable;

namespace MuOnline.BuildPlanner.Domain.Formulas;

public enum FormulaStatus
{
    Draft,
    Reviewed,
    Published,
    Deprecated,
}

public enum FormulaConfidence
{
    Unverified,
    Partial,
    Verified,
    Disputed,
    Deprecated,
}

public enum FormulaNumericType
{
    Signed32Bit,
    Signed64Bit,
}

public enum FormulaInputSourceKind
{
    ContextValue,
    FormulaOutput,
}

public enum FormulaBoundsClassification
{
    Technical,
    Factual,
}

public enum FormulaRoundingMode
{
    None,
    Floor,
    Ceiling,
    Truncate,
    HalfUp,
    HalfEven,
}

public sealed record FormulaReference
{
    public FormulaReference(string id, string version)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        Id = id;
        Version = version;
    }

    public string Id { get; }

    public string Version { get; }
}

public sealed class FormulaApplicability
{
    public FormulaApplicability(
        string characterClassId,
        IEnumerable<string> evolutionIds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterClassId);
        ArgumentNullException.ThrowIfNull(evolutionIds);

        var normalizedEvolutionIds = evolutionIds
            .Select(RequireId)
            .ToImmutableHashSet(StringComparer.Ordinal);
        if (normalizedEvolutionIds.Count == 0)
        {
            throw new ArgumentException(
                "At least one evolution ID is required.",
                nameof(evolutionIds));
        }

        CharacterClassId = characterClassId;
        EvolutionIds = normalizedEvolutionIds;
    }

    public string CharacterClassId { get; }

    public ImmutableHashSet<string> EvolutionIds { get; }

    private static string RequireId(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return id;
    }
}

public sealed class FormulaNumericBounds
{
    public FormulaNumericBounds(
        long? minimum,
        bool minimumInclusive,
        long? maximum,
        bool maximumInclusive,
        FormulaBoundsClassification classification,
        IEnumerable<string>? evidenceRefs = null)
    {
        if (minimum is null && maximum is null)
        {
            throw new ArgumentException("At least one numeric bound is required.");
        }

        if (minimum > maximum)
        {
            throw new ArgumentException("Minimum cannot be greater than maximum.");
        }

        Minimum = minimum;
        MinimumInclusive = minimumInclusive;
        Maximum = maximum;
        MaximumInclusive = maximumInclusive;
        Classification = classification;
        EvidenceRefs = CopyIds(evidenceRefs);

        if (classification == FormulaBoundsClassification.Factual &&
            EvidenceRefs.Length == 0)
        {
            throw new ArgumentException(
                "Factual numeric bounds require at least one evidence reference.",
                nameof(evidenceRefs));
        }
    }

    public long? Minimum { get; }

    public bool MinimumInclusive { get; }

    public long? Maximum { get; }

    public bool MaximumInclusive { get; }

    public FormulaBoundsClassification Classification { get; }

    public ImmutableArray<string> EvidenceRefs { get; }

    public bool Contains(long value) =>
        (Minimum is null ||
            (MinimumInclusive ? value >= Minimum.Value : value > Minimum.Value)) &&
        (Maximum is null ||
            (MaximumInclusive ? value <= Maximum.Value : value < Maximum.Value));

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

public sealed record FormulaInputDefinition
{
    public FormulaInputDefinition(
        string id,
        FormulaNumericType numericType,
        string unit,
        FormulaNumericBounds numericBounds,
        string rangeErrorCode,
        FormulaInputSource source)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(unit);
        ArgumentNullException.ThrowIfNull(numericBounds);
        ArgumentException.ThrowIfNullOrWhiteSpace(rangeErrorCode);
        ArgumentNullException.ThrowIfNull(source);

        Id = id;
        NumericType = numericType;
        Unit = unit;
        NumericBounds = numericBounds;
        RangeErrorCode = rangeErrorCode;
        Source = source;
    }

    public string Id { get; }

    public FormulaNumericType NumericType { get; }

    public string Unit { get; }

    public FormulaNumericBounds NumericBounds { get; }

    public string RangeErrorCode { get; }

    public FormulaInputSource Source { get; }
}

public sealed record FormulaInputSource
{
    public FormulaInputSource(FormulaInputSourceKind kind, string valueId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(valueId);
        Kind = kind;
        ValueId = valueId;
    }

    public FormulaInputSourceKind Kind { get; }

    public string ValueId { get; }
}

public sealed record FormulaOutputDefinition
{
    public FormulaOutputDefinition(
        string id,
        string unit,
        FormulaNumericBounds? numericBounds = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(unit);
        Id = id;
        NumericType = FormulaNumericType.Signed64Bit;
        Unit = unit;
        NumericBounds = numericBounds;
    }

    public string Id { get; }

    public FormulaNumericType NumericType { get; }

    public string Unit { get; }

    public FormulaNumericBounds? NumericBounds { get; }
}

public sealed record FormulaRoundingDefinition
{
    public FormulaRoundingDefinition(
        FormulaRoundingMode mode,
        string stageId,
        int? decimalPlaces = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stageId);
        if (decimalPlaces < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(decimalPlaces),
                "Decimal places cannot be negative.");
        }

        Mode = mode;
        StageId = stageId;
        DecimalPlaces = decimalPlaces;
    }

    public FormulaRoundingMode Mode { get; }

    public string StageId { get; }

    public int? DecimalPlaces { get; }
}

public sealed class FormulaTraceDefinition
{
    public FormulaTraceDefinition(
        IEnumerable<string> stepIds,
        string rawOutputStepId,
        string visibleOutputStepId)
    {
        ArgumentNullException.ThrowIfNull(stepIds);
        ArgumentException.ThrowIfNullOrWhiteSpace(rawOutputStepId);
        ArgumentException.ThrowIfNullOrWhiteSpace(visibleOutputStepId);

        var normalizedStepIds = stepIds.Select(RequireId).ToImmutableArray();
        if (normalizedStepIds.Length == 0)
        {
            throw new ArgumentException(
                "At least one trace step ID is required.",
                nameof(stepIds));
        }

        StepIds = normalizedStepIds;
        RawOutputStepId = rawOutputStepId;
        VisibleOutputStepId = visibleOutputStepId;
    }

    public ImmutableArray<string> StepIds { get; }

    public string RawOutputStepId { get; }

    public string VisibleOutputStepId { get; }

    private static string RequireId(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return id;
    }
}

public abstract class FormulaProgram
{
    protected FormulaProgram(string executionModel)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executionModel);
        ExecutionModel = executionModel;
    }

    public string ExecutionModel { get; }
}

public sealed class FormulaDefinition
{
    public FormulaDefinition(
        FormulaReference reference,
        string rulesetId,
        FormulaStatus status,
        FormulaConfidence confidence,
        FormulaApplicability applicability,
        IEnumerable<FormulaInputDefinition> inputs,
        FormulaOutputDefinition output,
        FormulaProgram program,
        FormulaRoundingDefinition rounding,
        FormulaTraceDefinition trace,
        IEnumerable<string> evidenceRefs,
        IEnumerable<string>? conflictIds = null)
    {
        ArgumentNullException.ThrowIfNull(reference);
        ArgumentException.ThrowIfNullOrWhiteSpace(rulesetId);
        ArgumentNullException.ThrowIfNull(applicability);
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(program);
        ArgumentNullException.ThrowIfNull(rounding);
        ArgumentNullException.ThrowIfNull(trace);
        ArgumentNullException.ThrowIfNull(evidenceRefs);

        var normalizedInputs = inputs.ToImmutableArray();
        if (normalizedInputs.Length == 0)
        {
            throw new ArgumentException(
                "At least one input definition is required.",
                nameof(inputs));
        }

        if (normalizedInputs.Any(input => input is null))
        {
            throw new ArgumentException(
                "Input definitions cannot contain null values.",
                nameof(inputs));
        }

        if (normalizedInputs
            .GroupBy(input => input.Id, StringComparer.Ordinal)
            .Any(group => group.Count() > 1))
        {
            throw new ArgumentException(
                "Input definition IDs must be unique.",
                nameof(inputs));
        }

        Reference = reference;
        RulesetId = rulesetId;
        Status = status;
        Confidence = confidence;
        Applicability = applicability;
        Inputs = normalizedInputs;
        Output = output;
        Program = program;
        Rounding = rounding;
        Trace = trace;
        EvidenceRefs = CopyIds(evidenceRefs);
        ConflictIds = CopyIds(conflictIds);
    }

    public FormulaReference Reference { get; }

    public string RulesetId { get; }

    public FormulaStatus Status { get; }

    public FormulaConfidence Confidence { get; }

    public FormulaApplicability Applicability { get; }

    public ImmutableArray<FormulaInputDefinition> Inputs { get; }

    public FormulaOutputDefinition Output { get; }

    public FormulaProgram Program { get; }

    public FormulaRoundingDefinition Rounding { get; }

    public FormulaTraceDefinition Trace { get; }

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
