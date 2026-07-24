namespace MuOnline.BuildPlanner.Application.Progression;

public static class ProgressionSnapshotErrorCodes
{
    public const string SnapshotNotFound = "snapshot-not-found";
    public const string SnapshotInvalid = "snapshot-invalid";
    public const string DuplicateId = "snapshot-duplicate-id";
    public const string RulesetMismatch = "snapshot-ruleset-mismatch";
    public const string RuleNotPublished = "snapshot-rule-not-published";
    public const string ReferenceIncoherent = "snapshot-reference-incoherent";
}

public sealed class ProgressionSnapshotException : Exception
{
    public ProgressionSnapshotException(string code, string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
    }

    public ProgressionSnapshotException(string code, string message, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
    }

    public string Code { get; }
}
