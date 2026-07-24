namespace MuOnline.BuildPlanner.Domain.Progression;

public sealed record ProgressionPointBudgetRequest(
    string ClassId,
    string EvolutionId,
    int Level,
    IReadOnlyCollection<string> CompletedQuestIds);

public enum ProgressionPointContributionKind
{
    Level,
    QuestBonus,
}

public sealed record ProgressionPointContribution(
    ProgressionPointContributionKind Kind,
    string SourceId,
    int AwardedLevelCount,
    int PointsPerLevel,
    long EarnedPoints);

public sealed record ProgressionPointBudgetResult(
    string RulesetId,
    string ProgressionRuleId,
    string ProgressionRuleVersion,
    long EarnedPoints,
    IReadOnlyList<ProgressionPointContribution> Contributions);
