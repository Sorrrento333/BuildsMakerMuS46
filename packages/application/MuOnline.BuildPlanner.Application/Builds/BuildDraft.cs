using System.Text.Json.Serialization;
using MuOnline.BuildPlanner.Application.Progression;

namespace MuOnline.BuildPlanner.Application.Builds;

public sealed record BuildDraft(
    [property: JsonPropertyName("schemaVersion")] string SchemaVersion,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("ruleset")] BuildDraftVersionedReference Ruleset,
    [property: JsonPropertyName("dataset")] BuildDraftDatasetReference Dataset,
    [property: JsonPropertyName("engineVersion")] string EngineVersion,
    [property: JsonPropertyName("progressionInputs")] BuildDraftProgressionInputs ProgressionInputs,
    [property: JsonPropertyName("resetInputs")] BuildDraftResetInputs ResetInputs,
    [property: JsonPropertyName("statDistribution")] BuildDraftStatDistribution StatDistribution)
{
    public const string CurrentSchemaVersion = "1.1.0";
    public const string PreviousSchemaVersion = "1.0.0";
}

public sealed record BuildDraftVersionedReference(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("version")] string Version);

public sealed record BuildDraftDatasetReference(
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("hash")] string Hash);

public sealed record BuildDraftProgressionInputs(
    [property: JsonPropertyName("characterClassId")] string CharacterClassId,
    [property: JsonPropertyName("evolutionId")] string EvolutionId,
    [property: JsonPropertyName("level")] int Level,
    [property: JsonPropertyName("completedQuestIds")] IReadOnlyCollection<string> CompletedQuestIds);

public sealed record BuildDraftResetInputs(
    [property: JsonPropertyName("resetCount")] long ResetCount,
    [property: JsonPropertyName("pointsPerReset")] long PointsPerReset);

public sealed record BuildDraftStatDistribution(
    [property: JsonPropertyName("schemaVersion")] string SchemaVersion,
    [property: JsonPropertyName("rulesetId")] string RulesetId,
    [property: JsonPropertyName("characterClassId")] string CharacterClassId,
    [property: JsonPropertyName("progressionRule")] BuildDraftVersionedReference ProgressionRule,
    [property: JsonPropertyName("earnedPoints")] long EarnedPoints,
    [property: JsonPropertyName("resetInputs")] BuildDraftResetInputs ResetInputs,
    [property: JsonPropertyName("resetPoints")] long ResetPoints,
    [property: JsonPropertyName("totalDistributablePoints")] long TotalDistributablePoints,
    [property: JsonPropertyName("allocations")] IReadOnlyDictionary<string, long> Allocations,
    [property: JsonPropertyName("spentPoints")] long SpentPoints,
    [property: JsonPropertyName("remainingPoints")] long RemainingPoints)
{
    public const string CurrentSchemaVersion = "1.1.0";
    public const string PreviousSchemaVersion = "1.0.0";
}

public sealed record BuildDraftRuntimeContext(
    ProgressionRulesetCatalog Catalog,
    BuildDraftVersionedReference Ruleset,
    BuildDraftDatasetReference Dataset,
    string EngineVersion);
