using MuOnline.BuildPlanner.Application.Progression;
using MuOnline.BuildPlanner.Application.Stats;
using MuOnline.BuildPlanner.Domain.Progression;
using MuOnline.BuildPlanner.Domain.Stats;

namespace MuOnline.BuildPlanner.Application.Builds;

internal static class BuildDraftCalculation
{
    public static BuildDraftStatDistribution Calculate(
        BuildDraftRuntimeContext context,
        BuildDraftProgressionInputs inputs,
        BuildDraftResetInputs resetInputs,
        IReadOnlyDictionary<string, long> allocations)
    {
        EnsureRuntimeContext(context);

        var budget = new CalculateProgressionPointBudgetUseCase(context.Catalog).Execute(
            new ProgressionPointBudgetRequest(
                inputs.CharacterClassId,
                inputs.EvolutionId,
                inputs.Level,
                inputs.CompletedQuestIds));
        var distribution = new CalculateStatDistributionUseCase(context.Catalog).Execute(
            budget,
            new ResetPointInputs(
                resetInputs.ResetCount,
                resetInputs.PointsPerReset),
            allocations);

        return ToSnapshot(distribution);
    }

    public static void EnsureRuntimeContext(BuildDraftRuntimeContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.Catalog);
        ArgumentNullException.ThrowIfNull(context.Ruleset);
        ArgumentNullException.ThrowIfNull(context.Dataset);
        ArgumentException.ThrowIfNullOrWhiteSpace(context.Ruleset.Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(context.Ruleset.Version);
        ArgumentException.ThrowIfNullOrWhiteSpace(context.Dataset.Version);
        ArgumentException.ThrowIfNullOrWhiteSpace(context.Dataset.Hash);
        ArgumentException.ThrowIfNullOrWhiteSpace(context.EngineVersion);

        if (context.Catalog.RulesetId != context.Ruleset.Id)
        {
            throw new BuildDraftException(
                BuildDraftErrorCodes.DependencyUnavailable,
                "The explicit ruleset metadata does not match the loaded catalog.");
        }
    }

    private static BuildDraftStatDistribution ToSnapshot(StatDistributionResult result) =>
        new(
            BuildDraftStatDistribution.CurrentSchemaVersion,
            result.RulesetId,
            result.CharacterClassId,
            new BuildDraftVersionedReference(
                result.ProgressionRuleId,
                result.ProgressionRuleVersion),
            result.EarnedPoints,
            new BuildDraftResetInputs(
                result.ResetInputs.ResetCount,
                result.ResetInputs.PointsPerReset),
            result.ResetPoints,
            result.TotalDistributablePoints,
            new Dictionary<string, long>(result.Allocations, StringComparer.Ordinal),
            result.SpentPoints,
            result.RemainingPoints);
}
