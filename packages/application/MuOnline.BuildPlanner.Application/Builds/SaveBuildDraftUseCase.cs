namespace MuOnline.BuildPlanner.Application.Builds;

public sealed record SaveBuildDraftRequest(
    string Id,
    BuildDraftProgressionInputs ProgressionInputs,
    BuildDraftResetInputs ResetInputs,
    IReadOnlyDictionary<string, long> Allocations,
    IReadOnlyList<BuildEquipmentEntry>? Equipment = null);

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
        ArgumentNullException.ThrowIfNull(request.ResetInputs);
        ArgumentNullException.ThrowIfNull(request.Allocations);

        var progressionInputs = request.ProgressionInputs with
        {
            CompletedQuestIds = request.ProgressionInputs.CompletedQuestIds.ToArray(),
        };
        var equipment = request.Equipment is null
            ? Array.Empty<BuildEquipmentEntry>()
            : request.Equipment.ToArray();
        var distribution = BuildDraftCalculation.Calculate(
            _context,
            progressionInputs,
            request.ResetInputs,
            request.Allocations);

        try
        {
            if (equipment.Length != 0)
            {
                var characterClass = BuildValidation.ResolveCharacterClass(
                    _context.Catalog,
                    request.Id,
                    progressionInputs.CharacterClassId,
                    progressionInputs.EvolutionId);
                var finalStats = BuildValidation.ComputeFinalStats(
                    characterClass,
                    request.Allocations,
                    request.Id);
                BuildEquipmentValidator.EnsureValid(
                    _context.ItemCatalog,
                    progressionInputs.CharacterClassId,
                    finalStats,
                    equipment);
            }
        }
        catch (BuildEquipmentValidationException exception)
        {
            throw Error(
                MapEquipmentCode(exception.Code),
                exception.Message);
        }

        var draft = new BuildDraft(
            BuildDraft.CurrentSchemaVersion,
            request.Id,
            _context.Ruleset,
            _context.Dataset,
            _context.EngineVersion,
            progressionInputs,
            request.ResetInputs,
            equipment,
            distribution);

        await _repository.SaveAsync(draft, cancellationToken);
        return draft;
    }

    private static string MapEquipmentCode(string code) => code switch
    {
        "item-not-found" => BuildDraftErrorCodes.EquipmentItemNotFound,
        "version-mismatch" => BuildDraftErrorCodes.EquipmentVersionMismatch,
        "class-not-allowed" => BuildDraftErrorCodes.EquipmentClassNotAllowed,
        "level-out-of-range" => BuildDraftErrorCodes.EquipmentLevelOutOfRange,
        "duplicate" => BuildDraftErrorCodes.EquipmentDuplicate,
        _ => BuildDraftErrorCodes.EquipmentRequirementsNotMet,
    };

    private static BuildDraftException Error(string code, string message) =>
        new(code, message);
}
