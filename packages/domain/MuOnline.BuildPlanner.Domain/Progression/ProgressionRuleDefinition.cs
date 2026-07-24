namespace MuOnline.BuildPlanner.Domain.Progression;

public enum ProgressionRuleStatus
{
    Draft,
    Reviewed,
    Published,
    Deprecated,
}

public sealed record LevelPointRule(
    int PointsPerLevel,
    int FirstAwardedLevel);

public sealed record QuestPointBonusRule(
    string QuestId,
    int MinimumLevel,
    IReadOnlySet<string> EligibleEvolutionIds,
    int AdditionalPointsPerLevel,
    int RetroactiveFromLevel);

public sealed record ProgressionRuleDefinition(
    string Id,
    string Version,
    string RulesetId,
    ProgressionRuleStatus Status,
    IReadOnlySet<string> AppliesToClassIds,
    LevelPointRule LevelPoints,
    QuestPointBonusRule? QuestBonus);
