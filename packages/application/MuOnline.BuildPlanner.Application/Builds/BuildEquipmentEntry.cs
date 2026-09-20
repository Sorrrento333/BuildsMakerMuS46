using System.Text.Json.Serialization;

namespace MuOnline.BuildPlanner.Application.Builds;

public sealed record BuildEquipmentEntry(
    [property: JsonPropertyName("itemId")] string ItemId,
    [property: JsonPropertyName("itemVersion")] string ItemVersion,
    [property: JsonPropertyName("level")] int Level);