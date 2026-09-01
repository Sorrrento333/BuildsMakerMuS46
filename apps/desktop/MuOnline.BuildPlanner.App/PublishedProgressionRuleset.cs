using System.IO;
using MuOnline.BuildPlanner.Application.Formulas;
using MuOnline.BuildPlanner.Application.Progression;
using MuOnline.BuildPlanner.Application.Stats;

namespace MuOnline.BuildPlanner.App;

internal static class PublishedProgressionRuleset
{
    private static readonly Lazy<ProgressionRulesetCatalog> CatalogValue =
        new(() => new JsonProgressionRulesetSnapshotReader().Read(SnapshotRoot));
    private static readonly Lazy<ExecutableFormulaCatalog> FormulaCatalogValue =
        new(() => new JsonExecutableFormulaSnapshotReader().Read(SnapshotRoot));

    public static string SnapshotRoot => Path.Combine(
        AppContext.BaseDirectory,
        "rulesets",
        "mu-s4-global-reference",
        "v1");

    public static ProgressionRulesetCatalog Catalog => CatalogValue.Value;

    public static ExecutableFormulaCatalog FormulaCatalog =>
        FormulaCatalogValue.Value;

    public static CalculateProgressionPointBudgetUseCase CreateUseCase() =>
        new(Catalog);

    public static CalculateStatDistributionUseCase CreateStatDistributionUseCase() =>
        new(Catalog);

    public static CalculateCharacterFormulaUseCase CreateCharacterFormulaUseCase() =>
        new(Catalog, FormulaCatalog);
}
