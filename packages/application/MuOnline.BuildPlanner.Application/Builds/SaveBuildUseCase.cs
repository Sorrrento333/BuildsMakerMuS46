namespace MuOnline.BuildPlanner.Application.Builds;

public sealed record SaveBuildRequest(
    string BuildId,
    string DraftId);

public sealed class SaveBuildUseCase
{
    private readonly IBuildRepository _repository;
    private readonly LoadBuildDraftUseCase _loadBuildDraft;
    private readonly BuildDraftRuntimeContext _context;

    public SaveBuildUseCase(
        IBuildRepository repository,
        LoadBuildDraftUseCase loadBuildDraft,
        BuildDraftRuntimeContext context)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(loadBuildDraft);
        ArgumentNullException.ThrowIfNull(context);
        _repository = repository;
        _loadBuildDraft = loadBuildDraft;
        _context = context;
    }

    public async Task<CharacterBuild> ExecuteAsync(
        SaveBuildRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        BuildValidation.EnsureValidId(request.BuildId);

        var draft = await _loadBuildDraft.ExecuteAsync(
            request.DraftId,
            cancellationToken);
        BuildValidation.EnsureRuntimeContext(_context);

        var characterClass = BuildValidation.ResolveCharacterClass(
            _context.Catalog,
            request.BuildId,
            draft.ProgressionInputs.CharacterClassId,
            draft.ProgressionInputs.EvolutionId);
        var stats = BuildValidation.ComputeFinalStats(
            characterClass,
            draft.StatDistribution.Allocations,
            request.BuildId);
        if (draft.ProgressionInputs.Level < 1)
        {
            throw Error(
                BuildErrorCodes.RevalidationFailed,
                "The promoted draft must be at level one or above.");
        }

        var build = new CharacterBuild(
            CharacterBuild.CurrentSchemaVersion,
            request.BuildId,
            _context.Ruleset,
            _context.Dataset,
            _context.EngineVersion,
            draft.ProgressionInputs.CharacterClassId,
            draft.ProgressionInputs.EvolutionId,
            draft.ProgressionInputs.Level,
            stats,
            draft.ProgressionInputs.CompletedQuestIds.ToArray(),
            draft.ResetInputs.ResetCount,
            draft.ResetInputs.PointsPerReset);

        await _repository.SaveAsync(build, cancellationToken);
        return build;
    }

    private static BuildException Error(string code, string message) =>
        new(code, message);
}