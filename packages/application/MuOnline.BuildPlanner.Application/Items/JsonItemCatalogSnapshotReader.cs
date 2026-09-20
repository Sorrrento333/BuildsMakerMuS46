using System.Text.Json;
using MuOnline.BuildPlanner.Domain.Items;

namespace MuOnline.BuildPlanner.Application.Items;

public sealed class JsonItemCatalogSnapshotReader : IItemCatalogSnapshotReader
{
    private const string SupportedSchemaVersion = "1.0.0";

    public ItemCatalog Read(string snapshotRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(snapshotRoot);

        var itemDirectory = Path.Combine(snapshotRoot, "items");
        EnsureDirectoryExists(itemDirectory);

        try
        {
            var items = LoadFiles(itemDirectory, ParseItem);
            EnsureUniqueIds(items.Select(item => item.Id));

            var rulesetIds = items
                .Select(item => item.RulesetId)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            if (rulesetIds.Length != 1)
            {
                throw Error(
                    ItemCatalogSnapshotErrorCodes.RulesetMismatch,
                    "All item records must belong to exactly one ruleset.");
            }

            foreach (var item in items)
            {
                if (item.Status != ItemDefinitionStatus.Published)
                {
                    throw Error(
                        ItemCatalogSnapshotErrorCodes.ItemNotPublished,
                        $"Item '{item.Id}' is not PUBLISHED.");
                }
            }

            return new ItemCatalog(rulesetIds[0], items);
        }
        catch (ItemCatalogSnapshotException)
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
                ItemCatalogSnapshotErrorCodes.SnapshotInvalid,
                $"Item snapshot '{snapshotRoot}' could not be materialized.",
                exception);
        }
    }

    private static ItemDefinition ParseItem(JsonElement element)
    {
        var schemaVersion = RequiredString(element, "schemaVersion");
        if (schemaVersion != SupportedSchemaVersion)
        {
            throw Error(
                ItemCatalogSnapshotErrorCodes.SnapshotInvalid,
                $"Item schema version '{schemaVersion}' is not supported.");
        }

        var slots = StringArray(element, "slots").ToHashSet(StringComparer.Ordinal);
        var allowedClassIds = StringArray(element, "allowedClassIds")
            .ToHashSet(StringComparer.Ordinal);
        if (slots.Count == 0 || allowedClassIds.Count == 0)
        {
            throw Error(
                ItemCatalogSnapshotErrorCodes.SnapshotInvalid,
                "Item slots and allowed classes cannot be empty.");
        }

        return new ItemDefinition(
            RequiredString(element, "id"),
            RequiredString(element, "version"),
            RequiredString(element, "rulesetId"),
            RequiredString(element, "displayName"),
            ParseStatus(RequiredString(element, "status")),
            slots,
            allowedClassIds,
            ParseRequiredStats(element),
            ParseMaxItemLevel(element));
    }

    private static Dictionary<string, long> ParseRequiredStats(JsonElement element)
    {
        var requiredStats = new Dictionary<string, long>(StringComparer.Ordinal);
        if (!element.TryGetProperty("requiredStats", out var statsElement))
        {
            return requiredStats;
        }

        foreach (var stat in statsElement.EnumerateObject())
        {
            var value = stat.Value.GetInt64();
            if (value < 0)
            {
                throw Error(
                    ItemCatalogSnapshotErrorCodes.SnapshotInvalid,
                    $"Item required stat '{stat.Name}' cannot be negative.");
            }

            requiredStats[stat.Name] = value;
        }

        return requiredStats;
    }

    private static int ParseMaxItemLevel(JsonElement element)
    {
        if (!element.TryGetProperty("maxItemLevel", out var levelElement))
        {
            throw Error(
                ItemCatalogSnapshotErrorCodes.SnapshotInvalid,
                "Item maximum level is missing.");
        }

        var level = levelElement.GetInt32();
        if (level < 0)
        {
            throw Error(
                ItemCatalogSnapshotErrorCodes.SnapshotInvalid,
                "Item maximum level cannot be negative.");
        }

        return level;
    }

    private static ItemDefinitionStatus ParseStatus(string status) =>
        status switch
        {
            "DRAFT" => ItemDefinitionStatus.Draft,
            "REVIEWED" => ItemDefinitionStatus.Reviewed,
            "PUBLISHED" => ItemDefinitionStatus.Published,
            "DEPRECATED" => ItemDefinitionStatus.Deprecated,
            _ => throw Error(
                ItemCatalogSnapshotErrorCodes.SnapshotInvalid,
                $"Unknown item status '{status}'."),
        };

    private static ItemDefinition[] LoadFiles(
        string directory,
        Func<JsonElement, ItemDefinition> parse)
    {
        var paths = Directory.GetFiles(directory, "*.json")
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (paths.Length == 0)
        {
            throw Error(
                ItemCatalogSnapshotErrorCodes.SnapshotInvalid,
                $"Item directory '{directory}' contains no JSON records.");
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
                ItemCatalogSnapshotErrorCodes.SnapshotNotFound,
                $"Item directory '{directory}' was not found.");
        }
    }

    private static void EnsureUniqueIds(IEnumerable<string> ids)
    {
        var duplicateId = ids
            .GroupBy(id => id, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1)
            ?.Key;
        if (duplicateId is not null)
        {
            throw Error(
                ItemCatalogSnapshotErrorCodes.DuplicateId,
                $"Duplicate item ID '{duplicateId}' was found.");
        }
    }

    private static string RequiredString(JsonElement element, string propertyName) =>
        element.GetProperty(propertyName).GetString()
        ?? throw Error(
            ItemCatalogSnapshotErrorCodes.SnapshotInvalid,
            $"'{propertyName}' cannot be null.");

    private static string[] StringArray(JsonElement element, string propertyName) =>
        element.GetProperty(propertyName)
            .EnumerateArray()
            .Select(item => item.GetString()
                ?? throw Error(
                    ItemCatalogSnapshotErrorCodes.SnapshotInvalid,
                    $"'{propertyName}' cannot contain null values."))
            .ToArray();

    private static ItemCatalogSnapshotException Error(string code, string message) =>
        new(code, message);

    private static ItemCatalogSnapshotException Error(
        string code,
        string message,
        Exception innerException) =>
        new(code, message, innerException);
}
