using System.Text.Json.Serialization;

namespace MuOnline.BuildPlanner.Application.Builds;

public sealed record CharacterBuild(
    [property: JsonPropertyName("schemaVersion")] string SchemaVersion,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("ruleset")] BuildDraftVersionedReference Ruleset,
    [property: JsonPropertyName("dataset")] BuildDraftDatasetReference Dataset,
    [property: JsonPropertyName("engineVersion")] string EngineVersion,
    [property: JsonPropertyName("characterClassId")] string CharacterClassId,
    [property: JsonPropertyName("evolutionId")] string EvolutionId,
    [property: JsonPropertyName("level")] int Level,
    [property: JsonPropertyName("stats")] IReadOnlyDictionary<string, long> Stats,
    [property: JsonPropertyName("questIds")] IReadOnlyCollection<string> QuestIds,
    [property: JsonPropertyName("resetCount")] long ResetCount,
    [property: JsonPropertyName("pointsPerReset")] long PointsPerReset,
    [property: JsonPropertyName("equipment")] IReadOnlyList<BuildEquipmentEntry> Equipment)
{
    public const string CurrentSchemaVersion = "1.2.0";
    public const string PreviousSchemaVersion = "1.1.0";
}