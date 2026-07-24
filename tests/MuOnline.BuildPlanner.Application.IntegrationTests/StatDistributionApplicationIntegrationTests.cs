using MuOnline.BuildPlanner.Application.Progression;
using MuOnline.BuildPlanner.Application.Stats;
using MuOnline.BuildPlanner.Domain.Progression;
using MuOnline.BuildPlanner.Domain.Stats;
using Xunit;

namespace MuOnline.BuildPlanner.Application.IntegrationTests;

public sealed class StatDistributionApplicationIntegrationTests
{
    private static readonly string CanonicalSnapshotRoot = FindCanonicalSnapshotRoot();

    public static TheoryData<long, long> ValidDistributionCases =>
        new()
        {
            { 3L, 4L },
            { 0L, 7L },
        };

    [Theory]
    [MemberData(nameof(ValidDistributionCases))]
    public void UseCaseCalculatesSyntheticDistributionFromTemporarySnapshot(
        long expectedRemainingPoints,
        long secondAllocation)
    {
        using var snapshot = TemporarySnapshot.CopyFrom(CanonicalSnapshotRoot);
        var catalog = new JsonProgressionRulesetSnapshotReader().Read(snapshot.Root);
        var characterClass = catalog.Classes
            .OrderBy(item => item.Id, StringComparer.Ordinal)
            .First(item => item.StatIds.Count >= 2);
        var statIds = characterClass.StatIds
            .Order(StringComparer.Ordinal)
            .ToArray();
        var allocations = statIds.ToDictionary(
            statId => statId,
            _ => 0L,
            StringComparer.Ordinal);
        allocations[statIds[0]] = 3;
        allocations[statIds[1]] = secondAllocation;
        var budget = CreateSyntheticBudget(catalog, characterClass, earnedPoints: 10);
        var useCase = new CalculateStatDistributionUseCase(catalog);

        var result = useCase.Execute(budget, allocations);

        Assert.Equal(10 - expectedRemainingPoints, result.SpentPoints);
        Assert.Equal(expectedRemainingPoints, result.RemainingPoints);
        Assert.Equal(budget.CharacterClassId, result.CharacterClassId);
        Assert.Equal(budget.ProgressionRuleId, result.ProgressionRuleId);
        Assert.Equal(
            statIds,
            result.Allocations.Keys.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void UseCaseFailsClosedWhenBudgetRulesetDoesNotMatchTemporarySnapshot()
    {
        using var snapshot = TemporarySnapshot.CopyFrom(CanonicalSnapshotRoot);
        var catalog = new JsonProgressionRulesetSnapshotReader().Read(snapshot.Root);
        var characterClass = catalog.Classes
            .OrderBy(item => item.Id, StringComparer.Ordinal)
            .First();
        var budget = CreateSyntheticBudget(catalog, characterClass, earnedPoints: 10) with
        {
            RulesetId = "synthetic-other-ruleset",
        };
        var allocations = characterClass.StatIds.ToDictionary(
            statId => statId,
            _ => 0L,
            StringComparer.Ordinal);
        var useCase = new CalculateStatDistributionUseCase(catalog);

        var exception = Assert.Throws<StatDistributionException>(
            () => useCase.Execute(budget, allocations));

        Assert.Equal(StatDistributionErrorCodes.BudgetSourceMismatch, exception.Code);
    }

    [Fact]
    public void UseCasePropagatesTypedCalculatorErrors()
    {
        using var snapshot = TemporarySnapshot.CopyFrom(CanonicalSnapshotRoot);
        var catalog = new JsonProgressionRulesetSnapshotReader().Read(snapshot.Root);
        var characterClass = catalog.Classes
            .OrderBy(item => item.Id, StringComparer.Ordinal)
            .First();
        var budget = CreateSyntheticBudget(catalog, characterClass, earnedPoints: 10);
        var allocations = characterClass.StatIds.ToDictionary(
            statId => statId,
            _ => 0L,
            StringComparer.Ordinal);
        allocations[characterClass.StatIds.Order(StringComparer.Ordinal).First()] = -1;
        var useCase = new CalculateStatDistributionUseCase(catalog);

        var exception = Assert.Throws<StatDistributionException>(
            () => useCase.Execute(budget, allocations));

        Assert.Equal(StatDistributionErrorCodes.AllocationNegative, exception.Code);
    }

    private static ProgressionPointBudgetResult CreateSyntheticBudget(
        ProgressionRulesetCatalog catalog,
        CharacterProgressionDefinition characterClass,
        long earnedPoints)
    {
        var rule = Assert.Single(
            catalog.Rules,
            item => characterClass.ProgressionRuleRefs.Contains(item.Id, StringComparer.Ordinal));

        return new ProgressionPointBudgetResult(
            catalog.RulesetId,
            characterClass.Id,
            rule.Id,
            rule.Version,
            earnedPoints,
            []);
    }

    private static string FindCanonicalSnapshotRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine(
                current.FullName,
                "packages",
                "rulesets",
                "mu-s4-global-reference",
                "v1");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Canonical ruleset snapshot was not found.");
    }

    private sealed class TemporarySnapshot : IDisposable
    {
        private TemporarySnapshot(string root)
        {
            Root = root;
        }

        public string Root { get; }

        public static TemporarySnapshot CopyFrom(string sourceRoot)
        {
            var root = Path.Combine(
                Path.GetTempPath(),
                $"mu-build-planner-stat-distribution-{Guid.NewGuid():N}");
            Directory.CreateDirectory(root);
            CopyDirectory(Path.Combine(sourceRoot, "character-classes"), root);
            CopyDirectory(Path.Combine(sourceRoot, "progression-rules"), root);
            return new TemporarySnapshot(root);
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }

        private static void CopyDirectory(string sourceDirectory, string destinationRoot)
        {
            var destinationDirectory = Path.Combine(
                destinationRoot,
                Path.GetFileName(sourceDirectory));
            Directory.CreateDirectory(destinationDirectory);

            foreach (var sourcePath in Directory.GetFiles(sourceDirectory, "*.json"))
            {
                File.Copy(
                    sourcePath,
                    Path.Combine(destinationDirectory, Path.GetFileName(sourcePath)));
            }
        }
    }
}
