using MuOnline.BuildPlanner.Application.Formulas;
using MuOnline.BuildPlanner.Application.Items;
using MuOnline.BuildPlanner.Application.Progression;
using MuOnline.BuildPlanner.Domain.Formulas;
using MuOnline.BuildPlanner.Domain.Progression;
using MuOnline.BuildPlanner.Domain.Stats;

namespace MuOnline.BuildPlanner.Application.Builds;

public static class BuildComparisonErrorCodes
{
    public const string SameBuild = "compare-same-build";
    public const string UnknownClass = "compare-unknown-class";
    public const string StatsMismatch = "compare-stats-mismatch";
    public const string UnknownBreakpoint = "compare-unknown-breakpoint";
    public const string InvalidScenario = "compare-invalid-scenario";
}

public static class ComparisonWarningCodes
{
    public const string TargetMissed = "compare-warning-target-missed";
    public const string RequirementUnmet = "compare-warning-requirement-unmet";
}

public sealed class BuildComparisonException : Exception
{
    public BuildComparisonException(string code, string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
    }

    public string Code { get; }
}

public sealed record StatDifference(
    string StatId,
    long? FirstValue,
    long? SecondValue,
    long? AbsoluteDifference);

public sealed record DerivedDifference(
    string FormulaId,
    string FormulaVersion,
    string OutputId,
    string OutputUnit,
    long FirstVisible,
    long SecondVisible,
    long AbsoluteDifference,
    decimal? PercentDifference);

public sealed record DerivedOnlyOnSide(
    string FormulaId,
    string FormulaVersion,
    string OutputId,
    string OutputUnit,
    long VisibleValue);

public sealed record BuildComparison(
    string FirstBuildId,
    string SecondBuildId,
    IReadOnlyList<StatDifference> StatDifferences,
    IReadOnlyList<DerivedDifference> DerivedDifferences,
    IReadOnlyList<DerivedOnlyOnSide> OnlyInFirst,
    IReadOnlyList<DerivedOnlyOnSide> OnlyInSecond,
    ComparisonScenario? Scenario,
    IReadOnlyList<FormulaReference> FocusedReferences,
    IReadOnlyList<BreakpointResult> BreakpointResults,
    IReadOnlyList<ComparisonWarning> Warnings);

public enum ComparisonModality
{
    Pvm,
    Pvp,
    Hybrid,
}

public enum ComparisonSide
{
    First,
    Second,
}

public enum BreakpointKind
{
    Stat,
    Derived,
}

public sealed record ComparisonScenario(
    ComparisonModality Modality,
    string? DisplayName,
    string? Objective,
    string? Notes);

public sealed record BreakpointTarget(
    BreakpointKind Kind,
    string Key,
    long Target);

public sealed record BuildComparisonOptions(
    ComparisonScenario? Scenario,
    IReadOnlyList<BreakpointTarget> BreakpointTargets)
{
    public static BuildComparisonOptions Empty { get; } = new(null, []);
}

public sealed record BreakpointResult(
    BreakpointTarget Target,
    long? FirstValue,
    long? SecondValue,
    bool? MeetsFirst,
    bool? MeetsSecond,
    long? FirstMargin,
    long? SecondMargin);

public sealed record ComparisonWarning(
    string Code,
    ComparisonSide Side,
    string Detail);

public sealed class CompareBuildsUseCase
{
    private readonly ProgressionRulesetCatalog _progressionCatalog;
    private readonly CalculateCharacterBuildUseCase _buildUseCase;
    private readonly ItemCatalog? _itemCatalog;

    public CompareBuildsUseCase(
        ProgressionRulesetCatalog progressionCatalog,
        ExecutableFormulaCatalog formulaCatalog,
        ItemCatalog? itemCatalog = null)
    {
        ArgumentNullException.ThrowIfNull(progressionCatalog);
        ArgumentNullException.ThrowIfNull(formulaCatalog);
        _progressionCatalog = progressionCatalog;
        _buildUseCase = new CalculateCharacterBuildUseCase(
            progressionCatalog,
            formulaCatalog);
        _itemCatalog = itemCatalog;
    }

    public BuildComparison Execute(CharacterBuild first, CharacterBuild second) =>
        Execute(first, second, null);

    public BuildComparison Execute(
        CharacterBuild first,
        CharacterBuild second,
        BuildComparisonOptions? options)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);

        var effectiveOptions = options ?? BuildComparisonOptions.Empty;
        ValidateScenario(effectiveOptions.Scenario);

        if (string.Equals(first.Id, second.Id, StringComparison.Ordinal))
        {
            throw Error(
                BuildComparisonErrorCodes.SameBuild,
                $"Cannot compare build '{first.Id}' with itself.");
        }

        var firstEvaluation = Evaluate(first);
        var secondEvaluation = Evaluate(second);
        var statDifferences = CompareStats(first.Stats, second.Stats);
        var derivedDifferences = CompareDerived(firstEvaluation, secondEvaluation);
        var breakpointResults = CompareBreakpoints(
            first,
            second,
            firstEvaluation,
            secondEvaluation,
            effectiveOptions);

        return new BuildComparison(
            first.Id,
            second.Id,
            statDifferences,
            derivedDifferences,
            OnlyOnSide(firstEvaluation, secondEvaluation),
            OnlyOnSide(secondEvaluation, firstEvaluation),
            effectiveOptions.Scenario,
            FocusedReferences(derivedDifferences, effectiveOptions.Scenario),
            breakpointResults,
            CollectWarnings(first, second, breakpointResults));
    }

    private CharacterBuildEvaluation Evaluate(CharacterBuild build)
    {
        var characterClass = _progressionCatalog.Classes.SingleOrDefault(
                item => item.Id == build.CharacterClassId)
            ?? throw Error(
                BuildComparisonErrorCodes.UnknownClass,
                $"Build '{build.Id}' references the unknown class " +
                $"'{build.CharacterClassId}'.");

        if (!build.Stats.Keys.ToHashSet(StringComparer.Ordinal)
                .SetEquals(characterClass.StatIds))
        {
            throw Error(
                BuildComparisonErrorCodes.StatsMismatch,
                $"Build '{build.Id}' stats do not match the stats declared " +
                $"by class '{characterClass.Id}'.");
        }

        var allocations = new Dictionary<string, long>(StringComparer.Ordinal);
        foreach (var statId in characterClass.StatIds)
        {
            var baseValue = characterClass.BaseStats[statId].BaseValue;
            var finalValue = build.Stats[statId];
            if (finalValue < baseValue)
            {
                throw Error(
                    BuildComparisonErrorCodes.StatsMismatch,
                    $"Build '{build.Id}' stat '{statId}' is below its canonical base value.");
            }

            allocations[statId] = checked(finalValue - baseValue);
        }

        return _buildUseCase.Execute(
            new ProgressionPointBudgetRequest(
                build.CharacterClassId,
                build.EvolutionId,
                build.Level,
                build.QuestIds),
            new ResetPointInputs(build.ResetCount, build.PointsPerReset),
            allocations);
    }

    private static void ValidateScenario(ComparisonScenario? scenario)
    {
        if (scenario is null)
        {
            return;
        }

        if (!Enum.IsDefined(scenario.Modality))
        {
            throw Error(
                BuildComparisonErrorCodes.InvalidScenario,
                "The comparison scenario declares an unknown modality.");
        }

        if (scenario.DisplayName is not null &&
            string.IsNullOrWhiteSpace(scenario.DisplayName))
        {
            throw Error(
                BuildComparisonErrorCodes.InvalidScenario,
                "The comparison scenario declares an empty display name.");
        }
    }

    private static FormulaReference[] FocusedReferences(
        IReadOnlyList<DerivedDifference> derivedDifferences,
        ComparisonScenario? scenario)
    {
        if (scenario is null || scenario.Modality == ComparisonModality.Hybrid)
        {
            return [];
        }

        var marker = scenario.Modality == ComparisonModality.Pvm ? "-pvm-" : "-pvp-";
        return derivedDifferences
            .Where(item => item.FormulaId.Contains(marker, StringComparison.Ordinal))
            .OrderBy(item => item.FormulaId, StringComparer.Ordinal)
            .ThenBy(item => item.FormulaVersion, StringComparer.Ordinal)
            .Select(item => new FormulaReference(item.FormulaId, item.FormulaVersion))
            .ToArray();
    }

    private static BreakpointResult[] CompareBreakpoints(
        CharacterBuild first,
        CharacterBuild second,
        CharacterBuildEvaluation firstEvaluation,
        CharacterBuildEvaluation secondEvaluation,
        BuildComparisonOptions options)
    {
        var firstDerived = firstEvaluation.Formulas.ToDictionary(
            item => item.Formula.Reference,
            item => item.Calculation.VisibleOutput);
        var secondDerived = secondEvaluation.Formulas.ToDictionary(
            item => item.Formula.Reference,
            item => item.Calculation.VisibleOutput);
        foreach (var target in options.BreakpointTargets)
        {
            if (target.Kind == BreakpointKind.Derived &&
                TryParseFormulaReference(target.Key, out var reference) &&
                reference is not null &&
                !firstDerived.ContainsKey(reference) &&
                !secondDerived.ContainsKey(reference))
            {
                throw Error(
                    BuildComparisonErrorCodes.UnknownBreakpoint,
                    $"The comparison declares a breakpoint for the unknown derived " +
                    $"'{target.Key}'. No evaluated formula matches that reference.");
            }
        }

        return options.BreakpointTargets
            .Select(target => CompareBreakpoint(
                target,
                ResolveBreakpointValue(target, first, firstDerived),
                ResolveBreakpointValue(target, second, secondDerived)))
            .ToArray();
    }

    private static long? ResolveBreakpointValue(
        BreakpointTarget target,
        CharacterBuild build,
        Dictionary<FormulaReference, long> derived)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (target.Kind == BreakpointKind.Stat)
        {
            if (string.IsNullOrWhiteSpace(target.Key))
            {
                throw Error(
                    BuildComparisonErrorCodes.UnknownBreakpoint,
                    "The comparison declares a breakpoint with an empty stat key.");
            }

            if (!build.Stats.TryGetValue(target.Key, out var statValue))
            {
                var knownStats = string.Join(", ", build.Stats.Keys.Order(StringComparer.Ordinal));
                throw Error(
                    BuildComparisonErrorCodes.UnknownBreakpoint,
                    $"The comparison declares a breakpoint for the unknown stat " +
                    $"'{target.Key}'. Known stats: {knownStats}.");
            }

            return statValue;
        }

        if (!TryParseFormulaReference(target.Key, out var reference) || reference is null)
        {
            throw Error(
                BuildComparisonErrorCodes.UnknownBreakpoint,
                $"The comparison declares a breakpoint for the malformed derived " +
                $"key '{target.Key}'. Expected 'formula-id@version'.");
        }

        if (!derived.TryGetValue(reference, out var visibleOutput))
        {
            return null;
        }

        return visibleOutput;
    }

    private static bool TryParseFormulaReference(
        string key,
        out FormulaReference? reference)
    {
        reference = null;
        var separator = key.IndexOf('@', StringComparison.Ordinal);
        if (separator <= 0 ||
            separator != key.LastIndexOf('@') ||
            separator == key.Length - 1)
        {
            return false;
        }

        reference = new FormulaReference(
            key.Substring(0, separator),
            key.Substring(separator + 1));
        return true;
    }

    private static BreakpointResult CompareBreakpoint(
        BreakpointTarget target,
        long? firstValue,
        long? secondValue) =>
        new(
            target,
            firstValue,
            secondValue,
            firstValue is null ? null : firstValue.Value >= target.Target,
            secondValue is null ? null : secondValue.Value >= target.Target,
            firstValue is null ? null : checked(firstValue.Value - target.Target),
            secondValue is null ? null : checked(secondValue.Value - target.Target));

    private List<ComparisonWarning> CollectWarnings(
        CharacterBuild first,
        CharacterBuild second,
        IReadOnlyList<BreakpointResult> breakpointResults)
    {
        var warnings = new List<ComparisonWarning>();
        foreach (var result in breakpointResults)
        {
            AddTargetWarning(warnings, result, ComparisonSide.First);
            AddTargetWarning(warnings, result, ComparisonSide.Second);
        }

        if (_itemCatalog is not null)
        {
            AddRequirementWarnings(warnings, first, ComparisonSide.First);
            AddRequirementWarnings(warnings, second, ComparisonSide.Second);
        }

        return warnings
            .OrderBy(item => item.Side)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Detail, StringComparer.Ordinal)
            .ToList();
    }

    private static void AddTargetWarning(
        List<ComparisonWarning> warnings,
        BreakpointResult result,
        ComparisonSide side)
    {
        var meets = side == ComparisonSide.First ? result.MeetsFirst : result.MeetsSecond;
        if (meets is false)
        {
            var value = side == ComparisonSide.First ? result.FirstValue : result.SecondValue;
            warnings.Add(new ComparisonWarning(
                ComparisonWarningCodes.TargetMissed,
                side,
                $"Breakpoint '{result.Target.Key}' target {result.Target.Target}: " +
                $"value {value}."));
        }
    }

    private void AddRequirementWarnings(
        List<ComparisonWarning> warnings,
        CharacterBuild build,
        ComparisonSide side)
    {
        if (_itemCatalog is null)
        {
            return;
        }

        foreach (var item in _itemCatalog.Items.OrderBy(candidate => candidate.Id, StringComparer.Ordinal))
        {
            if (!item.AllowedClassIds.Contains(build.CharacterClassId))
            {
                continue;
            }

            var unmet = item.RequiredStats
                .Where(requirement =>
                    !build.Stats.TryGetValue(requirement.Key, out var finalValue) ||
                    finalValue < requirement.Value)
                .OrderBy(requirement => requirement.Key, StringComparer.Ordinal)
                .Select(requirement =>
                {
                    build.Stats.TryGetValue(requirement.Key, out var finalValue);
                    return $"'{requirement.Key}' requires {requirement.Value} " +
                        $"(final {finalValue})";
                })
                .ToArray();
            if (unmet.Length > 0)
            {
                warnings.Add(new ComparisonWarning(
                    ComparisonWarningCodes.RequirementUnmet,
                    side,
                    $"Item '{item.Id}' v{item.Version} unmet: " +
                    $"{string.Join("; ", unmet)}."));
            }
        }
    }

    private static StatDifference[] CompareStats(
        IReadOnlyDictionary<string, long> first,
        IReadOnlyDictionary<string, long> second)
    {
        return first.Keys
            .Concat(second.Keys)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .Select(statId =>
            {
                var hasFirst = first.TryGetValue(statId, out var firstValue);
                var hasSecond = second.TryGetValue(statId, out var secondValue);
                return new StatDifference(
                    statId,
                    hasFirst ? firstValue : null,
                    hasSecond ? secondValue : null,
                    hasFirst && hasSecond
                        ? checked(secondValue - firstValue)
                        : null);
            })
            .ToArray();
    }

    private static DerivedDifference[] CompareDerived(
        CharacterBuildEvaluation first,
        CharacterBuildEvaluation second)
    {
        var secondByReference = second.Formulas.ToDictionary(
            item => item.Formula.Reference);
        return first.Formulas
            .Where(item => secondByReference.ContainsKey(item.Formula.Reference))
            .OrderBy(item => item.Formula.Reference.Id, StringComparer.Ordinal)
            .ThenBy(item => item.Formula.Reference.Version, StringComparer.Ordinal)
            .Select(item =>
            {
                var other = secondByReference[item.Formula.Reference];
                var absolute = checked(
                    other.Calculation.VisibleOutput -
                    item.Calculation.VisibleOutput);
                return new DerivedDifference(
                    item.Formula.Reference.Id,
                    item.Formula.Reference.Version,
                    item.Formula.Output.Id,
                    item.Formula.Output.Unit,
                    item.Calculation.VisibleOutput,
                    other.Calculation.VisibleOutput,
                    absolute,
                    item.Calculation.VisibleOutput == 0
                        ? null
                        : (decimal)absolute /
                            Math.Abs((decimal)item.Calculation.VisibleOutput) *
                            100m);
            })
            .ToArray();
    }

    private static DerivedOnlyOnSide[] OnlyOnSide(
        CharacterBuildEvaluation side,
        CharacterBuildEvaluation other)
    {
        var otherReferences = other.Formulas
            .Select(item => item.Formula.Reference)
            .ToHashSet();
        return side.Formulas
            .Where(item => !otherReferences.Contains(item.Formula.Reference))
            .OrderBy(item => item.Formula.Reference.Id, StringComparer.Ordinal)
            .ThenBy(item => item.Formula.Reference.Version, StringComparer.Ordinal)
            .Select(item => new DerivedOnlyOnSide(
                item.Formula.Reference.Id,
                item.Formula.Reference.Version,
                item.Formula.Output.Id,
                item.Formula.Output.Unit,
                item.Calculation.VisibleOutput))
            .ToArray();
    }

    private static BuildComparisonException Error(string code, string message) =>
        new(code, message);
}
