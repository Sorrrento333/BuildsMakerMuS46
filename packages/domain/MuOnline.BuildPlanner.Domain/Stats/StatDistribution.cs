using MuOnline.BuildPlanner.Domain.Progression;

namespace MuOnline.BuildPlanner.Domain.Stats;

public sealed record StatDistributionRequest(
    ProgressionPointBudgetResult Budget,
    CharacterProgressionDefinition CharacterClass,
    ResetPointInputs ResetInputs,
    IReadOnlyDictionary<string, long> Allocations);

public sealed record ResetPointInputs(
    long ResetCount,
    long PointsPerReset);

public sealed record StatDistributionResult(
    string RulesetId,
    string CharacterClassId,
    string ProgressionRuleId,
    string ProgressionRuleVersion,
    long EarnedPoints,
    ResetPointInputs ResetInputs,
    long ResetPoints,
    long TotalDistributablePoints,
    IReadOnlyDictionary<string, long> Allocations,
    long SpentPoints,
    long RemainingPoints);
