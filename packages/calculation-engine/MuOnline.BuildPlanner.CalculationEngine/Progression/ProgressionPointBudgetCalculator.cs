using MuOnline.BuildPlanner.Domain.Progression;

namespace MuOnline.BuildPlanner.CalculationEngine.Progression;

public sealed class ProgressionPointBudgetCalculator
{
    private readonly Dictionary<string, CharacterProgressionDefinition> _classes;
    private readonly Dictionary<string, ProgressionRuleDefinition> _rules;

    public ProgressionPointBudgetCalculator(
        IEnumerable<CharacterProgressionDefinition> classes,
        IEnumerable<ProgressionRuleDefinition> rules)
    {
        ArgumentNullException.ThrowIfNull(classes);
        ArgumentNullException.ThrowIfNull(rules);

        _classes = classes.ToDictionary(definition => definition.Id, StringComparer.Ordinal);
        _rules = rules.ToDictionary(definition => definition.Id, StringComparer.Ordinal);
    }

    public ProgressionPointBudgetResult Calculate(ProgressionPointBudgetRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_classes.TryGetValue(request.ClassId, out var characterClass))
        {
            throw Error(
                ProgressionPointBudgetErrorCodes.ClassNotFound,
                $"Character class '{request.ClassId}' was not found.");
        }

        if (!characterClass.EvolutionIds.Contains(request.EvolutionId))
        {
            throw Error(
                ProgressionPointBudgetErrorCodes.EvolutionDoesNotBelongToClass,
                $"Evolution '{request.EvolutionId}' does not belong to class '{request.ClassId}'.");
        }

        if (request.Level < 1)
        {
            throw Error(
                ProgressionPointBudgetErrorCodes.LevelOutOfRange,
                $"Level '{request.Level}' must be at least 1.");
        }

        var applicableRules = characterClass.ProgressionRuleRefs
            .Where(_rules.ContainsKey)
            .Select(ruleId => _rules[ruleId])
            .Where(rule =>
                rule.Status == ProgressionRuleStatus.Published &&
                rule.RulesetId == characterClass.RulesetId &&
                rule.AppliesToClassIds.Contains(characterClass.Id))
            .ToArray();

        if (applicableRules.Length == 0)
        {
            throw Error(
                ProgressionPointBudgetErrorCodes.ProgressionRuleNotFound,
                $"No published progression rule applies to class '{request.ClassId}'.");
        }

        if (applicableRules.Length > 1)
        {
            throw Error(
                ProgressionPointBudgetErrorCodes.ProgressionRuleAmbiguous,
                $"More than one published progression rule applies to class '{request.ClassId}'.");
        }

        var rule = applicableRules[0];
        var levelCount = Math.Max(0, request.Level - rule.LevelPoints.FirstAwardedLevel + 1);
        var levelPoints = checked((long)levelCount * rule.LevelPoints.PointsPerLevel);
        var contributions = new List<ProgressionPointContribution>
        {
            new(
                ProgressionPointContributionKind.Level,
                rule.Id,
                levelCount,
                rule.LevelPoints.PointsPerLevel,
                levelPoints),
        };

        var completedQuestIds = request.CompletedQuestIds.ToHashSet(StringComparer.Ordinal);
        if (completedQuestIds.Count == 0)
        {
            return CreateResult(characterClass, rule, levelPoints, contributions);
        }

        var questBonus = rule.QuestBonus;
        if (questBonus is null ||
            completedQuestIds.Count != 1 ||
            !completedQuestIds.Contains(questBonus.QuestId))
        {
            throw Error(
                ProgressionPointBudgetErrorCodes.QuestNotSupported,
                $"The completed quests are not supported by progression rule '{rule.Id}'.");
        }

        if (request.Level < questBonus.MinimumLevel)
        {
            throw Error(
                ProgressionPointBudgetErrorCodes.QuestMinimumLevelNotMet,
                $"Quest '{questBonus.QuestId}' requires level {questBonus.MinimumLevel}.");
        }

        if (!questBonus.EligibleEvolutionIds.Contains(request.EvolutionId))
        {
            throw Error(
                ProgressionPointBudgetErrorCodes.QuestIneligibleEvolution,
                $"Evolution '{request.EvolutionId}' is not eligible for quest '{questBonus.QuestId}'.");
        }

        var questLevelCount = Math.Max(
            0,
            request.Level - questBonus.RetroactiveFromLevel + 1);
        var questPoints = checked(
            (long)questLevelCount * questBonus.AdditionalPointsPerLevel);
        contributions.Add(new ProgressionPointContribution(
            ProgressionPointContributionKind.QuestBonus,
            questBonus.QuestId,
            questLevelCount,
            questBonus.AdditionalPointsPerLevel,
            questPoints));

        return CreateResult(
            characterClass,
            rule,
            checked(levelPoints + questPoints),
            contributions);
    }

    private static ProgressionPointBudgetResult CreateResult(
        CharacterProgressionDefinition characterClass,
        ProgressionRuleDefinition rule,
        long earnedPoints,
        IReadOnlyList<ProgressionPointContribution> contributions) =>
        new(
            rule.RulesetId,
            characterClass.Id,
            rule.Id,
            rule.Version,
            earnedPoints,
            contributions);

    private static ProgressionPointBudgetException Error(string code, string message) =>
        new(code, message);
}
