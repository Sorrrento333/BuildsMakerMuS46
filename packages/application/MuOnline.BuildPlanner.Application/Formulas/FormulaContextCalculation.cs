using System.Collections.Immutable;
using MuOnline.BuildPlanner.Application.Progression;
using MuOnline.BuildPlanner.Application.Stats;
using MuOnline.BuildPlanner.Domain.Formulas;
using MuOnline.BuildPlanner.Domain.Progression;
using MuOnline.BuildPlanner.Domain.Stats;

namespace MuOnline.BuildPlanner.Application.Formulas;

public static class FormulaContextErrorCodes
{
    public const string StateMismatch = "formula-context-state-mismatch";
    public const string SourceNotSupported = "formula-context-source-not-supported";
    public const string ValueNotResolvable = "formula-context-value-not-resolvable";
    public const string BaseStatMissing = "formula-context-base-stat-missing";
    public const string AllocationMissing = "formula-context-allocation-missing";
    public const string ArithmeticOverflow = "formula-context-arithmetic-overflow";
}

public sealed class FormulaContextException : Exception
{
    public FormulaContextException(string code, string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
    }

    public string Code { get; }
}

public enum FormulaContextResolutionKind
{
    CharacterLevel,
    ResolvedStat,
}

public sealed class FormulaContextResolutionTraceEntry
{
    public FormulaContextResolutionTraceEntry(
        string inputId,
        string contextValueId,
        long resolvedValue,
        string rulesetId,
        string characterClassId,
        string evolutionId,
        FormulaContextResolutionKind kind,
        int? characterLevel = null,
        string? statId = null,
        long? baseValue = null,
        long? allocation = null,
        IEnumerable<string>? evidenceRefs = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputId);
        ArgumentException.ThrowIfNullOrWhiteSpace(contextValueId);
        ArgumentException.ThrowIfNullOrWhiteSpace(rulesetId);
        ArgumentException.ThrowIfNullOrWhiteSpace(characterClassId);
        ArgumentException.ThrowIfNullOrWhiteSpace(evolutionId);

        InputId = inputId;
        ContextValueId = contextValueId;
        ResolvedValue = resolvedValue;
        RulesetId = rulesetId;
        CharacterClassId = characterClassId;
        EvolutionId = evolutionId;
        Kind = kind;
        CharacterLevel = characterLevel;
        StatId = statId;
        BaseValue = baseValue;
        Allocation = allocation;
        EvidenceRefs = (evidenceRefs ?? [])
            .Select(RequireId)
            .Distinct(StringComparer.Ordinal)
            .ToImmutableArray();
    }

    public string InputId { get; }

    public string ContextValueId { get; }

    public long ResolvedValue { get; }

    public string RulesetId { get; }

    public string CharacterClassId { get; }

    public string EvolutionId { get; }

    public FormulaContextResolutionKind Kind { get; }

    public int? CharacterLevel { get; }

    public string? StatId { get; }

    public long? BaseValue { get; }

    public long? Allocation { get; }

    public ImmutableArray<string> EvidenceRefs { get; }

    private static string RequireId(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return id;
    }
}

public sealed class ResolvedCharacterState
{
    public ResolvedCharacterState(
        ProgressionPointBudgetRequest progressionRequest,
        ProgressionPointBudgetResult budget,
        StatDistributionResult distribution,
        CharacterProgressionDefinition characterClass)
    {
        ArgumentNullException.ThrowIfNull(progressionRequest);
        ArgumentNullException.ThrowIfNull(budget);
        ArgumentNullException.ThrowIfNull(distribution);
        ArgumentNullException.ThrowIfNull(characterClass);

        if (progressionRequest.ClassId != characterClass.Id ||
            !characterClass.EvolutionIds.Contains(progressionRequest.EvolutionId) ||
            budget.RulesetId != characterClass.RulesetId ||
            budget.CharacterClassId != characterClass.Id ||
            distribution.RulesetId != characterClass.RulesetId ||
            distribution.CharacterClassId != characterClass.Id ||
            distribution.ProgressionRuleId != budget.ProgressionRuleId ||
            distribution.ProgressionRuleVersion != budget.ProgressionRuleVersion)
        {
            throw Error(
                FormulaContextErrorCodes.StateMismatch,
                "The validated progression, distribution, and character definition do not describe one character state.");
        }

        ProgressionRequest = new ProgressionPointBudgetRequest(
            progressionRequest.ClassId,
            progressionRequest.EvolutionId,
            progressionRequest.Level,
            progressionRequest.CompletedQuestIds
                .ToImmutableHashSet(StringComparer.Ordinal));
        Budget = budget with
        {
            Contributions = budget.Contributions.ToImmutableArray(),
        };
        Distribution = distribution with
        {
            Allocations = distribution.Allocations.ToImmutableDictionary(
                StringComparer.Ordinal),
        };
        CharacterClass = characterClass;
    }

    public ProgressionPointBudgetRequest ProgressionRequest { get; }

    public ProgressionPointBudgetResult Budget { get; }

    public StatDistributionResult Distribution { get; }

    public CharacterProgressionDefinition CharacterClass { get; }

    private static FormulaContextException Error(string code, string message) =>
        new(code, message);
}

public sealed class CharacterFormulaCalculationResult
{
    public CharacterFormulaCalculationResult(
        ResolvedCharacterState state,
        IEnumerable<FormulaContextResolutionTraceEntry> contextTrace,
        FormulaCalculationResult formula)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(contextTrace);
        ArgumentNullException.ThrowIfNull(formula);
        State = state;
        ContextTrace = contextTrace.ToImmutableArray();
        Formula = formula;
    }

    public ResolvedCharacterState State { get; }

    public ImmutableArray<FormulaContextResolutionTraceEntry> ContextTrace { get; }

    public FormulaCalculationResult Formula { get; }
}

public static class FormulaContextValueResolver
{
    private const string CharacterLevelValueId = "character-level";
    private const string ResolvedStatPrefix = "resolved-";

    public static (
        ImmutableDictionary<string, long> Inputs,
        ImmutableArray<FormulaContextResolutionTraceEntry> Trace)
        Resolve(
            FormulaDefinition formula,
            ResolvedCharacterState state)
    {
        ArgumentNullException.ThrowIfNull(formula);
        ArgumentNullException.ThrowIfNull(state);

        EnsureStateMatchesFormula(formula, state);
        var inputs = ImmutableDictionary.CreateBuilder<string, long>(
            StringComparer.Ordinal);
        var trace = ImmutableArray.CreateBuilder<FormulaContextResolutionTraceEntry>();

        foreach (var input in formula.Inputs)
        {
            if (input.Source.Kind != FormulaInputSourceKind.ContextValue)
            {
                throw Error(
                    FormulaContextErrorCodes.SourceNotSupported,
                    $"Formula input '{input.Id}' uses unsupported source kind '{input.Source.Kind}'.");
            }

            if (input.Source.ValueId == CharacterLevelValueId)
            {
                var level = state.ProgressionRequest.Level;
                inputs.Add(input.Id, level);
                trace.Add(new FormulaContextResolutionTraceEntry(
                    input.Id,
                    input.Source.ValueId,
                    level,
                    state.CharacterClass.RulesetId,
                    state.CharacterClass.Id,
                    state.ProgressionRequest.EvolutionId,
                    FormulaContextResolutionKind.CharacterLevel,
                    characterLevel: level));
                continue;
            }

            if (!input.Source.ValueId.StartsWith(
                    ResolvedStatPrefix,
                    StringComparison.Ordinal))
            {
                throw Error(
                    FormulaContextErrorCodes.ValueNotResolvable,
                    $"Context value '{input.Source.ValueId}' cannot be resolved from the character state.");
            }

            var statId = input.Source.ValueId[ResolvedStatPrefix.Length..];
            if (string.IsNullOrWhiteSpace(statId) ||
                !state.CharacterClass.StatIds.Contains(statId))
            {
                throw Error(
                    FormulaContextErrorCodes.ValueNotResolvable,
                    $"Context value '{input.Source.ValueId}' does not identify a stat available to the character class.");
            }

            if (!state.CharacterClass.BaseStats.TryGetValue(
                    statId,
                    out var baseStat))
            {
                throw Error(
                    FormulaContextErrorCodes.BaseStatMissing,
                    $"Canonical base value for stat '{statId}' is missing.");
            }

            if (!state.Distribution.Allocations.TryGetValue(
                    statId,
                    out var allocation))
            {
                throw Error(
                    FormulaContextErrorCodes.AllocationMissing,
                    $"Validated allocation for stat '{statId}' is missing.");
            }

            long resolvedValue;
            try
            {
                resolvedValue = checked(baseStat.BaseValue + allocation);
            }
            catch (OverflowException)
            {
                throw Error(
                    FormulaContextErrorCodes.ArithmeticOverflow,
                    $"Base value plus allocation for stat '{statId}' exceeds the signed 64-bit range.");
            }

            inputs.Add(input.Id, resolvedValue);
            trace.Add(new FormulaContextResolutionTraceEntry(
                input.Id,
                input.Source.ValueId,
                resolvedValue,
                state.CharacterClass.RulesetId,
                state.CharacterClass.Id,
                state.ProgressionRequest.EvolutionId,
                FormulaContextResolutionKind.ResolvedStat,
                statId: statId,
                baseValue: baseStat.BaseValue,
                allocation: allocation,
                evidenceRefs: baseStat.EvidenceRefs));
        }

        return (inputs.ToImmutable(), trace.ToImmutable());
    }

    private static void EnsureStateMatchesFormula(
        FormulaDefinition formula,
        ResolvedCharacterState state)
    {
        if (formula.RulesetId != state.CharacterClass.RulesetId ||
            formula.Applicability.CharacterClassId != state.CharacterClass.Id ||
            !formula.Applicability.EvolutionIds.Contains(
                state.ProgressionRequest.EvolutionId))
        {
            throw Error(
                FormulaContextErrorCodes.StateMismatch,
                "The resolved character state is not applicable to the requested formula.");
        }
    }

    private static FormulaContextException Error(string code, string message) =>
        new(code, message);
}

public sealed class CalculateCharacterFormulaUseCase
{
    private readonly ProgressionRulesetCatalog _progressionCatalog;
    private readonly ExecutableFormulaCatalog _formulaCatalog;
    private readonly CalculateProgressionPointBudgetUseCase _progressionUseCase;
    private readonly CalculateStatDistributionUseCase _distributionUseCase;
    private readonly CalculatePublishedFormulaUseCase _formulaUseCase;

    public CalculateCharacterFormulaUseCase(
        ProgressionRulesetCatalog progressionCatalog,
        ExecutableFormulaCatalog formulaCatalog)
    {
        ArgumentNullException.ThrowIfNull(progressionCatalog);
        ArgumentNullException.ThrowIfNull(formulaCatalog);
        _progressionCatalog = progressionCatalog;
        _formulaCatalog = formulaCatalog;
        _progressionUseCase = new CalculateProgressionPointBudgetUseCase(
            progressionCatalog);
        _distributionUseCase = new CalculateStatDistributionUseCase(
            progressionCatalog);
        _formulaUseCase = new CalculatePublishedFormulaUseCase(formulaCatalog);
    }

    public CharacterFormulaCalculationResult Execute(
        FormulaReference reference,
        ProgressionPointBudgetRequest progressionRequest,
        ResetPointInputs resetInputs,
        IReadOnlyDictionary<string, long> allocations)
    {
        ArgumentNullException.ThrowIfNull(reference);
        ArgumentNullException.ThrowIfNull(progressionRequest);
        ArgumentNullException.ThrowIfNull(resetInputs);
        ArgumentNullException.ThrowIfNull(allocations);

        var formula = _formulaCatalog.Resolve(reference);
        var budget = _progressionUseCase.Execute(progressionRequest);
        var distribution = _distributionUseCase.Execute(
            budget,
            resetInputs,
            allocations);
        var characterClass = _progressionCatalog.Classes.SingleOrDefault(
            item =>
                item.Id == budget.CharacterClassId &&
                item.RulesetId == budget.RulesetId)
            ?? throw Error(
                FormulaContextErrorCodes.StateMismatch,
                "The validated budget does not resolve to one character definition.");
        var state = new ResolvedCharacterState(
            progressionRequest,
            budget,
            distribution,
            characterClass);
        var resolved = FormulaContextValueResolver.Resolve(formula, state);
        var formulaResult = _formulaUseCase.Execute(
            reference,
            new FormulaCalculationRequest(
                new FormulaCalculationContext(
                    progressionRequest.ClassId,
                    progressionRequest.EvolutionId),
                resolved.Inputs));

        return new CharacterFormulaCalculationResult(
            state,
            resolved.Trace,
            formulaResult);
    }

    private static FormulaContextException Error(string code, string message) =>
        new(code, message);
}
