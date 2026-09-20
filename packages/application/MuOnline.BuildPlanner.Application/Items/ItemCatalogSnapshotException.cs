namespace MuOnline.BuildPlanner.Application.Items;

public static class ItemCatalogSnapshotErrorCodes
{
    public const string SnapshotNotFound = "item-snapshot-not-found";
    public const string SnapshotInvalid = "item-snapshot-invalid";
    public const string DuplicateId = "item-snapshot-duplicate-id";
    public const string RulesetMismatch = "item-snapshot-ruleset-mismatch";
    public const string ItemNotPublished = "item-snapshot-not-published";
}

public sealed class ItemCatalogSnapshotException : Exception
{
    public ItemCatalogSnapshotException(string code, string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
    }

    public ItemCatalogSnapshotException(string code, string message, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
    }

    public string Code { get; }
}
