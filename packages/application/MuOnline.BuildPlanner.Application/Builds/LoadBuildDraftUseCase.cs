using MuOnline.BuildPlanner.Domain.Progression;
using MuOnline.BuildPlanner.Domain.Stats;

namespace MuOnline.BuildPlanner.Application.Builds;

public sealed class LoadBuildDraftUseCase
{
    private readonly IBuildDraftRepository _repository;
    private readonly BuildDraftRuntimeContext _context;

    public LoadBuildDraftUseCase(
        IBuildDraftRepository repository,
        BuildDraftRuntimeContext context)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(context);
        _repository = repository;
        _context = context;
    }

    public async Task<BuildDraft> ExecuteAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var stored = await _repository.LoadAsync(id, cancellationToken);
        if (stored is null)
        {
            throw Error(
                BuildDraftErrorCodes.NotFound,
                $"Build draft '{id}' was not found.");
        }

        stored = NormalizeSupportedSchema(id, stored);

        EnsureCoherentSources(id, stored);
        EnsureDependenciesAvailable(stored);

        BuildDraftStatDistribution recalculated;
        try
        {
            recalculated = BuildDraftCalculation.Calculate(
                _context,
                stored.ProgressionInputs,
                stored.ResetInputs,
                stored.StatDistribution.Allocations);
        }
        catch (Exception exception) when (
            exception is ProgressionPointBudgetException or
            StatDistributionException)
        {
            throw Error(
                BuildDraftErrorCodes.RevalidationFailed,
                $"Build draft '{id}' could not be recalculated.",
                exception);
        }

        if (!SameDistribution(stored.StatDistribution, recalculated))
        {
            throw Error(
                BuildDraftErrorCodes.RevalidationFailed,
                $"Build draft '{id}' does not match its recalculated cache.");
        }

        try
        {
            if (stored.Equipment is { Count: > 0 })
            {
                var characterClass = BuildValidation.ResolveCharacterClass(
                    _context.Catalog,
                    id,
                    stored.ProgressionInputs.CharacterClassId,
                    stored.ProgressionInputs.EvolutionId);
                var finalStats = BuildValidation.ComputeFinalStats(
                    characterClass,
                    stored.StatDistribution.Allocations,
                    id);
                BuildEquipmentValidator.EnsureValid(
                    _context.ItemCatalog,
                    stored.ProgressionInputs.CharacterClassId,
                    finalStats,
                    stored.Equipment ?? []);
            }
        }
        catch (BuildEquipmentValidationException exception)
        {
            throw Error(
                MapEquipmentCode(exception.Code),
                exception.Message);
        }
        catch (BuildException exception)
        {
            throw Error(
                exception.Code == BuildErrorCodes.SourceMismatch
                    ? BuildDraftErrorCodes.SourceMismatch
                    : BuildDraftErrorCodes.RevalidationFailed,
                exception.Message);
        }

        return stored with
        {
            ProgressionInputs = stored.ProgressionInputs with
            {
                CompletedQuestIds = stored.ProgressionInputs.CompletedQuestIds.ToArray(),
            },
            Equipment = stored.Equipment ?? [],
            StatDistribution = recalculated,
        };
    }

    private void EnsureDependenciesAvailable(BuildDraft draft)
    {
        BuildDraftCalculation.EnsureRuntimeContext(_context);

        if (draft.Ruleset != _context.Ruleset ||
            draft.Dataset != _context.Dataset ||
            draft.EngineVersion != _context.EngineVersion)
        {
            throw Error(
                BuildDraftErrorCodes.DependencyUnavailable,
                "The exact ruleset, dataset or calculation engine is not available.");
        }
    }

    private static BuildDraft NormalizeSupportedSchema(string id, BuildDraft stored)
    {
        if (stored.SchemaVersion == BuildDraft.CurrentSchemaVersion &&
            stored.StatDistribution.SchemaVersion ==
                BuildDraftStatDistribution.CurrentSchemaVersion)
        {
            return stored;
        }

        if (stored.SchemaVersion == BuildDraft.PreviousSchemaVersion &&
            stored.StatDistribution.SchemaVersion ==
                BuildDraftStatDistribution.CurrentSchemaVersion)
        {
            return stored with
            {
                SchemaVersion = BuildDraft.CurrentSchemaVersion,
                Equipment = [],
            };
        }

        if (stored.SchemaVersion ==
                BuildDraftStatDistribution.PreviousSchemaVersion &&
            stored.StatDistribution.SchemaVersion ==
                BuildDraftStatDistribution.PreviousSchemaVersion)
        {
            var zeroResetInputs = new BuildDraftResetInputs(0, 0);
            return stored with
            {
                SchemaVersion = BuildDraft.CurrentSchemaVersion,
                ResetInputs = zeroResetInputs,
                Equipment = [],
                StatDistribution = stored.StatDistribution with
                {
                    SchemaVersion = BuildDraftStatDistribution.CurrentSchemaVersion,
                    ResetInputs = zeroResetInputs,
                    ResetPoints = 0,
                    TotalDistributablePoints =
                        stored.StatDistribution.EarnedPoints,
                },
            };
        }

        throw Error(
            BuildDraftErrorCodes.SchemaUnsupported,
            $"Build draft '{id}' uses an unsupported schema version.");
    }

    private static void EnsureCoherentSources(string requestedId, BuildDraft draft)
    {
        if (draft.Id != requestedId ||
            draft.Ruleset.Id != draft.StatDistribution.RulesetId ||
            draft.ProgressionInputs.CharacterClassId != draft.StatDistribution.CharacterClassId)
        {
            throw Error(
                BuildDraftErrorCodes.SourceMismatch,
                "The persisted draft, ruleset or character class identities are incoherent.");
        }
    }

    private static bool SameDistribution(
        BuildDraftStatDistribution stored,
        BuildDraftStatDistribution recalculated) =>
        stored.SchemaVersion == recalculated.SchemaVersion &&
        stored.RulesetId == recalculated.RulesetId &&
        stored.CharacterClassId == recalculated.CharacterClassId &&
        stored.ProgressionRule == recalculated.ProgressionRule &&
        stored.EarnedPoints == recalculated.EarnedPoints &&
        stored.ResetInputs == recalculated.ResetInputs &&
        stored.ResetPoints == recalculated.ResetPoints &&
        stored.TotalDistributablePoints == recalculated.TotalDistributablePoints &&
        stored.SpentPoints == recalculated.SpentPoints &&
        stored.RemainingPoints == recalculated.RemainingPoints &&
        SameAllocations(stored.Allocations, recalculated.Allocations);

    private static bool SameAllocations(
        IReadOnlyDictionary<string, long> stored,
        IReadOnlyDictionary<string, long> recalculated) =>
        stored.Count == recalculated.Count &&
        stored.All(allocation =>
            recalculated.TryGetValue(allocation.Key, out var value) &&
            value == allocation.Value);

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

    private static BuildDraftException Error(
        string code,
        string message,
        Exception innerException) =>
        new(code, message, innerException);
}
