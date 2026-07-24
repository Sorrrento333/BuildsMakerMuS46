namespace MuOnline.BuildPlanner.Application.Builds;

public sealed record SaveBuildDraftRequest(
    string Id,
    BuildDraftProgressionInputs ProgressionInputs,
    IReadOnlyDictionary<string, long> Allocations);

public sealed class SaveBuildDraftUseCase
{
    private readonly IBuildDraftRepository _repository;
    private readonly BuildDraftRuntimeContext _context;

    public SaveBuildDraftUseCase(
        IBuildDraftRepository repository,
        BuildDraftRuntimeContext context)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(context);
        _repository = repository;
        _context = context;
    }

    public async Task<BuildDraft> ExecuteAsync(
        SaveBuildDraftRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Id);
        ArgumentNullException.ThrowIfNull(request.ProgressionInputs);
        ArgumentNullException.ThrowIfNull(request.Allocations);

        var progressionInputs = request.ProgressionInputs with
        {
            CompletedQuestIds = request.ProgressionInputs.CompletedQuestIds.ToArray(),
        };
        var distribution = BuildDraftCalculation.Calculate(
            _context,
            progressionInputs,
            request.Allocations);
        var draft = new BuildDraft(
            BuildDraft.CurrentSchemaVersion,
            request.Id,
            _context.Ruleset,
            _context.Dataset,
            _context.EngineVersion,
            progressionInputs,
            distribution);

        await _repository.SaveAsync(draft, cancellationToken);
        return draft;
    }
}
