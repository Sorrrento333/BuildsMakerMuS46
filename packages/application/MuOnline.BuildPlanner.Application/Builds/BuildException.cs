namespace MuOnline.BuildPlanner.Application.Builds;

public static class BuildErrorCodes
{
    public const string NotFound = "build-not-found";
    public const string SchemaUnsupported = "build-schema-unsupported";
    public const string DependencyUnavailable = "build-dependency-unavailable";
    public const string SourceMismatch = "build-source-mismatch";
    public const string RevalidationFailed = "build-revalidation-failed";
    public const string WriteConflict = "build-write-conflict";
    public const string EquipmentItemNotFound = "build-equipment-item-not-found";
    public const string EquipmentVersionMismatch = "build-equipment-version-mismatch";
    public const string EquipmentClassNotAllowed = "build-equipment-class-not-allowed";
    public const string EquipmentLevelOutOfRange = "build-equipment-level-out-of-range";
    public const string EquipmentDuplicate = "build-equipment-duplicate";
    public const string EquipmentRequirementsNotMet = "build-equipment-requirements-not-met";
}

public sealed class BuildException : Exception
{
    public BuildException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    public BuildException(string code, string message, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }

    public string Code { get; }
}