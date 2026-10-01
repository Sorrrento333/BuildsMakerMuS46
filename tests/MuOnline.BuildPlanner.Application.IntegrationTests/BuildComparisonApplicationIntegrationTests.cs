using MuOnline.BuildPlanner.Application.Builds;
using MuOnline.BuildPlanner.Application.Formulas;
using MuOnline.BuildPlanner.Application.Items;
using MuOnline.BuildPlanner.Application.Progression;
using Xunit;

namespace MuOnline.BuildPlanner.Application.IntegrationTests;

public sealed class BuildComparisonApplicationIntegrationTests
{
    private static readonly string CanonicalSnapshotRoot = FindCanonicalSnapshotRoot();
    private static readonly string[] ExpectedSharedStatIds = ["agility", "energy", "strength", "vitality"];

    private static ProgressionRulesetCatalog CatalogForTest() =>
        new JsonProgressionRulesetSnapshotReader().Read(CanonicalSnapshotRoot);

    private static CompareBuildsUseCase CreateUseCase()
    {
        var progressionCatalog = CatalogForTest();
        var formulaCatalog =
            new JsonExecutableFormulaSnapshotReader().Read(CanonicalSnapshotRoot);
        return new CompareBuildsUseCase(progressionCatalog, formulaCatalog);
    }

    [Fact]
    public void ComparesFinalStatsWithAbsoluteDifferences()
    {
        var useCase = CreateUseCase();
        var first = CreateDarkKnightBuild("build-first", strengthBonus: 0, vitalityBonus: 0, energyBonus: 0);
        var second = CreateDarkKnightBuild("build-second", strengthBonus: 2, vitalityBonus: 1, energyBonus: 20);

        var comparison = useCase.Execute(first, second);

        Assert.Equal("build-first", comparison.FirstBuildId);
        Assert.Equal("build-second", comparison.SecondBuildId);
        Assert.Equal(
            new Dictionary<string, long?>(StringComparer.Ordinal)
            {
                ["agility"] = 0,
                ["energy"] = 20,
                ["strength"] = 2,
                ["vitality"] = 1,
            },
            comparison.StatDifferences.ToDictionary(
                item => item.StatId,
                item => item.AbsoluteDifference,
                StringComparer.Ordinal));
    }

    [Fact]
    public void ComparesSharedDerivedOutputsWithExactPercent()
    {
        var useCase = CreateUseCase();
        var first = CreateDarkKnightBuild("build-first", strengthBonus: 0, vitalityBonus: 0, energyBonus: 0);
        var second = CreateDarkKnightBuild("build-second", strengthBonus: 2, vitalityBonus: 1, energyBonus: 20);

        var comparison = useCase.Execute(first, second);

        var hitPoints = Assert.Single(
            comparison.DerivedDifferences,
            item => item.FormulaId == "formula-hp-dark-knight");
        Assert.Equal("1.0.0", hitPoints.FormulaVersion);
        Assert.Equal("hp", hitPoints.OutputId);
        Assert.Equal(120, hitPoints.FirstVisible);
        Assert.Equal(123, hitPoints.SecondVisible);
        Assert.Equal(3, hitPoints.AbsoluteDifference);
        Assert.Equal(2.5m, hitPoints.PercentDifference);

        var manaRegen = Assert.Single(
            comparison.DerivedDifferences,
            item => item.FormulaId == "formula-mana-regen-dark-knight");
        Assert.Equal(0, manaRegen.FirstVisible);
        Assert.Equal(1, manaRegen.SecondVisible);
        Assert.Equal(1, manaRegen.AbsoluteDifference);
        Assert.Null(manaRegen.PercentDifference);
    }

    [Fact]
    public void ListsFormulasPresentOnASingleSideBetweenClasses()
    {
        var useCase = CreateUseCase();
        var darkKnight = CreateDarkKnightBuild("build-dark-knight", strengthBonus: 0, vitalityBonus: 0, energyBonus: 0);
        var fairyElf = CreateClassBuild("build-fairy-elf", "class-fairy-elf", "evolution-fairy-elf");

        var comparison = useCase.Execute(darkKnight, fairyElf);

        Assert.Empty(comparison.DerivedDifferences);
        Assert.All(
            comparison.OnlyInFirst,
            item => Assert.StartsWith("formula-", item.FormulaId, StringComparison.Ordinal));
        Assert.Contains(
            comparison.OnlyInFirst,
            item => item.FormulaId == "formula-hp-dark-knight");
        Assert.Contains(
            comparison.OnlyInSecond,
            item => item.FormulaId == "formula-hp-fairy-elf");
        Assert.Equal(
            ExpectedSharedStatIds,
            comparison.StatDifferences.Select(item => item.StatId).ToArray());
    }

    [Fact]
    public void SwappingSidesNegatesAbsoluteDifferences()
    {
        var useCase = CreateUseCase();
        var first = CreateDarkKnightBuild("build-first", strengthBonus: 0, vitalityBonus: 0, energyBonus: 0);
        var second = CreateDarkKnightBuild("build-second", strengthBonus: 2, vitalityBonus: 1, energyBonus: 20);

        var forward = useCase.Execute(first, second);
        var swapped = useCase.Execute(second, first);

        Assert.Equal(
            forward.StatDifferences.Select(item => item.StatId),
            swapped.StatDifferences.Select(item => item.StatId));
        Assert.All(
            forward.StatDifferences.Zip(swapped.StatDifferences),
            pair => Assert.Equal(
                pair.First.AbsoluteDifference,
                pair.Second.AbsoluteDifference is null
                    ? null
                    : -pair.Second.AbsoluteDifference));
        Assert.Equal(
            forward.DerivedDifferences.Select(item => (item.FormulaId, item.FormulaVersion)),
            swapped.DerivedDifferences.Select(item => (item.FormulaId, item.FormulaVersion)));
        Assert.All(
            forward.DerivedDifferences.Zip(swapped.DerivedDifferences),
            pair =>
            {
                Assert.Equal(pair.First.FirstVisible, pair.Second.SecondVisible);
                Assert.Equal(pair.First.SecondVisible, pair.Second.FirstVisible);
                Assert.Equal(pair.First.AbsoluteDifference, -pair.Second.AbsoluteDifference);
            });
    }

    [Fact]
    public void RejectsComparingBuildWithItself()
    {
        var useCase = CreateUseCase();
        var first = CreateDarkKnightBuild("build-same", strengthBonus: 0, vitalityBonus: 0, energyBonus: 0);
        var second = CreateDarkKnightBuild("build-same", strengthBonus: 5, vitalityBonus: 0, energyBonus: 0);

        var exception = Assert.Throws<BuildComparisonException>(
            () => useCase.Execute(first, second));

        Assert.Equal(BuildComparisonErrorCodes.SameBuild, exception.Code);
    }

    [Fact]
    public void RejectsStatsOutsideTheClassDefinition()
    {
        var useCase = CreateUseCase();
        var first = CreateDarkKnightBuild("build-first", strengthBonus: 0, vitalityBonus: 0, energyBonus: 0);
        var belowBase = first with
        {
            Id = "build-below-base",
            Stats = new Dictionary<string, long>(first.Stats, StringComparer.Ordinal)
            {
                ["vitality"] = 1,
            },
        };

        var belowBaseException = Assert.Throws<BuildComparisonException>(
            () => useCase.Execute(first, belowBase));
        Assert.Equal(BuildComparisonErrorCodes.StatsMismatch, belowBaseException.Code);

        var unknownClass = first with { Id = "build-unknown-class", CharacterClassId = "class-missing" };
        var unknownClassException = Assert.Throws<BuildComparisonException>(
            () => useCase.Execute(first, unknownClass));
        Assert.Equal(BuildComparisonErrorCodes.UnknownClass, unknownClassException.Code);
    }

    private static CompareBuildsUseCase CreateUseCaseWithItems()
    {
        var progressionCatalog = CatalogForTest();
        var formulaCatalog =
            new JsonExecutableFormulaSnapshotReader().Read(CanonicalSnapshotRoot);
        var itemCatalog =
            new JsonItemCatalogSnapshotReader().Read(CanonicalSnapshotRoot);
        return new CompareBuildsUseCase(progressionCatalog, formulaCatalog, itemCatalog);
    }

    [Fact]
    public void EchoesScenarioAndFocusesModalityOutputs()
    {
        var useCase = CreateUseCase();
        var first = CreateDarkKnightBuild("build-first", strengthBonus: 0, vitalityBonus: 0, energyBonus: 0);
        var second = CreateDarkKnightBuild("build-second", strengthBonus: 2, vitalityBonus: 1, energyBonus: 20);

        var hunting = useCase.Execute(
            first,
            second,
            new BuildComparisonOptions(
                new ComparisonScenario(ComparisonModality.Pvm, "Hunt", null, null),
                []));
        Assert.Equal(ComparisonModality.Pvm, hunting.Scenario!.Modality);
        Assert.Equal("Hunt", hunting.Scenario.DisplayName);
        Assert.NotEmpty(hunting.FocusedReferences);
        Assert.All(
            hunting.FocusedReferences,
            item => Assert.Contains("-pvm-", item.Id, StringComparison.Ordinal));

        var duel = useCase.Execute(
            first,
            second,
            new BuildComparisonOptions(
                new ComparisonScenario(ComparisonModality.Pvp, null, null, null),
                []));
        Assert.NotEmpty(duel.FocusedReferences);
        Assert.All(
            duel.FocusedReferences,
            item => Assert.Contains("-pvp-", item.Id, StringComparison.Ordinal));

        var hybrid = useCase.Execute(
            first,
            second,
            new BuildComparisonOptions(
                new ComparisonScenario(ComparisonModality.Hybrid, null, null, null),
                []));
        Assert.Empty(hybrid.FocusedReferences);

        var plain = useCase.Execute(first, second);
        Assert.Null(plain.Scenario);
        Assert.Empty(plain.FocusedReferences);
        Assert.Empty(plain.BreakpointResults);
        Assert.Empty(plain.Warnings);
    }

    [Fact]
    public void EvaluatesStatBreakpointWithMarginsAndWarnings()
    {
        var useCase = CreateUseCase();
        var first = CreateDarkKnightBuild("build-first", strengthBonus: 0, vitalityBonus: 0, energyBonus: 0);
        var second = CreateDarkKnightBuild("build-second", strengthBonus: 2, vitalityBonus: 1, energyBonus: 20);

        var reachable = useCase.Execute(
            first,
            second,
            new BuildComparisonOptions(
                null,
                [new BreakpointTarget(BreakpointKind.Stat, "strength", 28)]));
        var reachableResult = Assert.Single(reachable.BreakpointResults);
        Assert.Equal(28, reachableResult.FirstValue);
        Assert.Equal(30, reachableResult.SecondValue);
        Assert.True(reachableResult.MeetsFirst);
        Assert.True(reachableResult.MeetsSecond);
        Assert.Equal(0, reachableResult.FirstMargin);
        Assert.Equal(2, reachableResult.SecondMargin);
        Assert.Empty(reachable.Warnings);

        var unreachable = useCase.Execute(
            first,
            second,
            new BuildComparisonOptions(
                null,
                [new BreakpointTarget(BreakpointKind.Stat, "strength", long.MaxValue)]));
        var missed = Assert.Single(unreachable.BreakpointResults);
        Assert.False(missed.MeetsFirst);
        Assert.False(missed.MeetsSecond);
        Assert.Equal(
            new[] { ComparisonSide.First, ComparisonSide.Second },
            unreachable.Warnings
                .Where(item => item.Code == ComparisonWarningCodes.TargetMissed)
                .Select(item => item.Side)
                .OrderBy(item => item)
                .ToArray());
    }

    [Fact]
    public void ResolvesDerivedBreakpointWithNullOnMissingSide()
    {
        var useCase = CreateUseCase();
        var darkKnight = CreateDarkKnightBuild("build-dark-knight", strengthBonus: 0, vitalityBonus: 0, energyBonus: 0);
        var fairyElf = CreateClassBuild("build-fairy-elf", "class-fairy-elf", "evolution-fairy-elf");

        var comparison = useCase.Execute(
            darkKnight,
            fairyElf,
            new BuildComparisonOptions(
                null,
                [new BreakpointTarget(BreakpointKind.Derived, "formula-hp-dark-knight@1.0.0", 120)]));
        var result = Assert.Single(comparison.BreakpointResults);
        Assert.Equal(120, result.FirstValue);
        Assert.Null(result.SecondValue);
        Assert.True(result.MeetsFirst);
        Assert.Null(result.MeetsSecond);
        Assert.DoesNotContain(
            comparison.Warnings,
            item => item.Side == ComparisonSide.Second);
    }

    [Fact]
    public void RejectsUnknownBreakpointKeys()
    {
        var useCase = CreateUseCase();
        var first = CreateDarkKnightBuild("build-first", strengthBonus: 0, vitalityBonus: 0, energyBonus: 0);
        var second = CreateDarkKnightBuild("build-second", strengthBonus: 2, vitalityBonus: 1, energyBonus: 20);

        foreach (var target in new[]
                 {
                     new BreakpointTarget(BreakpointKind.Stat, "missing-stat", 1),
                     new BreakpointTarget(BreakpointKind.Derived, "not-a-reference", 1),
                     new BreakpointTarget(BreakpointKind.Derived, "formula-missing@1.0.0", 1),
                 })
        {
            var exception = Assert.Throws<BuildComparisonException>(
                () => useCase.Execute(
                    first,
                    second,
                    new BuildComparisonOptions(null, [target])));
            Assert.Equal(BuildComparisonErrorCodes.UnknownBreakpoint, exception.Code);
        }
    }

    [Fact]
    public void RejectsInvalidScenario()
    {
        var useCase = CreateUseCase();
        var first = CreateDarkKnightBuild("build-first", strengthBonus: 0, vitalityBonus: 0, energyBonus: 0);
        var second = CreateDarkKnightBuild("build-second", strengthBonus: 2, vitalityBonus: 1, energyBonus: 20);

        var blankName = Assert.Throws<BuildComparisonException>(
            () => useCase.Execute(
                first,
                second,
                new BuildComparisonOptions(
                    new ComparisonScenario(ComparisonModality.Pvp, "  ", null, null),
                    [])));
        Assert.Equal(BuildComparisonErrorCodes.InvalidScenario, blankName.Code);

        var unknownModality = Assert.Throws<BuildComparisonException>(
            () => useCase.Execute(
                first,
                second,
                new BuildComparisonOptions(
                    new ComparisonScenario((ComparisonModality)99, null, null, null),
                    [])));
        Assert.Equal(BuildComparisonErrorCodes.InvalidScenario, unknownModality.Code);
    }

    [Fact]
    public void WarnsOnUnmetItemRequirementsFromThePublishedCatalog()
    {
        var useCase = CreateUseCaseWithItems();
        var first = CreateDarkKnightBuild("build-first", strengthBonus: 0, vitalityBonus: 0, energyBonus: 0);
        var second = CreateDarkKnightBuild("build-second", strengthBonus: 2, vitalityBonus: 1, energyBonus: 20);

        var comparison = useCase.Execute(
            first,
            second,
            new BuildComparisonOptions(null, []));
        var requirements = comparison.Warnings
            .Where(item => item.Code == ComparisonWarningCodes.RequirementUnmet)
            .ToArray();
        Assert.Contains(
            requirements,
            item => item.Side == ComparisonSide.First &&
                item.Detail.Contains("item-kris", StringComparison.Ordinal));
        Assert.Contains(
            requirements,
            item => item.Side == ComparisonSide.First &&
                item.Detail.Contains("item-dragon-armor", StringComparison.Ordinal));
        Assert.Contains(
            requirements,
            item => item.Side == ComparisonSide.Second &&
                item.Detail.Contains("item-kris", StringComparison.Ordinal));
        Assert.DoesNotContain(
            requirements,
            item => item.Detail.Contains("item-albatross-bow", StringComparison.Ordinal));
    }

    [Fact]
    public void OmitsRequirementWarningsWithoutAnItemCatalog()
    {
        var useCase = CreateUseCase();
        var first = CreateDarkKnightBuild("build-first", strengthBonus: 0, vitalityBonus: 0, energyBonus: 0);
        var second = CreateDarkKnightBuild("build-second", strengthBonus: 2, vitalityBonus: 1, energyBonus: 20);

        var comparison = useCase.Execute(
            first,
            second,
            new BuildComparisonOptions(
                null,
                [new BreakpointTarget(BreakpointKind.Stat, "strength", long.MaxValue)]));
        Assert.All(
            comparison.Warnings,
            item => Assert.Equal(ComparisonWarningCodes.TargetMissed, item.Code));
    }

    [Fact]
    public void MirrorsWarningsWhenSwappingSidesWithOptions()
    {
        var useCase = CreateUseCaseWithItems();
        var first = CreateDarkKnightBuild("build-first", strengthBonus: 0, vitalityBonus: 0, energyBonus: 0);
        var second = CreateClassBuild("build-second", "class-fairy-elf", "evolution-fairy-elf");
        var options = new BuildComparisonOptions(
            new ComparisonScenario(ComparisonModality.Pvm, "Hunt", null, null),
            [new BreakpointTarget(BreakpointKind.Stat, "strength", long.MaxValue)]);

        var forward = useCase.Execute(first, second, options);
        var swapped = useCase.Execute(second, first, options);

        Assert.Equal(forward.Warnings.Count, swapped.Warnings.Count);
        Assert.Equal(
            WarningsOnSide(forward, ComparisonSide.First),
            WarningsOnSide(swapped, ComparisonSide.Second));
        Assert.Equal(
            WarningsOnSide(forward, ComparisonSide.Second),
            WarningsOnSide(swapped, ComparisonSide.First));
    }

    private static string[] WarningsOnSide(BuildComparison comparison, ComparisonSide side) =>
        comparison.Warnings
            .Where(item => item.Side == side)
            .Select(item => $"{item.Code}|{item.Detail}")
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static CharacterBuild CreateDarkKnightBuild(
        string id,
        long strengthBonus,
        long vitalityBonus,
        long energyBonus)
    {
        var catalog = CatalogForTest();
        var characterClass = catalog.Classes.Single(
            item => item.Id == "class-dark-knight");
        var stats = new Dictionary<string, long>(StringComparer.Ordinal);
        foreach (var stat in characterClass.BaseStats)
        {
            var bonus = stat.Key switch
            {
                "strength" => strengthBonus,
                "vitality" => vitalityBonus,
                "energy" => energyBonus,
                _ => 0L,
            };
            stats[stat.Key] = checked(stat.Value.BaseValue + bonus);
        }

        return CreateBuild(id, "class-dark-knight", "evolution-dark-knight", 6, stats);
    }

    private static CharacterBuild CreateClassBuild(
        string id,
        string classId,
        string evolutionId)
    {
        var catalog = CatalogForTest();
        var characterClass = catalog.Classes.Single(item => item.Id == classId);
        var stats = characterClass.BaseStats.ToDictionary(
            stat => stat.Key,
            stat => stat.Value.BaseValue,
            StringComparer.Ordinal);
        return CreateBuild(id, classId, evolutionId, 6, stats);
    }

    private static CharacterBuild CreateBuild(
        string id,
        string classId,
        string evolutionId,
        int level,
        IReadOnlyDictionary<string, long> stats) =>
        new(
            CharacterBuild.CurrentSchemaVersion,
            id,
            new BuildDraftVersionedReference("mu-s4-global-reference", "1.0.0"),
            new BuildDraftDatasetReference(
                "synthetic-comparison",
                $"sha256:{new string('0', 64)}"),
            "0.2.0",
            classId,
            evolutionId,
            level,
            stats,
            [],
            0,
            0,
            []);

    private static string FindCanonicalSnapshotRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(
                directory.FullName,
                "packages",
                "rulesets",
                "mu-s4-global-reference",
                "v1");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "The canonical ruleset snapshot was not found above the test binaries.");
    }
}
