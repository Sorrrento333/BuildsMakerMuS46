namespace MuOnline.BuildPlanner.Application.Builds;

public static class BuildDraftErrorCodes
{
    public const string NotFound = "build-draft-not-found";
    public const string SchemaUnsupported = "build-draft-schema-unsupported";
    public const string DependencyUnavailable = "build-draft-dependency-unavailable";
    public const string SourceMismatch = "build-draft-source-mismatch";
    public const string RevalidationFailed = "build-draft-revalidation-failed";
    public const string WriteConflict = "build-draft-write-conflict";
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
