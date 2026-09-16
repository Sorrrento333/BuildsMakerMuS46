using MuOnline.BuildPlanner.Application.Progression;
using MuOnline.BuildPlanner.Domain.Progression;

namespace MuOnline.BuildPlanner.Application.Builds;

internal static class BuildValidation
{
    public static void EnsureValidId(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        if (id[0] == '-' ||
            id[^1] == '-')
        {
            throw Error(
                BuildErrorCodes.SourceMismatch,
                "The build id must not start or end with a hyphen.");
        }

        var previousWasHyphen = false;
        foreach (var character in id)
        {
            if (character == '-')
            {
                if (previousWasHyphen)
                {
                    throw Error(
                        BuildErrorCodes.SourceMismatch,
                        "The build id must not contain consecutive hyphens.");
                }

                previousWasHyphen = true;
                continue;
            }

            if (!char.IsAsciiLetterLower(character) && !char.IsAsciiDigit(character))
            {
                throw Error(
                    BuildErrorCodes.SourceMismatch,
                    "The build id must use lowercase letters, digits and single hyphens.");
            }

            previousWasHyphen = false;
        }
    }

    public static void EnsureRuntimeContext(BuildDraftRuntimeContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.Catalog);
        ArgumentNullException.ThrowIfNull(context.Ruleset);
        ArgumentNullException.ThrowIfNull(context.Dataset);
        ArgumentException.ThrowIfNullOrWhiteSpace(context.Ruleset.Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(context.Ruleset.Version);
        ArgumentException.ThrowIfNullOrWhiteSpace(context.Dataset.Version);
        ArgumentException.ThrowIfNullOrWhiteSpace(context.Dataset.Hash);
        ArgumentException.ThrowIfNullOrWhiteSpace(context.EngineVersion);

        if (context.Catalog.RulesetId != context.Ruleset.Id)
        {
            throw Error(
                BuildErrorCodes.DependencyUnavailable,
                "The explicit ruleset metadata does not match the loaded catalog.");
        }
    }

    public static CharacterProgressionDefinition ResolveCharacterClass(
        ProgressionRulesetCatalog catalog,
        string buildId,
        string characterClassId,
        string evolutionId)
    {
        var characterClass = catalog.Classes.SingleOrDefault(
            item => item.Id == characterClassId);
        if (characterClass is null)
        {
            throw Error(
                BuildErrorCodes.SourceMismatch,
                $"Build '{buildId}' references the unknown character class '{characterClassId}'.");
        }

        if (characterClass.RulesetId != catalog.RulesetId)
        {
            throw Error(
                BuildErrorCodes.SourceMismatch,
                $"Build '{buildId}' references a character class from another ruleset.");
        }

        if (!characterClass.EvolutionIds.Contains(evolutionId))
        {
            throw Error(
                BuildErrorCodes.SourceMismatch,
                $"Build '{buildId}' references an evolution the class '{characterClassId}' does not offer.");
        }

        return characterClass;
    }

    public static IReadOnlyDictionary<string, long> ComputeFinalStats(
        CharacterProgressionDefinition characterClass,
        IReadOnlyDictionary<string, long> allocations,
        string buildId)
    {
        var stats = new Dictionary<string, long>(StringComparer.Ordinal);
        foreach (var statId in characterClass.StatIds.Order(StringComparer.Ordinal))
        {
            if (!characterClass.BaseStats.TryGetValue(statId, out var baseStat))
            {
                throw Error(
                    BuildErrorCodes.RevalidationFailed,
                    $"Build '{buildId}' cannot resolve the canonical base value for stat '{statId}'.");
            }

            if (!allocations.TryGetValue(statId, out var allocation))
            {
                throw Error(
                    BuildErrorCodes.SourceMismatch,
                    $"Build '{buildId}' has no allocation for stat '{statId}'.");
            }

            long finalValue;
            try
            {
                finalValue = checked(baseStat.BaseValue + allocation);
            }
            catch (OverflowException)
            {
                throw Error(
                    BuildErrorCodes.RevalidationFailed,
                    $"Base value plus allocation for stat '{statId}' exceeds the signed 64-bit range.");
            }

            stats.Add(statId, finalValue);
        }

        return stats;
    }

    public static void EnsureStatsMatchCharacterClass(
        CharacterProgressionDefinition characterClass,
        IReadOnlyDictionary<string, long> stats)
    {
        var expected = characterClass.StatIds.ToHashSet(StringComparer.Ordinal);
        var actual = stats.Keys.ToHashSet(StringComparer.Ordinal);
        if (!expected.SetEquals(actual))
        {
            throw Error(
                BuildErrorCodes.SourceMismatch,
                "The persisted build stats do not match the character class stat set.");
        }
    }

    public static void EnsureFinalStatsAreReachable(
        CharacterProgressionDefinition characterClass,
        IReadOnlyDictionary<string, long> stats,
        string buildId)
    {
        foreach (var (statId, finalValue) in stats)
        {
            if (!characterClass.BaseStats.TryGetValue(statId, out var baseStat))
            {
                throw Error(
                    BuildErrorCodes.RevalidationFailed,
                    $"Build '{buildId}' has no canonical base value for stat '{statId}'.");
            }

            if (finalValue < baseStat.BaseValue)
            {
                throw Error(
                    BuildErrorCodes.RevalidationFailed,
                    $"Build '{buildId}' stat '{statId}' is below its canonical base value.");
            }
        }
    }

    private static BuildException Error(string code, string message) =>
        new(code, message);
}