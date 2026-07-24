using System.Text.Json;
using MuOnline.BuildPlanner.Domain.Progression;

namespace MuOnline.BuildPlanner.Application.Progression;

public sealed class JsonProgressionRulesetSnapshotReader : IProgressionRulesetSnapshotReader
{
    public ProgressionRulesetCatalog Read(string snapshotRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(snapshotRoot);

        var characterClassDirectory = Path.Combine(snapshotRoot, "character-classes");
        var progressionRuleDirectory = Path.Combine(snapshotRoot, "progression-rules");
        EnsureDirectoryExists(characterClassDirectory);
        EnsureDirectoryExists(progressionRuleDirectory);

        try
        {
            var parsedClasses = LoadFiles(characterClassDirectory, ParseCharacterClass);
            var classes = parsedClasses
                .Select(item => item.Definition)
                .ToArray();
            var rules = LoadFiles(progressionRuleDirectory, ParseProgressionRule);

            EnsureUniqueIds(classes.Select(item => item.Id), "character class");
            EnsureUniqueIds(rules.Select(item => item.Id), "progression rule");

            return ValidateCatalog(
                classes,
                rules,
                parsedClasses.Select(item => item.Option).ToArray());
        }
        catch (ProgressionSnapshotException)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is JsonException or
            InvalidOperationException or
            KeyNotFoundException or
            FormatException or
            OverflowException or
            IOException or
            UnauthorizedAccessException)
        {
            throw Error(
                ProgressionSnapshotErrorCodes.SnapshotInvalid,
                $"Progression snapshot '{snapshotRoot}' could not be materialized.",
                exception);
        }
    }

    private static ProgressionRulesetCatalog ValidateCatalog(
        CharacterProgressionDefinition[] classes,
        ProgressionRuleDefinition[] rules,
        ProgressionCharacterOption[] characterOptions)
    {
        if (classes.Length == 0 || rules.Length == 0)
        {
            throw Error(
                ProgressionSnapshotErrorCodes.SnapshotInvalid,
                "The progression snapshot must contain classes and progression rules.");
        }

        var rulesetIds = classes
            .Select(item => item.RulesetId)
            .Concat(rules.Select(item => item.RulesetId))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (rulesetIds.Length != 1)
        {
            throw Error(
                ProgressionSnapshotErrorCodes.RulesetMismatch,
                "All progression records must belong to exactly one ruleset.");
        }

        var classById = classes.ToDictionary(item => item.Id, StringComparer.Ordinal);
        var ruleById = rules.ToDictionary(item => item.Id, StringComparer.Ordinal);

        foreach (var rule in rules)
        {
            if (rule.Status != ProgressionRuleStatus.Published)
            {
                throw Error(
                    ProgressionSnapshotErrorCodes.RuleNotPublished,
                    $"Progression rule '{rule.Id}' is not PUBLISHED.");
            }

            foreach (var classId in rule.AppliesToClassIds)
            {
                if (!classById.TryGetValue(classId, out var characterClass) ||
                    !characterClass.ProgressionRuleRefs.Contains(rule.Id, StringComparer.Ordinal))
                {
                    throw Incoherent(
                        $"Rule '{rule.Id}' references class '{classId}' without a matching class reference.");
                }
            }
        }

        foreach (var characterClass in classes)
        {
            if (characterClass.ProgressionRuleRefs.Count == 0)
            {
                throw Incoherent(
                    $"Character class '{characterClass.Id}' has no progression rule reference.");
            }

            foreach (var ruleId in characterClass.ProgressionRuleRefs)
            {
                if (!ruleById.TryGetValue(ruleId, out var rule) ||
                    !rule.AppliesToClassIds.Contains(characterClass.Id) ||
                    rule.RulesetId != characterClass.RulesetId)
                {
                    throw Incoherent(
                        $"Character class '{characterClass.Id}' has an incoherent reference to rule '{ruleId}'.");
                }
            }
        }

        return new ProgressionRulesetCatalog(
            rulesetIds[0],
            classes,
            rules,
            characterOptions);
    }

    private static ParsedCharacterClass ParseCharacterClass(JsonElement element)
    {
        var evolutions = element.GetProperty("evolutions")
            .EnumerateArray()
            .Select(evolution => new ProgressionEvolutionOption(
                RequiredString(evolution, "id"),
                RequiredString(evolution, "displayName"),
                evolution.GetProperty("stage").GetInt32()))
            .OrderBy(evolution => evolution.Stage)
            .ThenBy(evolution => evolution.Id, StringComparer.Ordinal)
            .ToArray();
        var id = RequiredString(element, "id");

        return new ParsedCharacterClass(
            new CharacterProgressionDefinition(
                id,
                RequiredString(element, "rulesetId"),
                evolutions
                    .Select(evolution => evolution.Id)
                    .ToHashSet(StringComparer.Ordinal),
                StringArray(element, "progressionRuleRefs")),
            new ProgressionCharacterOption(
                id,
                RequiredString(element, "displayName"),
                evolutions));
    }

    private static ProgressionRuleDefinition ParseProgressionRule(JsonElement element)
    {
        var levelPoints = element.GetProperty("levelPoints");
        var questBonus = element.TryGetProperty("questBonus", out var questBonusElement)
            ? new QuestPointBonusRule(
                RequiredString(questBonusElement, "questId"),
                questBonusElement.GetProperty("minimumLevel").GetInt32(),
                StringArray(questBonusElement, "eligibleEvolutionIds")
                    .ToHashSet(StringComparer.Ordinal),
                questBonusElement.GetProperty("additionalPointsPerLevel").GetInt32(),
                questBonusElement.GetProperty("retroactiveFromLevel").GetInt32())
            : null;

        return new ProgressionRuleDefinition(
            RequiredString(element, "id"),
            RequiredString(element, "version"),
            RequiredString(element, "rulesetId"),
            ParseStatus(RequiredString(element, "status")),
            StringArray(element, "appliesToClassIds").ToHashSet(StringComparer.Ordinal),
            new LevelPointRule(
                levelPoints.GetProperty("pointsPerLevel").GetInt32(),
                levelPoints.GetProperty("firstAwardedLevel").GetInt32()),
            questBonus);
    }

    private static ProgressionRuleStatus ParseStatus(string status) =>
        status switch
        {
            "DRAFT" => ProgressionRuleStatus.Draft,
            "REVIEWED" => ProgressionRuleStatus.Reviewed,
            "PUBLISHED" => ProgressionRuleStatus.Published,
            "DEPRECATED" => ProgressionRuleStatus.Deprecated,
            _ => throw Error(
                ProgressionSnapshotErrorCodes.SnapshotInvalid,
                $"Unknown progression rule status '{status}'."),
        };

    private static T[] LoadFiles<T>(string directory, Func<JsonElement, T> parse)
    {
        var paths = Directory.GetFiles(directory, "*.json")
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (paths.Length == 0)
        {
            throw Error(
                ProgressionSnapshotErrorCodes.SnapshotInvalid,
                $"Snapshot directory '{directory}' contains no JSON records.");
        }

        return paths
            .Select(path =>
            {
                using var document = JsonDocument.Parse(File.ReadAllText(path));
                return parse(document.RootElement);
            })
            .ToArray();
    }

    private static void EnsureDirectoryExists(string directory)
    {
        if (!Directory.Exists(directory))
        {
            throw Error(
                ProgressionSnapshotErrorCodes.SnapshotNotFound,
                $"Snapshot directory '{directory}' was not found.");
        }
    }

    private static void EnsureUniqueIds(IEnumerable<string> ids, string recordType)
    {
        var duplicateId = ids
            .GroupBy(id => id, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1)
            ?.Key;
        if (duplicateId is not null)
        {
            throw Error(
                ProgressionSnapshotErrorCodes.DuplicateId,
                $"Duplicate {recordType} ID '{duplicateId}' was found.");
        }
    }

    private static string RequiredString(JsonElement element, string propertyName) =>
        element.GetProperty(propertyName).GetString()
        ?? throw Error(
            ProgressionSnapshotErrorCodes.SnapshotInvalid,
            $"'{propertyName}' cannot be null.");

    private static string[] StringArray(JsonElement element, string propertyName) =>
        element.GetProperty(propertyName)
            .EnumerateArray()
            .Select(item => item.GetString()
                ?? throw Error(
                    ProgressionSnapshotErrorCodes.SnapshotInvalid,
                    $"'{propertyName}' cannot contain null values."))
            .ToArray();

    private static ProgressionSnapshotException Incoherent(string message) =>
        Error(ProgressionSnapshotErrorCodes.ReferenceIncoherent, message);

    private static ProgressionSnapshotException Error(string code, string message) =>
        new(code, message);

    private static ProgressionSnapshotException Error(
        string code,
        string message,
        Exception innerException) =>
        new(code, message, innerException);

    private sealed record ParsedCharacterClass(
        CharacterProgressionDefinition Definition,
        ProgressionCharacterOption Option);
}
