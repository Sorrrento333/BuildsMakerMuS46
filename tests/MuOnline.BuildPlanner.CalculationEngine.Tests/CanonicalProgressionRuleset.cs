using System.Text.Json;
using MuOnline.BuildPlanner.Domain.Progression;

namespace MuOnline.BuildPlanner.CalculationEngine.Tests;

public sealed record ProgressionReferenceCase(
    string Id,
    string ProgressionRuleId,
    string ClassId,
    string EvolutionId,
    int Level,
    string[] CompletedQuestIds,
    long ExpectedEarnedPoints,
    string? ExpectedErrorCode);

internal sealed record CanonicalProgressionRuleset(
    IReadOnlyList<CharacterProgressionDefinition> Classes,
    IReadOnlyList<ProgressionRuleDefinition> Rules,
    IReadOnlyList<ProgressionReferenceCase> ValidCases,
    IReadOnlyList<ProgressionReferenceCase> InvalidCases)
{
    public static CanonicalProgressionRuleset Load()
    {
        var repositoryRoot = FindRepositoryRoot();
        var rulesetRoot = Path.Combine(
            repositoryRoot,
            "packages",
            "rulesets",
            "mu-s4-global-reference",
            "v1");

        return new CanonicalProgressionRuleset(
            LoadFiles(
                Path.Combine(rulesetRoot, "character-classes"),
                ParseCharacterClass),
            LoadFiles(
                Path.Combine(rulesetRoot, "progression-rules"),
                ParseProgressionRule),
            LoadFiles(
                Path.Combine(rulesetRoot, "reference-cases", "progression", "valid"),
                ParseReferenceCase),
            LoadFiles(
                Path.Combine(rulesetRoot, "reference-cases", "progression", "invalid"),
                ParseReferenceCase));
    }

    private static T[] LoadFiles<T>(
        string directory,
        Func<JsonElement, T> parse) =>
        Directory.GetFiles(directory, "*.json")
            .Order(StringComparer.Ordinal)
            .Select(path =>
            {
                using var document = JsonDocument.Parse(File.ReadAllText(path));
                return parse(document.RootElement);
            })
            .ToArray();

    private static CharacterProgressionDefinition ParseCharacterClass(JsonElement element) =>
        new(
            RequiredString(element, "id"),
            RequiredString(element, "rulesetId"),
            element.GetProperty("evolutions")
                .EnumerateArray()
                .Select(evolution => RequiredString(evolution, "id"))
                .ToHashSet(StringComparer.Ordinal),
            StringArray(element, "progressionRuleRefs"));

    private static ProgressionRuleDefinition ParseProgressionRule(JsonElement element)
    {
        var levelPoints = element.GetProperty("levelPoints");
        var questBonus = element.TryGetProperty("questBonus", out var questBonusElement)
            ? new QuestPointBonusRule(
                RequiredString(questBonusElement, "questId"),
                questBonusElement.GetProperty("minimumLevel").GetInt32(),
                StringArray(questBonusElement, "eligibleEvolutionIds")
                    .ToHashSet(StringComparer.Ordinal),
                questBonusElement.GetProperty("additionalPointsPerLevel").GetInt32(),
                questBonusElement.GetProperty("retroactiveFromLevel").GetInt32())
            : null;

        return new ProgressionRuleDefinition(
            RequiredString(element, "id"),
            RequiredString(element, "version"),
            RequiredString(element, "rulesetId"),
            ParseStatus(RequiredString(element, "status")),
            StringArray(element, "appliesToClassIds").ToHashSet(StringComparer.Ordinal),
            new LevelPointRule(
                levelPoints.GetProperty("pointsPerLevel").GetInt32(),
                levelPoints.GetProperty("firstAwardedLevel").GetInt32()),
            questBonus);
    }

    private static ProgressionReferenceCase ParseReferenceCase(JsonElement element) =>
        new(
            RequiredString(element, "id"),
            RequiredString(element, "progressionRuleId"),
            RequiredString(element, "classId"),
            RequiredString(element, "evolutionId"),
            element.GetProperty("level").GetInt32(),
            StringArray(element, "completedQuestIds"),
            element.GetProperty("expectedEarnedPoints").GetInt64(),
            element.TryGetProperty("expectedErrorCode", out var errorCode)
                ? errorCode.GetString()
                : null);

    private static ProgressionRuleStatus ParseStatus(string status) =>
        status switch
        {
            "DRAFT" => ProgressionRuleStatus.Draft,
            "REVIEWED" => ProgressionRuleStatus.Reviewed,
            "PUBLISHED" => ProgressionRuleStatus.Published,
            "DEPRECATED" => ProgressionRuleStatus.Deprecated,
            _ => throw new InvalidDataException($"Unknown progression rule status '{status}'."),
        };

    private static string RequiredString(JsonElement element, string propertyName) =>
        element.GetProperty(propertyName).GetString()
        ?? throw new InvalidDataException($"'{propertyName}' cannot be null.");

    private static string[] StringArray(JsonElement element, string propertyName) =>
        element.GetProperty(propertyName)
            .EnumerateArray()
            .Select(item => item.GetString()
                ?? throw new InvalidDataException(
                    $"'{propertyName}' cannot contain null values."))
            .ToArray();

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "MUOnline.BuildPlanner.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Repository root was not found.");
    }
}
