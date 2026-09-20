using System.Text.Json;
using System.Text.Json.Nodes;
using MuOnline.BuildPlanner.Application.Items;
using MuOnline.BuildPlanner.Domain.Items;
using Xunit;

namespace MuOnline.BuildPlanner.Application.IntegrationTests;

public sealed record ItemReferenceCase(
    string Id,
    string ItemId,
    string ClassId,
    IReadOnlyDictionary<string, long> Stats,
    string? ExpectedErrorCode);

public sealed class ItemApplicationIntegrationTests
{
    private static readonly string CanonicalSnapshotRoot = FindCanonicalSnapshotRoot();
    private static readonly ItemReferenceCase[] ValidCases =
        LoadReferenceCases("valid");
    private static readonly ItemReferenceCase[] InvalidCases =
        LoadReferenceCases("invalid");

    public static TheoryData<ItemReferenceCase> ApprovedCases =>
        CreateTheoryData(ValidCases);

    public static TheoryData<ItemReferenceCase> RejectedCases =>
        CreateTheoryData(InvalidCases);

    [Fact]
    public void ReaderMaterializesPublishedItemsWithoutDuplicatingCanonicalValues()
    {
        var catalog = new JsonItemCatalogSnapshotReader().Read(CanonicalSnapshotRoot);
        var itemDirectory = Path.Combine(CanonicalSnapshotRoot, "items");

        Assert.Equal("mu-s4-global-reference", catalog.RulesetId);
        Assert.Equal(3, catalog.Items.Count);

        foreach (var path in Directory.GetFiles(itemDirectory, "*.json"))
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            var element = document.RootElement;
            var itemId = RequiredString(element, "id");
            var item = Assert.Single(catalog.Items, candidate => candidate.Id == itemId);

            Assert.Equal(RequiredString(element, "displayName"), item.DisplayName);
            Assert.Equal(RequiredString(element, "rulesetId"), item.RulesetId);
            Assert.Equal(ItemDefinitionStatus.Published, item.Status);
            Assert.True(item.Slots.SetEquals(StringArray(element, "slots")));
            Assert.True(
                item.AllowedClassIds.SetEquals(StringArray(element, "allowedClassIds")));
            var expectedStats = element.GetProperty("requiredStats")
                .EnumerateObject()
                .ToDictionary(stat => stat.Name, stat => stat.Value.GetInt64());
            Assert.Equal(expectedStats, item.RequiredStats);
            var expectedDefense = element.TryGetProperty("defense", out var defenseElement)
                ? defenseElement.GetInt64()
                : (long?)null;
            Assert.Equal(expectedDefense, item.Defense);
        }
    }

    [Fact]
    public void UseCaseExposesBaseAndDerivedDefenseFromCanonicalSnapshot()
    {
        var useCase = CreateUseCase(CanonicalSnapshotRoot);

        var result = useCase.Execute(new EquipItemRequest(
            "class-dark-knight",
            new Dictionary<string, long>(StringComparer.Ordinal)
            {
                ["strength"] = 232,
                ["agility"] = 73,
                ["vitality"] = 25,
                ["energy"] = 10,
            },
            "item-dragon-armor",
            Level: 7));

        Assert.Equal(37, result.Defense);
        Assert.Equal(49, result.DefenseAtLevel);
    }

    [Theory]
    [MemberData(nameof(ApprovedCases))]
    public void UseCaseAcceptsApprovedCasesFromCanonicalSnapshot(
        ItemReferenceCase referenceCase)
    {
        var useCase = CreateUseCase(CanonicalSnapshotRoot);

        var result = useCase.Execute(ToRequest(referenceCase));

        Assert.Equal(referenceCase.ItemId, result.ItemId);
        Assert.NotEmpty(result.Slots);
    }

    [Theory]
    [MemberData(nameof(RejectedCases))]
    public void UseCaseRejectsApprovedCasesFromCanonicalSnapshot(
        ItemReferenceCase referenceCase)
    {
        var useCase = CreateUseCase(CanonicalSnapshotRoot);

        var exception = Assert.Throws<ItemEquipException>(
            () => useCase.Execute(ToRequest(referenceCase)));

        Assert.Equal(referenceCase.ExpectedErrorCode, exception.Code);
    }

    [Fact]
    public void ReaderFailsClosedWhenSnapshotContainsAnUnpublishedItem()
    {
        using var snapshot = TemporarySnapshot.CopyFrom(CanonicalSnapshotRoot);
        var itemPath = Directory.GetFiles(snapshot.ItemsDirectory, "*.json")
            .Order(StringComparer.Ordinal)
            .First();
        UpdateJson(itemPath, root => root["status"] = "REVIEWED");

        var exception = Assert.Throws<ItemCatalogSnapshotException>(
            () => new JsonItemCatalogSnapshotReader().Read(snapshot.Root));

        Assert.Equal(
            ItemCatalogSnapshotErrorCodes.ItemNotPublished,
            exception.Code);
    }

    [Fact]
    public void ReaderFailsClosedWhenItemRecordsSpanMultipleRulesets()
    {
        using var snapshot = TemporarySnapshot.CopyFrom(CanonicalSnapshotRoot);
        var itemPath = Directory.GetFiles(snapshot.ItemsDirectory, "*.json")
            .Order(StringComparer.Ordinal)
            .First();
        UpdateJson(itemPath, root => root["rulesetId"] = "mu-s4-other-reference");

        var exception = Assert.Throws<ItemCatalogSnapshotException>(
            () => new JsonItemCatalogSnapshotReader().Read(snapshot.Root));

        Assert.Equal(
            ItemCatalogSnapshotErrorCodes.RulesetMismatch,
            exception.Code);
    }

    [Fact]
    public void ReaderFailsClosedWhenItemDefenseIsNegative()
    {
        using var snapshot = TemporarySnapshot.CopyFrom(CanonicalSnapshotRoot);
        var itemPath = Directory.GetFiles(snapshot.ItemsDirectory, "*.json")
            .Order(StringComparer.Ordinal)
            .First(path => File.ReadAllText(path).Contains("\"defense\""));
        UpdateJson(itemPath, root => root["defense"] = -1);

        var exception = Assert.Throws<ItemCatalogSnapshotException>(
            () => new JsonItemCatalogSnapshotReader().Read(snapshot.Root));

        Assert.Equal(
            ItemCatalogSnapshotErrorCodes.SnapshotInvalid,
            exception.Code);
    }

    [Theory]
    [InlineData(0, 37)]
    [InlineData(1, 38)]
    [InlineData(7, 49)]
    [InlineData(10, 55)]
    [InlineData(15, 64)]
    public void DefenseCalculatorTruncatesOnlyAtOutputAcrossTheApprovedLevelRange(
        int level,
        long expectedDefenseAtLevel)
    {
        Assert.Equal(
            expectedDefenseAtLevel,
            ItemDefenseBonusCalculator.Calculate(37, level));
    }

    [Fact]
    public void DefenseCalculatorReturnsNullWithoutABaseDefense()
    {
        Assert.Null(ItemDefenseBonusCalculator.Calculate(null, 7));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ItemDefenseBonusCalculator.Calculate(37, -1));
    }

    [Fact]
    public void ReaderFailsClosedWhenItemDirectoryIsMissing()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            $"mu-build-planner-items-missing-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var exception = Assert.Throws<ItemCatalogSnapshotException>(
                () => new JsonItemCatalogSnapshotReader().Read(root));

            Assert.Equal(
                ItemCatalogSnapshotErrorCodes.SnapshotNotFound,
                exception.Code);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static EquipItemUseCase CreateUseCase(string snapshotRoot)
    {
        var catalog = new JsonItemCatalogSnapshotReader().Read(snapshotRoot);
        return new EquipItemUseCase(catalog);
    }

    private static ItemReferenceCase[] LoadReferenceCases(string classification)
    {
        var directory = Path.Combine(
            CanonicalSnapshotRoot,
            "reference-cases",
            "items",
            classification);

        return Directory.GetFiles(directory, "*.json")
            .Order(StringComparer.Ordinal)
            .Select(path =>
            {
                using var document = JsonDocument.Parse(File.ReadAllText(path));
                var element = document.RootElement;
                return new ItemReferenceCase(
                    RequiredString(element, "id"),
                    RequiredString(element, "itemId"),
                    RequiredString(element, "classId"),
                    element.GetProperty("stats")
                        .EnumerateObject()
                        .ToDictionary(stat => stat.Name, stat => stat.Value.GetInt64()),
                    element.TryGetProperty("expectedErrorCode", out var errorCode)
                        ? errorCode.GetString()
                        : null);
            })
            .ToArray();
    }

    private static void UpdateJson(string path, Action<JsonObject> update)
    {
        var root = JsonNode.Parse(File.ReadAllText(path))?.AsObject()
            ?? throw new InvalidDataException($"JSON object expected in '{path}'.");
        update(root);
        File.WriteAllText(
            path,
            root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    private static EquipItemRequest ToRequest(ItemReferenceCase referenceCase) =>
        new(
            referenceCase.ClassId,
            referenceCase.Stats,
            referenceCase.ItemId);

    private static TheoryData<ItemReferenceCase> CreateTheoryData(
        IEnumerable<ItemReferenceCase> cases)
    {
        var data = new TheoryData<ItemReferenceCase>();
        foreach (var referenceCase in cases)
        {
            data.Add(referenceCase);
        }

        return data;
    }

    private static string RequiredString(JsonElement element, string propertyName) =>
        element.GetProperty(propertyName).GetString()
        ?? throw new InvalidDataException($"'{propertyName}' cannot be null.");

    private static string[] StringArray(JsonElement element, string propertyName) =>
        element.GetProperty(propertyName)
            .EnumerateArray()
            .Select(item => item.GetString()
                ?? throw new InvalidDataException(
                    $"'{propertyName}' cannot contain null values."))
            .ToArray();

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

        public string ItemsDirectory => Path.Combine(Root, "items");

        public static TemporarySnapshot CopyFrom(string sourceRoot)
        {
            var root = Path.Combine(
                Path.GetTempPath(),
                $"mu-build-planner-items-{Guid.NewGuid():N}");
            Directory.CreateDirectory(root);
            CopyDirectory(Path.Combine(sourceRoot, "items"), root);
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
