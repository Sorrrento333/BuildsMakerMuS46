namespace MuOnline.BuildPlanner.Domain.Formulas;

public static class FormulaCalculationErrorCodes
{
    public const string FormulaNotPublished = "formula-not-published";
    public const string FormulaNotApplicable = "formula-not-applicable";
    public const string InputMissing = "formula-input-missing";
    public const string InputNotDeclared = "formula-input-not-declared";
    public const string ArithmeticOverflow = "formula-arithmetic-overflow";
    public const string ProgramNotSupported = "formula-program-not-supported";
    public const string ProgramInvalid = "formula-program-invalid";
}

public sealed class FormulaCalculationException : Exception
{
    public FormulaCalculationException(string code, string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
    }

    public string Code { get; }
}
