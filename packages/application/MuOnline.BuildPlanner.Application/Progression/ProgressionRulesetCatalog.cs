using MuOnline.BuildPlanner.Domain.Progression;

namespace MuOnline.BuildPlanner.Application.Progression;

public sealed record ProgressionRulesetCatalog(
    string RulesetId,
    IReadOnlyList<CharacterProgressionDefinition> Classes,
    IReadOnlyList<ProgressionRuleDefinition> Rules,
    IReadOnlyList<ProgressionCharacterOption> CharacterOptions);

public sealed record ProgressionCharacterOption(
    string Id,
    string DisplayName,
    IReadOnlyList<ProgressionEvolutionOption> Evolutions);

public sealed record ProgressionEvolutionOption(
    string Id,
    string DisplayName,
    int Stage);
