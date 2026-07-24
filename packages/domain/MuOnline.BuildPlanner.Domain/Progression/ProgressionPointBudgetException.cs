namespace MuOnline.BuildPlanner.Domain.Progression;

public static class ProgressionPointBudgetErrorCodes
{
    public const string ClassNotFound = "class-not-found";
    public const string EvolutionDoesNotBelongToClass = "evolution-does-not-belong-to-class";
    public const string LevelOutOfRange = "level-out-of-range";
    public const string ProgressionRuleNotFound = "progression-rule-not-found";
    public const string ProgressionRuleAmbiguous = "progression-rule-ambiguous";
    public const string QuestNotSupported = "quest-not-supported";
    public const string QuestMinimumLevelNotMet = "quest-minimum-level-not-met";
    public const string QuestIneligibleEvolution = "quest-ineligible-evolution";
}

public sealed class ProgressionPointBudgetException : Exception
{
    public ProgressionPointBudgetException(string code, string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
    }

    public string Code { get; }
}
