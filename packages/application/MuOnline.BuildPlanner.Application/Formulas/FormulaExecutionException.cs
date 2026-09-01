namespace MuOnline.BuildPlanner.Application.Formulas;

public static class FormulaExecutionErrorCodes
{
    public const string FormulaNotExecutable = "formula-not-executable";
}

public sealed class FormulaExecutionException : Exception
{
    public FormulaExecutionException(string code, string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
    }

    public string Code { get; }
}
