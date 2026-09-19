using System.Text.Json;
using MuOnline.BuildPlanner.Domain.Skills;

namespace MuOnline.BuildPlanner.Application.Skills;

public sealed class JsonSkillCatalogSnapshotReader : ISkillCatalogSnapshotReader
{
    private const string SupportedSchemaVersion = "1.0.0";

    public SkillCatalog Read(string snapshotRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(snapshotRoot);

        var skillDirectory = Path.Combine(snapshotRoot, "skills");
        EnsureDirectoryExists(skillDirectory);

        try
        {
            var skills = LoadFiles(skillDirectory, ParseSkill);
            EnsureUniqueIds(skills.Select(skill => skill.Id));

            var rulesetIds = skills
                .Select(skill => skill.RulesetId)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            if (rulesetIds.Length != 1)
            {
                throw Error(
                    SkillCatalogSnapshotErrorCodes.RulesetMismatch,
                    "All skill records must belong to exactly one ruleset.");
            }

            foreach (var skill in skills)
            {
                if (skill.Status != SkillDefinitionStatus.Published)
                {
                    throw Error(
                        SkillCatalogSnapshotErrorCodes.SkillNotPublished,
                        $"Skill '{skill.Id}' is not PUBLISHED.");
                }
            }

            return new SkillCatalog(rulesetIds[0], skills);
        }
        catch (SkillCatalogSnapshotException)
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
                SkillCatalogSnapshotErrorCodes.SnapshotInvalid,
                $"Skill snapshot '{snapshotRoot}' could not be read as a bounded catalog.");
        }
    }

    private static SkillDefinition ParseSkill(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var root = document.RootElement;
        EnsureSupportedSchemaVersion(root);

        var requiredLevel = RequiredLong(root, "requiredLevel");
        if (requiredLevel < 1)
        {
            throw Error(
                SkillCatalogSnapshotErrorCodes.SnapshotInvalid,
                $"Skill record must declare a positive 'requiredLevel'.");
        }

        var allowedEvolutionIds = StringArray(root, "allowedEvolutionIds");
        if (allowedEvolutionIds.Length == 0)
        {
            throw Error(
                SkillCatalogSnapshotErrorCodes.SnapshotInvalid,
                "Skill record must declare at least one 'allowedEvolutionIds' entry.");
        }

        var uniqueEvolutionIds = allowedEvolutionIds.ToHashSet(StringComparer.Ordinal);
        if (uniqueEvolutionIds.Count != allowedEvolutionIds.Length)
        {
            throw Error(
                SkillCatalogSnapshotErrorCodes.SnapshotInvalid,
                "Skill record 'allowedEvolutionIds' cannot contain duplicates.");
        }

        return new SkillDefinition(
            Id: RequiredString(root, "id"),
            Version: RequiredString(root, "version"),
            RulesetId: RequiredString(root, "rulesetId"),
            DisplayName: RequiredString(root, "displayName"),
            Status: ParseStatus(RequiredString(root, "status")),
            Kind: ParseKind(RequiredString(root, "kind")),
            RequiredLevel: requiredLevel,
            AllowedEvolutionIds: uniqueEvolutionIds);
    }

    private static void EnsureSupportedSchemaVersion(JsonElement root)
    {
        if (!root.TryGetProperty("schemaVersion", out var property) ||
            property.ValueKind != JsonValueKind.String ||
            !string.Equals(
                property.GetString(),
                SupportedSchemaVersion,
                StringComparison.Ordinal))
        {
            throw Error(
                SkillCatalogSnapshotErrorCodes.SnapshotInvalid,
                $"Skill record must declare supported schemaVersion " +
                $"'{SupportedSchemaVersion}'.");
        }
    }

    private static SkillDefinitionStatus ParseStatus(string value) =>
        value switch
        {
            "DRAFT" => SkillDefinitionStatus.Draft,
            "REVIEWED" => SkillDefinitionStatus.Reviewed,
            "PUBLISHED" => SkillDefinitionStatus.Published,
            "DEPRECATED" => SkillDefinitionStatus.Deprecated,
            _ => throw Error(
                SkillCatalogSnapshotErrorCodes.SnapshotInvalid,
                $"Unsupported skill status '{value}'."),
        };

    private static SkillKind ParseKind(string value) =>
        value switch
        {
            "ACTIVE" => SkillKind.Active,
            "BUFF" => SkillKind.Buff,
            "PASSIVE" => SkillKind.Passive,
            "SUMMON" => SkillKind.Summon,
            _ => throw Error(
                SkillCatalogSnapshotErrorCodes.SnapshotInvalid,
                $"Unsupported skill kind '{value}'."),
        };

    private static long RequiredLong(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.Number)
        {
            throw Error(
                SkillCatalogSnapshotErrorCodes.SnapshotInvalid,
                $"Skill record must declare numeric '{propertyName}'.");
        }

        return property.GetInt64();
    }

    private static string RequiredString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String)
        {
            throw Error(
                SkillCatalogSnapshotErrorCodes.SnapshotInvalid,
                $"Skill record must declare '{propertyName}'.");
        }

        return property.GetString()!;
    }

    private static string[] StringArray(JsonElement element, string propertyName) =>
        element.GetProperty(propertyName)
            .EnumerateArray()
            .Select(item => item.GetString()
                ?? throw Error(
                    SkillCatalogSnapshotErrorCodes.SnapshotInvalid,
                    $"'{propertyName}' cannot contain null values."))
            .ToArray();

    private static void EnsureUniqueIds(IEnumerable<string> ids)
    {
        var duplicates = ids
            .GroupBy(id => id, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (duplicates.Length != 0)
        {
            throw Error(
                SkillCatalogSnapshotErrorCodes.DuplicateId,
                $"Duplicate skill ids: {string.Join(", ", duplicates)}");
        }
    }

    private static T[] LoadFiles<T>(string directory, Func<string, T> parser)
    {
        var paths = Directory.GetFiles(directory, "*.json", SearchOption.TopDirectoryOnly)
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (paths.Length == 0)
        {
            throw Error(
                SkillCatalogSnapshotErrorCodes.SnapshotInvalid,
                $"Skill directory '{directory}' contains no JSON records.");
        }

        return paths.Select(parser).ToArray();
    }

    private static void EnsureDirectoryExists(string directory)
    {
        if (!Directory.Exists(directory))
        {
            throw Error(
                SkillCatalogSnapshotErrorCodes.SnapshotNotFound,
                $"Skill snapshot directory '{directory}' does not exist.");
        }
    }

    private static SkillCatalogSnapshotException Error(string code, string message) =>
        new(code, message);
}
