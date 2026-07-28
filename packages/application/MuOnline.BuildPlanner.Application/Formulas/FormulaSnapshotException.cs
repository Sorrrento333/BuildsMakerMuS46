namespace MuOnline.BuildPlanner.Application.Formulas;

public static class FormulaSnapshotErrorCodes
{
    public const string SnapshotNotFound = "formula-snapshot-not-found";
    public const string SnapshotInvalid = "formula-snapshot-invalid";
    public const string DuplicateReference = "formula-snapshot-duplicate-reference";
    public const string RulesetMismatch = "formula-snapshot-ruleset-mismatch";
    public const string FormulaNotPublished = "formula-snapshot-formula-not-published";
    public const string ReferenceIncoherent = "formula-snapshot-reference-incoherent";
}

public sealed class FormulaSnapshotException : Exception
{
    public FormulaSnapshotException(string code, string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
    }

    public FormulaSnapshotException(
        string code,
        string message,
        Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
    }

    public string Code { get; }
}
