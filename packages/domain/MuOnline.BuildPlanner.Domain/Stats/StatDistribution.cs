using MuOnline.BuildPlanner.Domain.Progression;

namespace MuOnline.BuildPlanner.Domain.Stats;

public sealed record StatDistributionRequest(
    ProgressionPointBudgetResult Budget,
    CharacterProgressionDefinition CharacterClass,
    IReadOnlyDictionary<string, long> Allocations);

public sealed record StatDistributionResult(
    string RulesetId,
    string CharacterClassId,
    string ProgressionRuleId,
    string ProgressionRuleVersion,
    long EarnedPoints,
    IReadOnlyDictionary<string, long> Allocations,
    long SpentPoints,
    long RemainingPoints);
