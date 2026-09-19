namespace MuOnline.BuildPlanner.Domain.Skills;

public sealed record SkillDefinition(
    string Id,
    string Version,
    string RulesetId,
    string DisplayName,
    SkillDefinitionStatus Status,
    SkillKind Kind,
    long RequiredLevel,
    IReadOnlySet<string> AllowedEvolutionIds);
