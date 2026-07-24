using System.IO;
using MuOnline.BuildPlanner.Application.Progression;

namespace MuOnline.BuildPlanner.App;

internal static class PublishedProgressionRuleset
{
    private static readonly Lazy<ProgressionRulesetCatalog> CatalogValue =
        new(() => new JsonProgressionRulesetSnapshotReader().Read(SnapshotRoot));

    public static string SnapshotRoot => Path.Combine(
        AppContext.BaseDirectory,
        "rulesets",
        "mu-s4-global-reference",
        "v1");

    public static ProgressionRulesetCatalog Catalog => CatalogValue.Value;

    public static CalculateProgressionPointBudgetUseCase CreateUseCase() =>
        new(Catalog);
}
