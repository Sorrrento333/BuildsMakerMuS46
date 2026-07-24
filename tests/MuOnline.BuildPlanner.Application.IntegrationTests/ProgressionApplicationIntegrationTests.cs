using System.Text.Json;
using System.Text.Json.Nodes;
using MuOnline.BuildPlanner.Application.Progression;
using MuOnline.BuildPlanner.Domain.Progression;
using Xunit;

namespace MuOnline.BuildPlanner.Application.IntegrationTests;

public sealed record ProgressionReferenceCase(
    string Id,
    string ProgressionRuleId,
    string ClassId,
    string EvolutionId,
    int Level,
    string[] CompletedQuestIds,
    long ExpectedEarnedPoints,
    string? ExpectedErrorCode);

public sealed class ProgressionApplicationIntegrationTests
{
    private static readonly string CanonicalSnapshotRoot = FindCanonicalSnapshotRoot();
    private static readonly ProgressionReferenceCase[] ValidCases =
        LoadReferenceCases("valid");
    private static readonly ProgressionReferenceCase[] InvalidCases =
        LoadReferenceCases("invalid");

    public static TheoryData<ProgressionReferenceCase> ApprovedCases =>
        CreateTheoryData(ValidCases);

    public static TheoryData<ProgressionReferenceCase> RejectedCases =>
        CreateTheoryData(InvalidCases);

    [Theory]
    [MemberData(nameof(ApprovedCases))]
    public void UseCaseReproducesApprovedCasesFromCanonicalSnapshot(
        ProgressionReferenceCase referenceCase)
    {
        var useCase = CreateUseCase(CanonicalSnapshotRoot);

        var result = useCase.Execute(ToRequest(referenceCase));

        Assert.Equal(referenceCase.ExpectedEarnedPoints, result.EarnedPoints);
        Assert.Equal(referenceCase.ClassId, result.CharacterClassId);
        Assert.Equal(referenceCase.ProgressionRuleId, result.ProgressionRuleId);
        Assert.Equal(
            referenceCase.ExpectedEarnedPoints,
            result.Contributions.Sum(item => item.EarnedPoints));
    }

    [Theory]
    [MemberData(nameof(RejectedCases))]
    public void UseCaseReproducesApprovedRejectionsFromCanonicalSnapshot(
        ProgressionReferenceCase referenceCase)
    {
        var useCase = CreateUseCase(CanonicalSnapshotRoot);

        var exception = Assert.Throws<ProgressionPointBudgetException>(
            () => useCase.Execute(ToRequest(referenceCase)));

        Assert.Equal(referenceCase.ExpectedErrorCode, exception.Code);
    }

    [Fact]
    public void ReaderMaterializesStatIdsWithoutDuplicatingCanonicalValues()
    {
        var catalog = new JsonProgressionRulesetSnapshotReader().Read(
            CanonicalSnapshotRoot);
        var characterClassDirectory = Path.Combine(
            CanonicalSnapshotRoot,
            "character-classes");

        foreach (var path in Directory.GetFiles(characterClassDirectory, "*.json"))
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            var element = document.RootElement;
            var classId = RequiredString(element, "id");
            var expectedStatIds = element.GetProperty("stats")
                .EnumerateObject()
                .Select(stat => stat.Name)
                .ToHashSet(StringComparer.Ordinal);
            var characterClass = Assert.Single(
                catalog.Classes,
                item => item.Id == classId);

            Assert.True(characterClass.StatIds.SetEquals(expectedStatIds));
        }
    }

    [Fact]
    public void ReaderFailsClosedWhenSnapshotContainsAnUnpublishedRule()
    {
        using var snapshot = TemporarySnapshot.CopyFrom(CanonicalSnapshotRoot);
        var rulePath = Directory.GetFiles(snapshot.ProgressionRulesDirectory, "*.json")
            .Order(StringComparer.Ordinal)
            .First();
        UpdateJson(rulePath, root => root["status"] = "REVIEWED");

        var exception = Assert.Throws<ProgressionSnapshotException>(
            () => new JsonProgressionRulesetSnapshotReader().Read(snapshot.Root));

        Assert.Equal(ProgressionSnapshotErrorCodes.RuleNotPublished, exception.Code);
    }

    [Fact]
    public void ReaderFailsClosedWhenClassReferencesAnUnknownRule()
    {
        using var snapshot = TemporarySnapshot.CopyFrom(CanonicalSnapshotRoot);
        var classPath = Directory.GetFiles(snapshot.CharacterClassesDirectory, "*.json")
            .Order(StringComparer.Ordinal)
            .First();
        UpdateJson(
            classPath,
            root => root["progressionRuleRefs"] = new JsonArray("progression-rule-not-present"));

        var exception = Assert.Throws<ProgressionSnapshotException>(
            () => new JsonProgressionRulesetSnapshotReader().Read(snapshot.Root));

        Assert.Equal(ProgressionSnapshotErrorCodes.ReferenceIncoherent, exception.Code);
    }

    private static CalculateProgressionPointBudgetUseCase CreateUseCase(string snapshotRoot)
    {
        var catalog = new JsonProgressionRulesetSnapshotReader().Read(snapshotRoot);
        return new CalculateProgressionPointBudgetUseCase(catalog);
    }

    private static ProgressionReferenceCase[] LoadReferenceCases(
        string classification)
    {
        var directory = Path.Combine(
            CanonicalSnapshotRoot,
            "reference-cases",
            "progression",
            classification);

        return Directory.GetFiles(directory, "*.json")
            .Order(StringComparer.Ordinal)
            .Select(path =>
            {
                using var document = JsonDocument.Parse(File.ReadAllText(path));
                var element = document.RootElement;
                return new ProgressionReferenceCase(
                    RequiredString(element, "id"),
                    RequiredString(element, "progressionRuleId"),
                    RequiredString(element, "classId"),
                    RequiredString(element, "evolutionId"),
                    element.GetProperty("level").GetInt32(),
                    StringArray(element, "completedQuestIds"),
                    element.GetProperty("expectedEarnedPoints").GetInt64(),
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

    private static ProgressionPointBudgetRequest ToRequest(
        ProgressionReferenceCase referenceCase) =>
        new(
            referenceCase.ClassId,
            referenceCase.EvolutionId,
            referenceCase.Level,
            referenceCase.CompletedQuestIds);

    private static TheoryData<ProgressionReferenceCase> CreateTheoryData(
        IEnumerable<ProgressionReferenceCase> cases)
    {
        var data = new TheoryData<ProgressionReferenceCase>();
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

        public string CharacterClassesDirectory =>
            Path.Combine(Root, "character-classes");

        public string ProgressionRulesDirectory =>
            Path.Combine(Root, "progression-rules");

        public static TemporarySnapshot CopyFrom(string sourceRoot)
        {
            var root = Path.Combine(
                Path.GetTempPath(),
                $"mu-build-planner-progression-{Guid.NewGuid():N}");
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
