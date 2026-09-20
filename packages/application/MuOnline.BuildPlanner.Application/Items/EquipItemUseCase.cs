namespace MuOnline.BuildPlanner.Application.Items;

public sealed record EquipItemRequest(
    string CharacterClassId,
    IReadOnlyDictionary<string, long> FinalStats,
    string ItemId,
    int Level = 0);

public sealed record EquipItemResult(
    string ItemId,
    string DisplayName,
    string RulesetId,
    IReadOnlyList<string> Slots,
    IReadOnlyDictionary<string, long> RequiredStats,
    int Level,
    int MaxItemLevel);

public sealed class EquipItemUseCase
{
    private readonly ItemCatalog _catalog;

    public EquipItemUseCase(ItemCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        _catalog = catalog;
    }

    public EquipItemResult Execute(EquipItemRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.CharacterClassId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ItemId);
        ArgumentNullException.ThrowIfNull(request.FinalStats);

        var matches = _catalog.Items
            .Where(item => item.Id == request.ItemId)
            .ToArray();
        if (matches.Length != 1)
        {
            throw Error(
                ItemEquipErrorCodes.ItemNotFound,
                $"Item '{request.ItemId}' does not resolve to exactly one published definition.");
        }

        var item = matches[0];
        if (!item.AllowedClassIds.Contains(request.CharacterClassId))
        {
            throw Error(
                ItemEquipErrorCodes.ClassNotAllowed,
                $"Class '{request.CharacterClassId}' cannot equip item '{item.Id}'.");
        }

        if (request.Level < 0 || request.Level > item.MaxItemLevel)
        {
            throw Error(
                ItemEquipErrorCodes.LevelOutOfRange,
                $"Item '{item.Id}' cannot be equipped at level '{request.Level}' " +
                $"when its published maximum is '{item.MaxItemLevel}'.");
        }

        var unmetStats = item.RequiredStats
            .Where(requirement =>
                !request.FinalStats.TryGetValue(requirement.Key, out var finalValue) ||
                finalValue < requirement.Value)
            .Select(requirement => requirement.Key)
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (unmetStats.Length != 0)
        {
            throw Error(
                ItemEquipErrorCodes.RequirementsNotMet,
                $"Item '{item.Id}' requires {string.Join(", ", unmetStats)} at or above the published level +0 requirement.");
        }

        return new EquipItemResult(
            item.Id,
            item.DisplayName,
            item.RulesetId,
            item.Slots.Order(StringComparer.Ordinal).ToArray(),
            item.RequiredStats,
            request.Level,
            item.MaxItemLevel);
    }

    private static ItemEquipException Error(string code, string message) =>
        new(code, message);
}
