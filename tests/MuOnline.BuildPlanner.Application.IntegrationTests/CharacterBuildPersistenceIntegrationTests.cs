using System.Text.Json;
using System.Text.Json.Nodes;
using MuOnline.BuildPlanner.Application.Builds;
using MuOnline.BuildPlanner.Application.Progression;
using MuOnline.BuildPlanner.Application.Stats;
using MuOnline.BuildPlanner.Domain.Progression;
using MuOnline.BuildPlanner.Domain.Stats;
using Xunit;

namespace MuOnline.BuildPlanner.Application.IntegrationTests;

public sealed class CharacterBuildPersistenceIntegrationTests
{
    private static readonly BuildDraftRuntimeContext RuntimeContext = CreateRuntimeContext();

    [Fact]
    public async Task SavePromotesDraftAndLoadRevalidatesExactSnapshot()
    {
        var draftRepository = new InMemoryBuildDraftRepository();
        var buildRepository = new InMemoryBuildRepository();
        var loadDraft = new LoadBuildDraftUseCase(draftRepository, RuntimeContext);
        await new SaveBuildDraftUseCase(draftRepository, RuntimeContext)
            .ExecuteAsync(
                CreateSaveDraftRequest("draft-synthetic"),
                TestContext.Current.CancellationToken);

        var saved = await new SaveBuildUseCase(
                buildRepository,
                loadDraft,
                RuntimeContext)
            .ExecuteAsync(
                new SaveBuildRequest("build-synthetic", "draft-synthetic"),
                TestContext.Current.CancellationToken);

        Assert.Equal("build-synthetic", saved.Id);
        Assert.Equal(CharacterBuild.CurrentSchemaVersion, saved.SchemaVersion);
        Assert.Equal(14, saved.Stats["stat-alpha"]);
        Assert.Equal(9, saved.Stats["stat-beta"]);
        Assert.Equal(3, saved.Level);
        Assert.Equal(2, saved.ResetCount);
        Assert.Equal(100, saved.PointsPerReset);
        Assert.Equal(["quest-synthetic"], saved.QuestIds);
        Assert.Equal("class-synthetic", saved.CharacterClassId);
        Assert.Equal("evolution-synthetic", saved.EvolutionId);

        var loaded = await new LoadBuildUseCase(buildRepository, RuntimeContext)
            .ExecuteAsync(saved.Id, TestContext.Current.CancellationToken);

        Assert.Equal(saved.Id, loaded.Id);
        Assert.Equal(saved.SchemaVersion, loaded.SchemaVersion);
        Assert.Equal(saved.Ruleset, loaded.Ruleset);
        Assert.Equal(saved.Dataset, loaded.Dataset);
        Assert.Equal(saved.EngineVersion, loaded.EngineVersion);
        Assert.Equal(saved.CharacterClassId, loaded.CharacterClassId);
        Assert.Equal(saved.EvolutionId, loaded.EvolutionId);
        Assert.Equal(saved.Level, loaded.Level);
        Assert.Equal(saved.ResetCount, loaded.ResetCount);
        Assert.Equal(saved.PointsPerReset, loaded.PointsPerReset);
        Assert.Equal(saved.Stats.Count, loaded.Stats.Count);
        Assert.Equal(saved.Stats["stat-alpha"], loaded.Stats["stat-alpha"]);
        Assert.Equal(saved.Stats["stat-beta"], loaded.Stats["stat-beta"]);
        Assert.Equal(saved.QuestIds, loaded.QuestIds);
        Assert.NotSame(saved.Stats, loaded.Stats);
        Assert.NotSame(saved.QuestIds, loaded.QuestIds);
    }

    [Fact]
    public async Task SaveReplacesExistingBuildWithSameId()
    {
        var draftRepository = new InMemoryBuildDraftRepository();
        var buildRepository = new InMemoryBuildRepository();
        var loadDraft = new LoadBuildDraftUseCase(draftRepository, RuntimeContext);
        var saveBuild = new SaveBuildUseCase(
            buildRepository,
            loadDraft,
            RuntimeContext);
        await new SaveBuildDraftUseCase(draftRepository, RuntimeContext)
            .ExecuteAsync(
                CreateSaveDraftRequest("draft-synthetic"),
                TestContext.Current.CancellationToken);
        await new SaveBuildDraftUseCase(draftRepository, RuntimeContext)
            .ExecuteAsync(
                CreateSaveDraftRequest("draft-replacement"),
                TestContext.Current.CancellationToken);

        await saveBuild.ExecuteAsync(
            new SaveBuildRequest("build-synthetic", "draft-synthetic"),
            TestContext.Current.CancellationToken);
        var replacement = await saveBuild.ExecuteAsync(
            new SaveBuildRequest("build-synthetic", "draft-replacement"),
            TestContext.Current.CancellationToken);
        var loaded = await new LoadBuildUseCase(buildRepository, RuntimeContext)
            .ExecuteAsync(replacement.Id, TestContext.Current.CancellationToken);

        Assert.Equal(1, buildRepository.Count);
        Assert.Equal(replacement.Id, loaded.Id);
        Assert.Equal(replacement.Ruleset, loaded.Ruleset);
        Assert.Equal(replacement.Dataset, loaded.Dataset);
        Assert.Equal(replacement.EngineVersion, loaded.EngineVersion);
        Assert.Equal(replacement.CharacterClassId, loaded.CharacterClassId);
        Assert.Equal(replacement.EvolutionId, loaded.EvolutionId);
        Assert.Equal(replacement.Level, loaded.Level);
        Assert.Equal(replacement.ResetCount, loaded.ResetCount);
        Assert.Equal(replacement.PointsPerReset, loaded.PointsPerReset);
        Assert.Equal(replacement.Stats, loaded.Stats);
        Assert.Equal(replacement.QuestIds, loaded.QuestIds);
    }

    [Fact]
    public async Task SaveRejectsInvalidBuildIdWithStableCode()
    {
        var draftRepository = new InMemoryBuildDraftRepository();
        var buildRepository = new InMemoryBuildRepository();
        var loadDraft = new LoadBuildDraftUseCase(draftRepository, RuntimeContext);
        await new SaveBuildDraftUseCase(draftRepository, RuntimeContext)
            .ExecuteAsync(
                CreateSaveDraftRequest("draft-synthetic"),
                TestContext.Current.CancellationToken);

        var exception = await Assert.ThrowsAsync<BuildException>(
            () => new SaveBuildUseCase(buildRepository, loadDraft, RuntimeContext)
                .ExecuteAsync(
                    new SaveBuildRequest("INVALID-ID", "draft-synthetic"),
                    TestContext.Current.CancellationToken));

        Assert.Equal(BuildErrorCodes.SourceMismatch, exception.Code);
        Assert.Equal(0, buildRepository.Count);
    }

    [Fact]
    public async Task SaveRejectsMissingSourceDraft()
    {
        var draftRepository = new InMemoryBuildDraftRepository();
        var buildRepository = new InMemoryBuildRepository();
        var loadDraft = new LoadBuildDraftUseCase(draftRepository, RuntimeContext);

        var exception = await Assert.ThrowsAsync<BuildDraftException>(
            () => new SaveBuildUseCase(buildRepository, loadDraft, RuntimeContext)
                .ExecuteAsync(
                    new SaveBuildRequest("build-synthetic", "draft-missing"),
                    TestContext.Current.CancellationToken));

        Assert.Equal(BuildDraftErrorCodes.NotFound, exception.Code);
        Assert.Equal(0, buildRepository.Count);
    }

    [Fact]
    public async Task LoadInputsReproduceThePersistedDistribution()
    {
        var build = await CreateValidBuildAsync(
            new InMemoryBuildRepository(),
            TestContext.Current.CancellationToken);
        var characterClass = RuntimeContext.Catalog.Classes.Single(
            item => item.Id == build.CharacterClassId);
        var derivedAllocations = build.Stats.ToDictionary(
            item => item.Key,
            item => item.Value -
                characterClass.BaseStats[item.Key].BaseValue,
            StringComparer.Ordinal);
        var budget = new CalculateProgressionPointBudgetUseCase(RuntimeContext.Catalog)
            .Execute(
                new ProgressionPointBudgetRequest(
                    build.CharacterClassId,
                    build.EvolutionId,
                    build.Level,
                    build.QuestIds));
        var distribution = new CalculateStatDistributionUseCase(RuntimeContext.Catalog)
            .Execute(
                budget,
                new ResetPointInputs(
                    build.ResetCount,
                    build.PointsPerReset),
                derivedAllocations);

        Assert.Equal(200, distribution.ResetPoints);
        Assert.Equal(7, distribution.SpentPoints);
        Assert.Equal(
            distribution.TotalDistributablePoints,
            distribution.SpentPoints + distribution.RemainingPoints);
        Assert.Equal(
            new Dictionary<string, long>(StringComparer.Ordinal)
            {
                ["stat-alpha"] = 4,
                ["stat-beta"] = 3,
            },
            distribution.Allocations);
    }

    [Fact]
    public async Task ListOrdersSavedBuildsByOrdinalId()
    {
        var draftRepository = new InMemoryBuildDraftRepository();
        var buildRepository = new InMemoryBuildRepository();
        var loadDraft = new LoadBuildDraftUseCase(draftRepository, RuntimeContext);
        var saveBuild = new SaveBuildUseCase(buildRepository, loadDraft, RuntimeContext);
        var saveDraft = new SaveBuildDraftUseCase(draftRepository, RuntimeContext);
        await saveDraft.ExecuteAsync(
            CreateSaveDraftRequest("draft-second"),
            TestContext.Current.CancellationToken);
        await saveDraft.ExecuteAsync(
            CreateSaveDraftRequest("draft-first"),
            TestContext.Current.CancellationToken);
        await saveBuild.ExecuteAsync(
            new SaveBuildRequest("build-second", "draft-second"),
            TestContext.Current.CancellationToken);
        await saveBuild.ExecuteAsync(
            new SaveBuildRequest("build-first", "draft-first"),
            TestContext.Current.CancellationToken);

        var listed = await new ListBuildsUseCase(buildRepository)
            .ExecuteAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["build-first", "build-second"], listed.Select(item => item.Id));
        var first = listed.Single(item => item.Id == "build-first");
        Assert.Equal(CharacterBuild.CurrentSchemaVersion, first.SchemaVersion);
        Assert.Equal("class-synthetic", first.CharacterClassId);
        Assert.Equal("evolution-synthetic", first.EvolutionId);
        Assert.Equal(3, first.Level);
        Assert.Equal(2, first.ResetCount);
        Assert.Equal(100, first.PointsPerReset);
        Assert.Equal("synthetic-001", first.DatasetVersion);
    }

    [Fact]
    public async Task ListReturnsEmptyWhenNoBuildWasSaved()
    {
        var listed = await new ListBuildsUseCase(new InMemoryBuildRepository())
            .ExecuteAsync(TestContext.Current.CancellationToken);

        Assert.Empty(listed);
    }

    [Fact]
    public async Task LoadRejectsMissingBuildWithStableCode()
    {
        var repository = new InMemoryBuildRepository();

        var exception = await Assert.ThrowsAsync<BuildException>(
            () => new LoadBuildUseCase(repository, RuntimeContext)
                .ExecuteAsync("build-missing", TestContext.Current.CancellationToken));

        Assert.Equal(BuildErrorCodes.NotFound, exception.Code);
    }

    [Fact]
    public async Task LoadRejectsUnsupportedSchemaVersion()
    {
        var repository = new InMemoryBuildRepository();
        var build = await CreateValidBuildAsync(repository, TestContext.Current.CancellationToken);
        await repository.SaveAsync(
            build with { SchemaVersion = "2.0.0" },
            TestContext.Current.CancellationToken);

        var exception = await Assert.ThrowsAsync<BuildException>(
            () => new LoadBuildUseCase(repository, RuntimeContext)
                .ExecuteAsync(build.Id, TestContext.Current.CancellationToken));

        Assert.Equal(BuildErrorCodes.SchemaUnsupported, exception.Code);
    }

    [Fact]
    public async Task LoadRejectsUnavailableExactDependencyMetadata()
    {
        var repository = new InMemoryBuildRepository();
        var build = await CreateValidBuildAsync(repository, TestContext.Current.CancellationToken);
        var unavailableContext = RuntimeContext with
        {
            Dataset = new BuildDraftDatasetReference(
                "synthetic-002",
                $"sha256:{new string('1', 64)}"),
        };

        var exception = await Assert.ThrowsAsync<BuildException>(
            () => new LoadBuildUseCase(repository, unavailableContext)
                .ExecuteAsync(build.Id, TestContext.Current.CancellationToken));

        Assert.Equal(BuildErrorCodes.DependencyUnavailable, exception.Code);
    }

    [Fact]
    public async Task LoadRejectsIncoherentCharacterIdentity()
    {
        var repository = new InMemoryBuildRepository();
        var build = await CreateValidBuildAsync(repository, TestContext.Current.CancellationToken);
        await repository.SaveAsync(
            build with { CharacterClassId = "class-other" },
            TestContext.Current.CancellationToken);

        var exception = await Assert.ThrowsAsync<BuildException>(
            () => new LoadBuildUseCase(repository, RuntimeContext)
                .ExecuteAsync(build.Id, TestContext.Current.CancellationToken));

        Assert.Equal(BuildErrorCodes.SourceMismatch, exception.Code);
    }

    [Fact]
    public async Task LoadRejectsEvolutionTheClassDoesNotOffer()
    {
        var repository = new InMemoryBuildRepository();
        var build = await CreateValidBuildAsync(repository, TestContext.Current.CancellationToken);
        await repository.SaveAsync(
            build with { EvolutionId = "evolution-other" },
            TestContext.Current.CancellationToken);

        var exception = await Assert.ThrowsAsync<BuildException>(
            () => new LoadBuildUseCase(repository, RuntimeContext)
                .ExecuteAsync(build.Id, TestContext.Current.CancellationToken));

        Assert.Equal(BuildErrorCodes.SourceMismatch, exception.Code);
    }

    [Fact]
    public async Task LoadRejectsStatsBelowCanonicalBaseValue()
    {
        var repository = new InMemoryBuildRepository();
        var build = await CreateValidBuildAsync(repository, TestContext.Current.CancellationToken);
        var alteredStats = new Dictionary<string, long>(
            build.Stats,
            StringComparer.Ordinal)
        {
            ["stat-alpha"] = 4,
        };
        await repository.SaveAsync(
            build with { Stats = alteredStats },
            TestContext.Current.CancellationToken);

        var exception = await Assert.ThrowsAsync<BuildException>(
            () => new LoadBuildUseCase(repository, RuntimeContext)
                .ExecuteAsync(build.Id, TestContext.Current.CancellationToken));

        Assert.Equal(BuildErrorCodes.RevalidationFailed, exception.Code);
    }

    [Fact]
    public async Task LoadRejectsStatSetThatDoesNotMatchCharacterClass()
    {
        var repository = new InMemoryBuildRepository();
        var build = await CreateValidBuildAsync(repository, TestContext.Current.CancellationToken);
        var alteredStats = new Dictionary<string, long>(StringComparer.Ordinal)
        {
            ["stat-alpha"] = build.Stats["stat-alpha"],
            ["stat-extra"] = 20,
        };
        await repository.SaveAsync(
            build with { Stats = alteredStats },
            TestContext.Current.CancellationToken);

        var exception = await Assert.ThrowsAsync<BuildException>(
            () => new LoadBuildUseCase(repository, RuntimeContext)
                .ExecuteAsync(build.Id, TestContext.Current.CancellationToken));

        Assert.Equal(BuildErrorCodes.SourceMismatch, exception.Code);
    }

    [Fact]
    public async Task SerializableModelUsesExactSchemaPropertyNames()
    {
        var build = await CreateValidBuildAsync(
            new InMemoryBuildRepository(),
            TestContext.Current.CancellationToken);

        var json = JsonSerializer.Serialize(build);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal(CharacterBuild.CurrentSchemaVersion, root.GetProperty("schemaVersion").GetString());
        Assert.Equal(build.Id, root.GetProperty("id").GetString());
        Assert.Equal(build.Ruleset.Id, root.GetProperty("ruleset").GetProperty("id").GetString());
        Assert.Equal(build.Dataset.Version, root.GetProperty("dataset").GetProperty("version").GetString());
        Assert.Equal(build.EngineVersion, root.GetProperty("engineVersion").GetString());
        Assert.Equal(build.CharacterClassId, root.GetProperty("characterClassId").GetString());
        Assert.Equal(build.EvolutionId, root.GetProperty("evolutionId").GetString());
        Assert.Equal(build.Level, root.GetProperty("level").GetInt32());
        Assert.Equal(build.ResetCount, root.GetProperty("resetCount").GetInt64());
        Assert.Equal(build.PointsPerReset, root.GetProperty("pointsPerReset").GetInt64());
        Assert.Equal(
            build.Stats["stat-alpha"],
            root.GetProperty("stats").GetProperty("stat-alpha").GetInt64());
        Assert.Equal(build.QuestIds.Count, root.GetProperty("questIds").GetArrayLength());
        Assert.False(root.TryGetProperty(nameof(CharacterBuild.SchemaVersion), out _));
        var roundTripped = JsonSerializer.Deserialize<CharacterBuild>(json);
        Assert.NotNull(roundTripped);
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(json),
                JsonSerializer.SerializeToNode(roundTripped)));
    }

    private static SaveBuildDraftRequest CreateSaveDraftRequest(string id) =>
        new(
            id,
            new BuildDraftProgressionInputs(
                "class-synthetic",
                "evolution-synthetic",
3,
            ["quest-synthetic"]),
            new BuildDraftResetInputs(2, 100),
            new Dictionary<string, long>(StringComparer.Ordinal)
            {
                ["stat-alpha"] = 4,
                ["stat-beta"] = 3,
            });

    private static async Task<CharacterBuild> CreateValidBuildAsync(
        IBuildRepository buildRepository,
        CancellationToken cancellationToken)
    {
        var draftRepository = new InMemoryBuildDraftRepository();
        await new SaveBuildDraftUseCase(draftRepository, RuntimeContext)
            .ExecuteAsync(
                CreateSaveDraftRequest("draft-synthetic"),
                cancellationToken);
        return await new SaveBuildUseCase(
                buildRepository,
                new LoadBuildDraftUseCase(draftRepository, RuntimeContext),
                RuntimeContext)
            .ExecuteAsync(
                new SaveBuildRequest("build-synthetic", "draft-synthetic"),
                cancellationToken);
    }

    private static BuildDraftRuntimeContext CreateRuntimeContext()
    {
        var characterClass = new CharacterProgressionDefinition(
            "class-synthetic",
            "ruleset-synthetic",
            new HashSet<string>(["stat-alpha", "stat-beta"], StringComparer.Ordinal),
            new HashSet<string>(["evolution-synthetic"], StringComparer.Ordinal),
            ["progression-synthetic"],
            [
                new CharacterBaseStatDefinition(
                    "stat-alpha",
                    10,
                    ["evidence-synthetic"]),
                new CharacterBaseStatDefinition(
                    "stat-beta",
                    6,
                    ["evidence-synthetic"]),
            ]);
        var rule = new ProgressionRuleDefinition(
            "progression-synthetic",
            "1.0.0",
            "ruleset-synthetic",
            ProgressionRuleStatus.Published,
            new HashSet<string>(["class-synthetic"], StringComparer.Ordinal),
            new LevelPointRule(5, 2),
            new QuestPointBonusRule(
                "quest-synthetic",
                1,
                new HashSet<string>(["evolution-synthetic"], StringComparer.Ordinal),
                2,
                1));
        var catalog = new ProgressionRulesetCatalog(
            "ruleset-synthetic",
            [characterClass],
            [rule],
            [
                new ProgressionCharacterOption(
                    "class-synthetic",
                    "Synthetic class",
                    [new ProgressionEvolutionOption("evolution-synthetic", "Synthetic evolution", 0)]),
            ]);

        return new BuildDraftRuntimeContext(
            catalog,
            new BuildDraftVersionedReference("ruleset-synthetic", "1.0.0"),
            new BuildDraftDatasetReference(
                "synthetic-001",
                $"sha256:{new string('0', 64)}"),
            "0.1.0");
    }

    private sealed class InMemoryBuildDraftRepository : IBuildDraftRepository
    {
        private readonly Dictionary<string, BuildDraft> _drafts =
            new(StringComparer.Ordinal);

        public Task SaveAsync(
            BuildDraft draft,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _drafts[draft.Id] = draft;
            return Task.CompletedTask;
        }

        public Task<BuildDraft?> LoadAsync(
            string id,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _drafts.TryGetValue(id, out var draft);
            return Task.FromResult(draft);
        }
    }

    private sealed class InMemoryBuildRepository : IBuildRepository
    {
        private readonly Dictionary<string, CharacterBuild> _builds =
            new(StringComparer.Ordinal);

        public int Count => _builds.Count;

        public Task SaveAsync(
            CharacterBuild build,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _builds[build.Id] = build;
            return Task.CompletedTask;
        }

        public Task<CharacterBuild?> LoadAsync(
            string id,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _builds.TryGetValue(id, out var build);
            return Task.FromResult(build);
        }

        public Task<IReadOnlyList<CharacterBuildSummary>> ListAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IReadOnlyList<CharacterBuildSummary> summaries = _builds.Values
                .Select(
                    build => new CharacterBuildSummary(
                        build.Id,
                        build.SchemaVersion,
                        build.CharacterClassId,
                        build.EvolutionId,
                        build.Level,
                        build.ResetCount,
                        build.PointsPerReset,
                        build.Dataset.Version))
                .OrderBy(item => item.Id, StringComparer.Ordinal)
                .ToArray();
            return Task.FromResult(summaries);
        }
    }
}