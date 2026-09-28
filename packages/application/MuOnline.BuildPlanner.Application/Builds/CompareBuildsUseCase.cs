using MuOnline.BuildPlanner.Application.Formulas;
using MuOnline.BuildPlanner.Application.Progression;
using MuOnline.BuildPlanner.Domain.Progression;
using MuOnline.BuildPlanner.Domain.Stats;

namespace MuOnline.BuildPlanner.Application.Builds;

public static class BuildComparisonErrorCodes
{
    public const string SameBuild = "compare-same-build";
    public const string UnknownClass = "compare-unknown-class";
    public const string StatsMismatch = "compare-stats-mismatch";
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
    IReadOnlyList<DerivedOnlyOnSide> OnlyInSecond);

public sealed class CompareBuildsUseCase
{
    private readonly ProgressionRulesetCatalog _progressionCatalog;
    private readonly CalculateCharacterBuildUseCase _buildUseCase;

    public CompareBuildsUseCase(
        ProgressionRulesetCatalog progressionCatalog,
        ExecutableFormulaCatalog formulaCatalog)
    {
        ArgumentNullException.ThrowIfNull(progressionCatalog);
        ArgumentNullException.ThrowIfNull(formulaCatalog);
        _progressionCatalog = progressionCatalog;
        _buildUseCase = new CalculateCharacterBuildUseCase(
            progressionCatalog,
            formulaCatalog);
    }

    public BuildComparison Execute(CharacterBuild first, CharacterBuild second)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);

        if (string.Equals(first.Id, second.Id, StringComparison.Ordinal))
        {
            throw Error(
                BuildComparisonErrorCodes.SameBuild,
                $"Cannot compare build '{first.Id}' with itself.");
        }

        var firstEvaluation = Evaluate(first);
        var secondEvaluation = Evaluate(second);

        return new BuildComparison(
            first.Id,
            second.Id,
            CompareStats(first.Stats, second.Stats),
            CompareDerived(firstEvaluation, secondEvaluation),
            OnlyOnSide(firstEvaluation, secondEvaluation),
            OnlyOnSide(secondEvaluation, firstEvaluation));
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
