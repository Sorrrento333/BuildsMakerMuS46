namespace MuOnline.BuildPlanner.Domain.Items;

public enum ItemDefinitionStatus
{
    Draft,
    Reviewed,
    Published,
    Deprecated,
}

public sealed record ItemDefinition(
    string Id,
    string Version,
    string RulesetId,
    string DisplayName,
    ItemDefinitionStatus Status,
    IReadOnlySet<string> Slots,
    IReadOnlySet<string> AllowedClassIds,
    IReadOnlyDictionary<string, long> RequiredStats);
