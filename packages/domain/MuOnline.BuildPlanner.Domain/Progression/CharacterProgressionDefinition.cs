using System.Collections.Immutable;

namespace MuOnline.BuildPlanner.Domain.Progression;

public sealed class CharacterBaseStatDefinition
{
    public CharacterBaseStatDefinition(
        string statId,
        long baseValue,
        IEnumerable<string> evidenceRefs)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(statId);
        ArgumentNullException.ThrowIfNull(evidenceRefs);

        StatId = statId;
        BaseValue = baseValue;
        EvidenceRefs = evidenceRefs
            .Select(RequireId)
            .Distinct(StringComparer.Ordinal)
            .ToImmutableArray();
    }

    public string StatId { get; }

    public long BaseValue { get; }

    public ImmutableArray<string> EvidenceRefs { get; }

    private static string RequireId(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return id;
    }
}

public sealed class CharacterProgressionDefinition
{
    public CharacterProgressionDefinition(
        string id,
        string rulesetId,
        IEnumerable<string> statIds,
        IEnumerable<string> evolutionIds,
        IEnumerable<string> progressionRuleRefs,
        IEnumerable<CharacterBaseStatDefinition>? baseStats = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(rulesetId);
        ArgumentNullException.ThrowIfNull(statIds);
        ArgumentNullException.ThrowIfNull(evolutionIds);
        ArgumentNullException.ThrowIfNull(progressionRuleRefs);

        Id = id;
        RulesetId = rulesetId;
        StatIds = statIds
            .Select(RequireId)
            .ToImmutableHashSet(StringComparer.Ordinal);
        EvolutionIds = evolutionIds
            .Select(RequireId)
            .ToImmutableHashSet(StringComparer.Ordinal);
        ProgressionRuleRefs = progressionRuleRefs
            .Select(RequireId)
            .ToImmutableArray();
        BaseStats = (baseStats ?? [])
            .ToImmutableDictionary(
                stat => stat.StatId,
                stat => stat,
                StringComparer.Ordinal);
    }

    public string Id { get; }

    public string RulesetId { get; }

    public ImmutableHashSet<string> StatIds { get; }

    public ImmutableHashSet<string> EvolutionIds { get; }

    public ImmutableArray<string> ProgressionRuleRefs { get; }

    public ImmutableDictionary<string, CharacterBaseStatDefinition> BaseStats { get; }

    private static string RequireId(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return id;
    }
}
