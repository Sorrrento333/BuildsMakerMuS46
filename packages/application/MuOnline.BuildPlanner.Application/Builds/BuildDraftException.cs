namespace MuOnline.BuildPlanner.Application.Builds;

public static class BuildDraftErrorCodes
{
    public const string NotFound = "build-draft-not-found";
    public const string SchemaUnsupported = "build-draft-schema-unsupported";
    public const string DependencyUnavailable = "build-draft-dependency-unavailable";
    public const string SourceMismatch = "build-draft-source-mismatch";
    public const string RevalidationFailed = "build-draft-revalidation-failed";
    public const string WriteConflict = "build-draft-write-conflict";
    public const string EquipmentItemNotFound = "build-draft-equipment-item-not-found";
    public const string EquipmentVersionMismatch = "build-draft-equipment-version-mismatch";
    public const string EquipmentClassNotAllowed = "build-draft-equipment-class-not-allowed";
    public const string EquipmentLevelOutOfRange = "build-draft-equipment-level-out-of-range";
    public const string EquipmentDuplicate = "build-draft-equipment-duplicate";
    public const string EquipmentRequirementsNotMet = "build-draft-equipment-requirements-not-met";
}

public sealed class BuildDraftException : Exception
{
    public BuildDraftException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    public BuildDraftException(string code, string message, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }

    public string Code { get; }
}
