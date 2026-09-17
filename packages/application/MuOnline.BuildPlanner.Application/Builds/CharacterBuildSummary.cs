namespace MuOnline.BuildPlanner.Application.Builds;

public sealed record CharacterBuildSummary(
    string Id,
    string SchemaVersion,
    string CharacterClassId,
    string EvolutionId,
    int Level,
    long ResetCount,
    long PointsPerReset,
    string DatasetVersion);