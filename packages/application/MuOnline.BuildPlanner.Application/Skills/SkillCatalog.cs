using MuOnline.BuildPlanner.Domain.Skills;

namespace MuOnline.BuildPlanner.Application.Skills;

public sealed record SkillCatalog(
    string RulesetId,
    IReadOnlyList<SkillDefinition> Skills);
