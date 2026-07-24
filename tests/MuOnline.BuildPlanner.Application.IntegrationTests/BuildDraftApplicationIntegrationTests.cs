using System.Text.Json;
using System.Text.Json.Nodes;
using MuOnline.BuildPlanner.Application.Builds;
using MuOnline.BuildPlanner.Application.Progression;
using MuOnline.BuildPlanner.Domain.Progression;
using Xunit;

namespace MuOnline.BuildPlanner.Application.IntegrationTests;

public sealed class BuildDraftApplicationIntegrationTests
{
    private static readonly BuildDraftRuntimeContext RuntimeContext = CreateRuntimeContext();

    [Fact]
    public async Task SaveAndLoadRevalidatesDraftThroughInMemoryRepository()
    {
        var repository = new InMemoryBuildDraftRepository();
        var saved = await new SaveBuildDraftUseCase(repository, RuntimeContext)
            .ExecuteAsync(
                CreateSaveRequest("draft-synthetic"),
                TestContext.Current.CancellationToken);

        var loaded = await new LoadBuildDraftUseCase(repository, RuntimeContext)
            .ExecuteAsync(saved.Id, TestContext.Current.CancellationToken);

        Assert.Equal(saved.Id, loaded.Id);
        Assert.Equal(saved.Ruleset, loaded.Ruleset);
        Assert.Equal(saved.Dataset, loaded.Dataset);
        Assert.Equal(saved.EngineVersion, loaded.EngineVersion);
        Assert.Equal(
            saved.ProgressionInputs.CompletedQuestIds,
            loaded.ProgressionInputs.CompletedQuestIds);
        Assert.Equal(
            saved.StatDistribution.Allocations,
            loaded.StatDistribution.Allocations);
        Assert.NotSame(saved.StatDistribution, loaded.StatDistribution);
        Assert.Equal(10, loaded.StatDistribution.EarnedPoints);
        Assert.Equal(7, loaded.StatDistribution.SpentPoints);
        Assert.Equal(3, loaded.StatDistribution.RemainingPoints);
    }

    [Fact]
    public async Task SaveReplacesExistingDraftWithSameId()
    {
        var repository = new InMemoryBuildDraftRepository();
        var useCase = new SaveBuildDraftUseCase(repository, RuntimeContext);
        await useCase.ExecuteAsync(
            CreateSaveRequest("draft-synthetic"),
            TestContext.Current.CancellationToken);
        var replacementRequest = CreateSaveRequest("draft-synthetic") with
        {
            Allocations = new Dictionary<string, long>(StringComparer.Ordinal)
            {
                ["stat-alpha"] = 5,
                ["stat-beta"] = 5,
            },
        };

        var replacement = await useCase.ExecuteAsync(
            replacementRequest,
            TestContext.Current.CancellationToken);
        var loaded = await new LoadBuildDraftUseCase(repository, RuntimeContext)
            .ExecuteAsync(replacement.Id, TestContext.Current.CancellationToken);

        Assert.Equal(10, loaded.StatDistribution.SpentPoints);
        Assert.Equal(0, loaded.StatDistribution.RemainingPoints);
        Assert.Equal(1, repository.Count);
    }

    [Fact]
    public async Task LoadRejectsMissingDraftWithStableCode()
    {
        var repository = new InMemoryBuildDraftRepository();

        var exception = await Assert.ThrowsAsync<BuildDraftException>(
            () => new LoadBuildDraftUseCase(repository, RuntimeContext)
                .ExecuteAsync("draft-missing", TestContext.Current.CancellationToken));

        Assert.Equal(BuildDraftErrorCodes.NotFound, exception.Code);
    }

    [Fact]
    public async Task LoadRejectsIncoherentIdentityBeforeRecalculation()
    {
        var draft = await CreateValidDraftAsync(TestContext.Current.CancellationToken);
        var repository = new InMemoryBuildDraftRepository();
        await repository.SaveAsync(
            draft with
            {
                StatDistribution = draft.StatDistribution with
                {
                    CharacterClassId = "class-other",
                },
            },
            TestContext.Current.CancellationToken);

        var exception = await Assert.ThrowsAsync<BuildDraftException>(
            () => new LoadBuildDraftUseCase(repository, RuntimeContext)
                .ExecuteAsync(draft.Id, TestContext.Current.CancellationToken));

        Assert.Equal(BuildDraftErrorCodes.SourceMismatch, exception.Code);
    }

    [Fact]
    public async Task LoadRejectsUnavailableExactDependencyMetadata()
    {
        var draft = await CreateValidDraftAsync(TestContext.Current.CancellationToken);
        var repository = new InMemoryBuildDraftRepository();
        await repository.SaveAsync(draft, TestContext.Current.CancellationToken);
        var unavailableContext = RuntimeContext with
        {
            Dataset = new BuildDraftDatasetReference(
                "synthetic-002",
                $"sha256:{new string('1', 64)}"),
        };

        var exception = await Assert.ThrowsAsync<BuildDraftException>(
            () => new LoadBuildDraftUseCase(repository, unavailableContext)
                .ExecuteAsync(draft.Id, TestContext.Current.CancellationToken));

        Assert.Equal(BuildDraftErrorCodes.DependencyUnavailable, exception.Code);
    }

    [Fact]
    public async Task LoadRejectsAlteredCalculatedCache()
    {
        var draft = await CreateValidDraftAsync(TestContext.Current.CancellationToken);
        var repository = new InMemoryBuildDraftRepository();
        await repository.SaveAsync(
            draft with
            {
                StatDistribution = draft.StatDistribution with
                {
                    SpentPoints = 8,
                    RemainingPoints = 2,
                },
            },
            TestContext.Current.CancellationToken);

        var exception = await Assert.ThrowsAsync<BuildDraftException>(
            () => new LoadBuildDraftUseCase(repository, RuntimeContext)
                .ExecuteAsync(draft.Id, TestContext.Current.CancellationToken));

        Assert.Equal(BuildDraftErrorCodes.RevalidationFailed, exception.Code);
    }

    [Fact]
    public async Task SerializableModelUsesExactSchemaPropertyNames()
    {
        var draft = await CreateValidDraftAsync(TestContext.Current.CancellationToken);

        var json = JsonSerializer.Serialize(draft);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var progressionInputs = root.GetProperty("progressionInputs");
        var distribution = root.GetProperty("statDistribution");

        Assert.Equal(BuildDraft.CurrentSchemaVersion, root.GetProperty("schemaVersion").GetString());
        Assert.Equal(draft.Id, root.GetProperty("id").GetString());
        Assert.Equal(
            draft.ProgressionInputs.CharacterClassId,
            progressionInputs.GetProperty("characterClassId").GetString());
        Assert.Equal(
            draft.StatDistribution.ProgressionRule.Id,
            distribution.GetProperty("progressionRule").GetProperty("id").GetString());
        Assert.False(root.TryGetProperty(nameof(BuildDraft.SchemaVersion), out _));
        var roundTripped = JsonSerializer.Deserialize<BuildDraft>(json);
        Assert.NotNull(roundTripped);
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(json),
                JsonSerializer.SerializeToNode(roundTripped)));
    }

    private static SaveBuildDraftRequest CreateSaveRequest(string id) =>
        new(
            id,
            new BuildDraftProgressionInputs(
                "class-synthetic",
                "evolution-synthetic",
                3,
                []),
            new Dictionary<string, long>(StringComparer.Ordinal)
            {
                ["stat-alpha"] = 4,
                ["stat-beta"] = 3,
            });

    private static async Task<BuildDraft> CreateValidDraftAsync(
        CancellationToken cancellationToken)
    {
        var repository = new InMemoryBuildDraftRepository();
        return await new SaveBuildDraftUseCase(repository, RuntimeContext)
            .ExecuteAsync(CreateSaveRequest("draft-synthetic"), cancellationToken);
    }

    private static BuildDraftRuntimeContext CreateRuntimeContext()
    {
        var characterClass = new CharacterProgressionDefinition(
            "class-synthetic",
            "ruleset-synthetic",
            new HashSet<string>(["stat-alpha", "stat-beta"], StringComparer.Ordinal),
            new HashSet<string>(["evolution-synthetic"], StringComparer.Ordinal),
            ["progression-synthetic"]);
        var rule = new ProgressionRuleDefinition(
            "progression-synthetic",
            "1.0.0",
            "ruleset-synthetic",
            ProgressionRuleStatus.Published,
            new HashSet<string>(["class-synthetic"], StringComparer.Ordinal),
            new LevelPointRule(5, 2),
            null);
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

        public int Count => _drafts.Count;

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
}
