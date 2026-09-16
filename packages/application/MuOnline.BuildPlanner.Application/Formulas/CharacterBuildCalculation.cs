using System.Collections.Immutable;
using MuOnline.BuildPlanner.Application.Progression;
using MuOnline.BuildPlanner.Application.Stats;
using MuOnline.BuildPlanner.Domain.Formulas;
using MuOnline.BuildPlanner.Domain.Progression;
using MuOnline.BuildPlanner.Domain.Stats;

namespace MuOnline.BuildPlanner.Application.Formulas;

public sealed class CharacterBuildFormulaEvaluation
{
    public CharacterBuildFormulaEvaluation(
        FormulaDefinition formula,
        ImmutableArray<FormulaContextResolutionTraceEntry> contextTrace,
        ImmutableArray<FormulaDependencyResolutionTraceEntry> dependencyTrace,
        FormulaCalculationResult calculation)
    {
        ArgumentNullException.ThrowIfNull(formula);
        ArgumentNullException.ThrowIfNull(calculation);
        Formula = formula;
        ContextTrace = contextTrace;
        DependencyTrace = dependencyTrace;
        Calculation = calculation;
    }

    public FormulaDefinition Formula { get; }

    public ImmutableArray<FormulaContextResolutionTraceEntry> ContextTrace { get; }

    public ImmutableArray<FormulaDependencyResolutionTraceEntry> DependencyTrace { get; }

    public FormulaCalculationResult Calculation { get; }
}

public sealed class CharacterBuildEvaluation
{
    public CharacterBuildEvaluation(
        ResolvedCharacterState state,
        IEnumerable<CharacterBuildFormulaEvaluation> formulaEvaluations)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(formulaEvaluations);

        var orderedEvaluations = formulaEvaluations.ToImmutableArray();
        if (orderedEvaluations.Length == 0)
        {
            throw new ArgumentException(
                "A validated character build must expose at least one applicable formula.",
                nameof(formulaEvaluations));
        }

        State = state;
        Formulas = orderedEvaluations;
    }

    public ResolvedCharacterState State { get; }

    public ImmutableArray<CharacterBuildFormulaEvaluation> Formulas { get; }
}

public sealed class CalculateCharacterBuildUseCase
{
    private readonly ProgressionRulesetCatalog _progressionCatalog;
    private readonly ExecutableFormulaCatalog _formulaCatalog;
    private readonly CalculateProgressionPointBudgetUseCase _progressionUseCase;
    private readonly CalculateStatDistributionUseCase _distributionUseCase;
    private readonly CalculatePublishedFormulaUseCase _formulaUseCase;

    public CalculateCharacterBuildUseCase(
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

    public CharacterBuildEvaluation Execute(
        ProgressionPointBudgetRequest progressionRequest,
        ResetPointInputs resetInputs,
        IReadOnlyDictionary<string, long> allocations)
    {
        ArgumentNullException.ThrowIfNull(progressionRequest);
        ArgumentNullException.ThrowIfNull(resetInputs);
        ArgumentNullException.ThrowIfNull(allocations);

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

        var cache = new Dictionary<FormulaReference, CharacterFormulaEvaluation>();
        var active = new HashSet<FormulaReference>();

        var evaluations = new List<CharacterBuildFormulaEvaluation>();
        foreach (var formula in _formulaCatalog.Formulas
                     .Where(item =>
                         item.Applicability.CharacterClassId == characterClass.Id &&
                         item.Applicability.EvolutionIds.Contains(
                             progressionRequest.EvolutionId))
                     .OrderBy(item => item.Reference.Id, StringComparer.Ordinal)
                     .ThenBy(item => item.Reference.Version, StringComparer.Ordinal))
        {
            var evaluation = EvaluateFormula(formula, state, cache, active);
            evaluations.Add(evaluation);
        }

        if (evaluations.Count == 0)
        {
            throw Error(
                FormulaContextErrorCodes.NoApplicableFormula,
                $"No published formula is applicable to class '{characterClass.Id}' " +
                $"evolution '{progressionRequest.EvolutionId}'.");
        }

        return new CharacterBuildEvaluation(state, evaluations);
    }

    private CharacterBuildFormulaEvaluation EvaluateFormula(
        FormulaDefinition formula,
        ResolvedCharacterState state,
        IDictionary<FormulaReference, CharacterFormulaEvaluation> cache,
        ISet<FormulaReference> active)
    {
        if (!active.Add(formula.Reference))
        {
            throw Error(
                FormulaContextErrorCodes.DependencyCycle,
                $"Formula dependency cycle detected at '{formula.Reference.Id}' " +
                $"version '{formula.Reference.Version}'.");
        }

        try
        {
            var resolvedContext = FormulaContextValueResolver.Resolve(formula, state);
            var inputs = resolvedContext.Inputs.ToBuilder();
            var contextTrace = resolvedContext.Trace.ToBuilder();
            var dependencyTrace =
                ImmutableArray.CreateBuilder<FormulaDependencyResolutionTraceEntry>();

            foreach (var input in formula.Inputs.Where(
                         item =>
                             item.Source.Kind == FormulaInputSourceKind.FormulaOutput))
            {
                var dependencyReference = input.Source.FormulaReference
                    ?? throw Error(
                        FormulaContextErrorCodes.DependencyIncoherent,
                        $"Formula input '{input.Id}' has no dependency reference.");
                var outputStage = input.Source.OutputStage
                    ?? throw Error(
                        FormulaContextErrorCodes.DependencyIncoherent,
                        $"Formula input '{input.Id}' has no dependency output stage.");
                var dependency = _formulaCatalog.Resolve(dependencyReference);
                if (!cache.TryGetValue(dependencyReference, out var dependencyEvaluation))
                {
                    var evaluatedDependency = EvaluateFormula(
                        dependency,
                        state,
                        cache,
                        active);
                    dependencyEvaluation = new CharacterFormulaEvaluation(
                        evaluatedDependency);
                    cache.Add(dependencyReference, dependencyEvaluation);
                }

                dependencyTrace.AddRange(dependencyEvaluation.DependencyTrace);
                var selectedValue = outputStage switch
                {
                    FormulaOutputStage.Raw => dependencyEvaluation.Calculation.RawOutput,
                    FormulaOutputStage.Visible =>
                        dependencyEvaluation.Calculation.VisibleOutput,
                    _ => throw Error(
                        FormulaContextErrorCodes.DependencyIncoherent,
                        $"Formula input '{input.Id}' selects an unknown output stage."),
                };
                inputs.Add(input.Id, selectedValue);
                dependencyTrace.Add(new FormulaDependencyResolutionTraceEntry(
                    formula.Reference,
                    input.Id,
                    dependencyReference,
                    outputStage,
                    selectedValue,
                    dependencyEvaluation.ContextTrace,
                    dependencyEvaluation.Calculation.Trace));
            }

            var formulaResult = _formulaUseCase.Execute(
                formula.Reference,
                new FormulaCalculationRequest(
                    new FormulaCalculationContext(
                        state.ProgressionRequest.ClassId,
                        state.ProgressionRequest.EvolutionId),
                    inputs));
            return new CharacterBuildFormulaEvaluation(
                formula,
                contextTrace.ToImmutable(),
                dependencyTrace.ToImmutable(),
                formulaResult);
        }
        finally
        {
            active.Remove(formula.Reference);
        }
    }

    private sealed record CharacterFormulaEvaluation(
        CharacterBuildFormulaEvaluation Evaluation)
    {
        public FormulaCalculationResult Calculation => Evaluation.Calculation;

        public ImmutableArray<FormulaContextResolutionTraceEntry> ContextTrace =>
            Evaluation.ContextTrace;

        public ImmutableArray<FormulaDependencyResolutionTraceEntry> DependencyTrace =>
            Evaluation.DependencyTrace;
    }

    private static FormulaContextException Error(string code, string message) =>
        new(code, message);
}