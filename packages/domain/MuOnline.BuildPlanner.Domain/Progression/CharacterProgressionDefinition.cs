namespace MuOnline.BuildPlanner.Domain.Progression;

public sealed record CharacterProgressionDefinition(
    string Id,
    string RulesetId,
    IReadOnlySet<string> StatIds,
    IReadOnlySet<string> EvolutionIds,
    IReadOnlyList<string> ProgressionRuleRefs);
