namespace MuOnline.BuildPlanner.Application.Skills;

public static class SkillCatalogSnapshotErrorCodes
{
    public const string SnapshotNotFound = "skill-snapshot-not-found";
    public const string SnapshotInvalid = "skill-snapshot-invalid";
    public const string DuplicateId = "skill-snapshot-duplicate-id";
    public const string RulesetMismatch = "skill-snapshot-ruleset-mismatch";
    public const string SkillNotPublished = "skill-snapshot-skill-not-published";
}

public sealed class SkillCatalogSnapshotException : Exception
{
    public SkillCatalogSnapshotException(string code, string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
    }

    public SkillCatalogSnapshotException(string code, string message, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
    }

    public string Code { get; }
}
