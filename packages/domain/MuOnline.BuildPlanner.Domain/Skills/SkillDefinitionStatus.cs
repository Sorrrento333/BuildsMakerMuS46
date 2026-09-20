namespace MuOnline.BuildPlanner.Domain.Skills;

public enum SkillDefinitionStatus
{
    Draft,
    Reviewed,
    Published,
    Deprecated,
}

public enum SkillKind
{
    Active,
    Passive,
    Buff,
    Summon,
}
