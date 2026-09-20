using MuOnline.BuildPlanner.Application.Items;

namespace MuOnline.BuildPlanner.Application.Builds;

internal static class BuildEquipmentValidator
{
    public static void EnsureValid(
        ItemCatalog catalog,
        string characterClassId,
        IReadOnlyDictionary<string, long> finalStats,
        IReadOnlyList<BuildEquipmentEntry> equipment)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentException.ThrowIfNullOrWhiteSpace(characterClassId);
        ArgumentNullException.ThrowIfNull(finalStats);
        ArgumentNullException.ThrowIfNull(equipment);

        var seenItemIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var entry in equipment)
        {
            ArgumentNullException.ThrowIfNull(entry);

            if (!seenItemIds.Add(entry.ItemId))
            {
                throw Error(
                    EquipmentCode.Duplicate,
                    $"Equipment references item '{entry.ItemId}' more than once.");
            }

            var matches = catalog.Items
                .Where(item => item.Id == entry.ItemId)
                .ToArray();
            if (matches.Length != 1)
            {
                throw Error(
                    EquipmentCode.ItemNotFound,
                    $"Equipment references the unknown item '{entry.ItemId}'.");
            }

            var item = matches[0];
            if (item.Version != entry.ItemVersion)
            {
                throw Error(
                    EquipmentCode.VersionMismatch,
                    $"Equipment references item '{entry.ItemId}' at version " +
                    $"'{entry.ItemVersion}' but the published definition is '{item.Version}'.");
            }

            if (!item.AllowedClassIds.Contains(characterClassId))
            {
                throw Error(
                    EquipmentCode.ClassNotAllowed,
                    $"Class '{characterClassId}' cannot equip item '{entry.ItemId}'.");
            }

            if (entry.Level < 0 || entry.Level > item.MaxItemLevel)
            {
                throw Error(
                    EquipmentCode.LevelOutOfRange,
                    $"Item '{entry.ItemId}' is equipped at level '{entry.Level}' " +
                    $"but its published maximum is '{item.MaxItemLevel}'.");
            }

            var unmetStats = item.RequiredStats
                .Where(requirement =>
                    !finalStats.TryGetValue(requirement.Key, out var finalValue) ||
                    finalValue < requirement.Value)
                .Select(requirement => requirement.Key)
                .Order(StringComparer.Ordinal)
                .ToArray();
            if (unmetStats.Length != 0)
            {
                throw Error(
                    EquipmentCode.RequirementsNotMet,
                    $"Item '{entry.ItemId}' requires {string.Join(", ", unmetStats)} " +
                    "at or above the published level +0 requirement.");
            }
        }
    }

    private static BuildEquipmentValidationException Error(
        string code,
        string message) => new(code, message);

    private static class EquipmentCode
    {
        public const string ItemNotFound = "item-not-found";
        public const string VersionMismatch = "version-mismatch";
        public const string ClassNotAllowed = "class-not-allowed";
        public const string LevelOutOfRange = "level-out-of-range";
        public const string Duplicate = "duplicate";
        public const string RequirementsNotMet = "requirements-not-met";
    }
}

internal sealed class BuildEquipmentValidationException : Exception
{
    public BuildEquipmentValidationException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}