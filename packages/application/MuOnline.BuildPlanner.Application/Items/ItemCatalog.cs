using MuOnline.BuildPlanner.Domain.Items;

namespace MuOnline.BuildPlanner.Application.Items;

public sealed record ItemCatalog(
    string RulesetId,
    IReadOnlyList<ItemDefinition> Items);
