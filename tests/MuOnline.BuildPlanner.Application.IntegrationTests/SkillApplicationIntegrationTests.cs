using System.Text.Json;
using System.Text.Json.Nodes;
using MuOnline.BuildPlanner.Application.Skills;
using MuOnline.BuildPlanner.Domain.Skills;
using Xunit;

namespace MuOnline.BuildPlanner.Application.IntegrationTests;

public sealed record SkillReferenceCase(
    string Id,
    string SkillId,
    string EvolutionId,
    long FinalLevel,
    string? ExpectedErrorCode);

public sealed class SkillApplicationIntegrationTests
{
    private static readonly string CanonicalSnapshotRoot = FindCanonicalSnapshotRoot();
    private static readonly SkillReferenceCase[] ValidCases =
        LoadReferenceCases("valid");
    private static readonly SkillReferenceCase[] InvalidCases =
        LoadReferenceCases("invalid");

    public static TheoryData<SkillReferenceCase> ApprovedCases =>
        CreateTheoryData(ValidCases);

    public static TheoryData<SkillReferenceCase> RejectedCases =>
        CreateTheoryData(InvalidCases);

    [Fact]
    public void ReaderMaterializesPublishedSkillsWithoutDuplicatingCanonicalValues()
    {
        var catalog = new JsonSkillCatalogSnapshotReader().Read(CanonicalSnapshotRoot);
        var skillDirectory = Path.Combine(CanonicalSnapshotRoot, "skills");

        Assert.Equal("mu-s4-global-reference", catalog.RulesetId);
        Assert.Equal(8, catalog.Skills.Count);

        foreach (var path in Directory.GetFiles(skillDirectory, "*.json"))
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            var element = document.RootElement;
            var skillId = RequiredString(element, "id");
            var skill = Assert.Single(catalog.Skills, candidate => candidate.Id == skillId);

            Assert.Equal(RequiredString(element, "displayName"), skill.DisplayName);
            Assert.Equal(RequiredString(element, "rulesetId"), skill.RulesetId);
            Assert.Equal(SkillDefinitionStatus.Published, skill.Status);
            Assert.Equal(ParseKind(RequiredString(element, "kind")), skill.Kind);
            Assert.Equal(
                element.GetProperty("requiredLevel").GetInt64(),
                skill.RequiredLevel);
            Assert.True(
                skill.AllowedEvolutionIds.SetEquals(
                    StringArray(element, "allowedEvolutionIds")));
        }
    }

    [Theory]
    [MemberData(nameof(ApprovedCases))]
    public void UseCaseAcceptsApprovedCasesFromCanonicalSnapshot(
        SkillReferenceCase referenceCase)
    {
        var useCase = CreateUseCase(CanonicalSnapshotRoot);

        var result = useCase.Execute(ToRequest(referenceCase));

        Assert.Equal(referenceCase.SkillId, result.SkillId);
        Assert.NotEmpty(result.AllowedEvolutionIds);
    }

    [Theory]
    [MemberData(nameof(RejectedCases))]
    public void UseCaseRejectsApprovedCasesFromCanonicalSnapshot(
        SkillReferenceCase referenceCase)
    {
        var useCase = CreateUseCase(CanonicalSnapshotRoot);

        var exception = Assert.Throws<SkillLearnException>(
            () => useCase.Execute(ToRequest(referenceCase)));

        Assert.Equal(referenceCase.ExpectedErrorCode, exception.Code);
    }

    [Fact]
    public void ReaderFailsClosedWhenSnapshotContainsAnUnpublishedSkill()
    {
        using var snapshot = TemporarySnapshot.CopyFrom(CanonicalSnapshotRoot);
        var skillPath = Directory.GetFiles(snapshot.SkillsDirectory, "*.json")
            .Order(StringComparer.Ordinal)
            .First();
        UpdateJson(skillPath, root => root["status"] = "REVIEWED");

        var exception = Assert.Throws<SkillCatalogSnapshotException>(
            () => new JsonSkillCatalogSnapshotReader().Read(snapshot.Root));

        Assert.Equal(
            SkillCatalogSnapshotErrorCodes.SkillNotPublished,
            exception.Code);
    }

    [Fact]
    public void ReaderFailsClosedWhenSkillRecordsSpanMultipleRulesets()
    {
        using var snapshot = TemporarySnapshot.CopyFrom(CanonicalSnapshotRoot);
        var skillPath = Directory.GetFiles(snapshot.SkillsDirectory, "*.json")
            .Order(StringComparer.Ordinal)
            .First();
        UpdateJson(skillPath, root => root["rulesetId"] = "mu-s4-other-reference");

        var exception = Assert.Throws<SkillCatalogSnapshotException>(
            () => new JsonSkillCatalogSnapshotReader().Read(snapshot.Root));

        Assert.Equal(
            SkillCatalogSnapshotErrorCodes.RulesetMismatch,
            exception.Code);
    }

    [Fact]
    public void ReaderFailsClosedWhenSkillDirectoryIsMissing()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            $"mu-build-planner-skills-missing-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var exception = Assert.Throws<SkillCatalogSnapshotException>(
                () => new JsonSkillCatalogSnapshotReader().Read(root));

            Assert.Equal(
                SkillCatalogSnapshotErrorCodes.SnapshotNotFound,
                exception.Code);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ReaderFailsClosedWhenRequiredLevelIsNotPositive()
    {
        using var snapshot = TemporarySnapshot.CopyFrom(CanonicalSnapshotRoot);
        var skillPath = Directory.GetFiles(snapshot.SkillsDirectory, "*.json")
            .Order(StringComparer.Ordinal)
            .First();
        UpdateJson(skillPath, root => root["requiredLevel"] = 0);

        var exception = Assert.Throws<SkillCatalogSnapshotException>(
            () => new JsonSkillCatalogSnapshotReader().Read(snapshot.Root));

        Assert.Equal(
            SkillCatalogSnapshotErrorCodes.SnapshotInvalid,
            exception.Code);
    }

    [Fact]
    public void ReaderFailsClosedWhenAllowedEvolutionIdsAreEmpty()
    {
        using var snapshot = TemporarySnapshot.CopyFrom(CanonicalSnapshotRoot);
        var skillPath = Directory.GetFiles(snapshot.SkillsDirectory, "*.json")
            .Order(StringComparer.Ordinal)
            .First();
        UpdateJson(skillPath, root => root["allowedEvolutionIds"] = new JsonArray());

        var exception = Assert.Throws<SkillCatalogSnapshotException>(
            () => new JsonSkillCatalogSnapshotReader().Read(snapshot.Root));

        Assert.Equal(
            SkillCatalogSnapshotErrorCodes.SnapshotInvalid,
            exception.Code);
    }

    private static LearnSkillUseCase CreateUseCase(string snapshotRoot)
    {
        var catalog = new JsonSkillCatalogSnapshotReader().Read(snapshotRoot);
        return new LearnSkillUseCase(catalog);
    }

    private static SkillReferenceCase[] LoadReferenceCases(string classification)
    {
        var directory = Path.Combine(
            CanonicalSnapshotRoot,
            "reference-cases",
            "skills",
            classification);

        return Directory.GetFiles(directory, "*.json")
            .Order(StringComparer.Ordinal)
            .Select(path =>
            {
                using var document = JsonDocument.Parse(File.ReadAllText(path));
                var element = document.RootElement;
                return new SkillReferenceCase(
                    RequiredString(element, "id"),
                    RequiredString(element, "skillId"),
                    RequiredString(element, "evolutionId"),
                    element.GetProperty("finalLevel").GetInt64(),
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

    private static LearnSkillRequest ToRequest(SkillReferenceCase referenceCase) =>
        new(
            referenceCase.EvolutionId,
            referenceCase.FinalLevel,
            referenceCase.SkillId);

    private static TheoryData<SkillReferenceCase> CreateTheoryData(
        IEnumerable<SkillReferenceCase> cases)
    {
        var data = new TheoryData<SkillReferenceCase>();
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

    private static SkillKind ParseKind(string value) =>
        value switch
        {
            "ACTIVE" => SkillKind.Active,
            "PASSIVE" => SkillKind.Passive,
            "BUFF" => SkillKind.Buff,
            "SUMMON" => SkillKind.Summon,
            _ => throw new InvalidDataException($"Unsupported skill kind '{value}'."),
        };

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

        public string SkillsDirectory => Path.Combine(Root, "skills");

        public static TemporarySnapshot CopyFrom(string sourceRoot)
        {
            var root = Path.Combine(
                Path.GetTempPath(),
                $"mu-build-planner-skills-{Guid.NewGuid():N}");
            Directory.CreateDirectory(root);
            CopyDirectory(Path.Combine(sourceRoot, "skills"), root);
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