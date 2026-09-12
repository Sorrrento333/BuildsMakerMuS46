namespace MuOnline.BuildPlanner.Application.Builds;

public sealed class LoadBuildUseCase
{
    private readonly IBuildRepository _repository;
    private readonly BuildDraftRuntimeContext _context;

    public LoadBuildUseCase(
        IBuildRepository repository,
        BuildDraftRuntimeContext context)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(context);
        _repository = repository;
        _context = context;
    }

    public async Task<CharacterBuild> ExecuteAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        BuildValidation.EnsureValidId(id);

        var stored = await _repository.LoadAsync(id, cancellationToken);
        if (stored is null)
        {
            throw Error(
                BuildErrorCodes.NotFound,
                $"Build '{id}' was not found.");
        }

        if (stored.SchemaVersion != CharacterBuild.CurrentSchemaVersion)
        {
            throw Error(
                BuildErrorCodes.SchemaUnsupported,
                $"Build '{id}' uses the unsupported schema version '{stored.SchemaVersion}'.");
        }

        BuildValidation.EnsureRuntimeContext(_context);

        if (stored.Ruleset != _context.Ruleset ||
            stored.Dataset != _context.Dataset ||
            stored.EngineVersion != _context.EngineVersion)
        {
            throw Error(
                BuildErrorCodes.DependencyUnavailable,
                "The exact ruleset, dataset or calculation engine is not available.");
        }

        var characterClass = BuildValidation.ResolveCharacterClass(
            _context.Catalog,
            stored.Id,
            stored.CharacterClassId,
            stored.EvolutionId);
        BuildValidation.EnsureStatsMatchCharacterClass(
            characterClass,
            stored.Stats);
        BuildValidation.EnsureFinalStatsAreReachable(
            characterClass,
            stored.Stats,
            stored.Id);

        return stored with
        {
            QuestIds = stored.QuestIds.ToArray(),
            Stats = new Dictionary<string, long>(
                stored.Stats,
                StringComparer.Ordinal),
        };
    }

    private static BuildException Error(string code, string message) =>
        new(code, message);
}