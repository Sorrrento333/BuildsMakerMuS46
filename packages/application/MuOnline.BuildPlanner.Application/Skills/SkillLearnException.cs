namespace MuOnline.BuildPlanner.Application.Skills;

public static class SkillLearnErrorCodes
{
    public const string SkillNotFound = "skill-learn-skill-not-found";
    public const string EvolutionNotAllowed = "skill-learn-evolution-not-allowed";
    public const string RequirementsNotMet = "skill-learn-requirements-not-met";
}

public sealed class SkillLearnException : Exception
{
    public SkillLearnException(string code, string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
    }

    public SkillLearnException(string code, string message, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
    }

    public string Code { get; }
}
