using MuOnline.BuildPlanner.Domain.Skills;

namespace MuOnline.BuildPlanner.Application.Skills;

public sealed record LearnSkillRequest(
    string EvolutionId,
    long FinalLevel,
    string SkillId);

public sealed record LearnSkillResult(
    string SkillId,
    string DisplayName,
    string RulesetId,
    SkillKind Kind,
    long RequiredLevel,
    IReadOnlySet<string> AllowedEvolutionIds);

public sealed class LearnSkillUseCase
{
    private readonly SkillCatalog _catalog;

    public LearnSkillUseCase(SkillCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        _catalog = catalog;
    }

    public LearnSkillResult Execute(LearnSkillRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.EvolutionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.SkillId);

        var matches = _catalog.Skills
            .Where(skill => skill.Id == request.SkillId)
            .ToArray();
        if (matches.Length != 1)
        {
            throw Error(
                SkillLearnErrorCodes.SkillNotFound,
                $"Skill '{request.SkillId}' does not resolve to exactly one published definition.");
        }

        var skill = matches[0];
        if (!skill.AllowedEvolutionIds.Contains(request.EvolutionId))
        {
            throw Error(
                SkillLearnErrorCodes.EvolutionNotAllowed,
                $"Evolution '{request.EvolutionId}' is not allowed to learn '{skill.DisplayName}'.");
        }

        if (request.FinalLevel < skill.RequiredLevel)
        {
            throw Error(
                SkillLearnErrorCodes.RequirementsNotMet,
                $"Skill '{skill.DisplayName}' requires final level {skill.RequiredLevel}.");
        }

        return new LearnSkillResult(
            skill.Id,
            skill.DisplayName,
            skill.RulesetId,
            skill.Kind,
            skill.RequiredLevel,
            skill.AllowedEvolutionIds);
    }

    private static SkillLearnException Error(string code, string message) =>
        new(code, message);
}
