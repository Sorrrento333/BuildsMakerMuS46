using System.Text.Json.Nodes;

namespace MuOnline.SchemaValidator;

public sealed record ProgressionReferenceCaseValidationResult(
    string CaseId,
    bool ExpectedValidity,
    bool ActualValidity,
    int ExpectedEarnedPoints,
    int? ActualEarnedPoints,
    string? ExpectedErrorCode,
    string? ActualErrorCode,
    string CasePath)
{
    public bool MatchesExpectation =>
        ExpectedValidity == ActualValidity &&
        ExpectedEarnedPoints == ActualEarnedPoints &&
        (ExpectedValidity || ExpectedErrorCode == ActualErrorCode);
}

public sealed record ProgressionRuleReferenceValidationResult(
    string RuleId,
    string Status,
    IReadOnlyList<string> TestCaseRefs,
    IReadOnlyList<string> Errors,
    string RulePath)
{
    public bool IsValid => Errors.Count == 0;
}

public static class ProgressionReferenceCaseValidator
{
    private const string RulesetId = "mu-s4-global-reference";

    public static IReadOnlyList<ProgressionReferenceCaseValidationResult> ValidateRepository(
        string repositoryRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryRoot);

        var rulesetRoot = Path.Combine(
            Path.GetFullPath(repositoryRoot),
            "packages",
            "rulesets",
            RulesetId,
            "v1");
        var classes = LoadRecords(Path.Combine(rulesetRoot, "character-classes"));
        var rules = LoadRecords(Path.Combine(rulesetRoot, "progression-rules"));
        var casesRoot = Path.Combine(rulesetRoot, "reference-cases", "progression");
        var results = new List<ProgressionReferenceCaseValidationResult>();

        foreach (var fixtureKind in new[] { "valid", "invalid" })
        {
            var expectedValidity = fixtureKind == "valid";
            var caseDirectory = Path.Combine(casesRoot, fixtureKind);

            if (!Directory.Exists(caseDirectory))
            {
                throw new DirectoryNotFoundException(
                    $"Progression reference case directory was not found: {caseDirectory}");
            }

            foreach (var casePath in Directory.GetFiles(caseDirectory, "*.json")
                         .Order(StringComparer.Ordinal))
            {
                results.Add(ValidateCase(
                    ParseObject(casePath),
                    casePath,
                    expectedValidity,
                    classes,
                    rules));
            }
        }

        return results;
    }

    public static IReadOnlyList<ProgressionRuleReferenceValidationResult>
        ValidateRuleReferences(string repositoryRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryRoot);

        var rulesetRoot = Path.Combine(
            Path.GetFullPath(repositoryRoot),
            "packages",
            "rulesets",
            RulesetId,
            "v1");
        var ruleDirectory = Path.Combine(rulesetRoot, "progression-rules");
        var validCaseDirectory = Path.Combine(
            rulesetRoot,
            "reference-cases",
            "progression",
            "valid");
        var invalidCaseDirectory = Path.Combine(
            rulesetRoot,
            "reference-cases",
            "progression",
            "invalid");
        var validCases = LoadRecords(validCaseDirectory);
        var invalidCaseIds = LoadRecords(invalidCaseDirectory).Keys.ToHashSet(
            StringComparer.Ordinal);
        var results = new List<ProgressionRuleReferenceValidationResult>();

        if (!Directory.Exists(ruleDirectory))
        {
            throw new DirectoryNotFoundException(
                $"Ruleset directory was not found: {ruleDirectory}");
        }

        foreach (var rulePath in Directory.GetFiles(ruleDirectory, "*.json")
                     .Order(StringComparer.Ordinal))
        {
            var rule = ParseObject(rulePath);
            var ruleId = GetString(rule, "id", rulePath);
            var status = GetString(rule, "status", rulePath);
            var ruleRulesetId = GetString(rule, "rulesetId", rulePath);
            var testCaseRefs = GetStringArray(rule, "testCaseRefs", rulePath);
            var errors = new List<string>();

            foreach (var testCaseRef in testCaseRefs)
            {
                if (invalidCaseIds.Contains(testCaseRef))
                {
                    errors.Add(
                        $"'{testCaseRef}' is a negative control and cannot be published.");
                    continue;
                }

                if (!validCases.TryGetValue(testCaseRef, out var referenceCase))
                {
                    errors.Add($"'{testCaseRef}' does not resolve to a valid reference case.");
                    continue;
                }

                var caseRulesetId = GetString(
                    referenceCase,
                    "rulesetId",
                    testCaseRef);
                if (caseRulesetId != ruleRulesetId)
                {
                    errors.Add(
                        $"'{testCaseRef}' belongs to ruleset '{caseRulesetId}', " +
                        $"not '{ruleRulesetId}'.");
                }

                var caseRuleId = GetString(
                    referenceCase,
                    "progressionRuleId",
                    testCaseRef);
                if (caseRuleId != ruleId)
                {
                    errors.Add(
                        $"'{testCaseRef}' belongs to rule '{caseRuleId}', not '{ruleId}'.");
                }
            }

            if (status == "PUBLISHED")
            {
                var expectedCaseIds = validCases
                    .Where(item =>
                        GetString(item.Value, "rulesetId", item.Key) == ruleRulesetId &&
                        GetString(item.Value, "progressionRuleId", item.Key) == ruleId)
                    .Select(item => item.Key)
                    .ToHashSet(StringComparer.Ordinal);
                var missingCaseIds = expectedCaseIds
                    .Except(testCaseRefs, StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal);

                foreach (var missingCaseId in missingCaseIds)
                {
                    errors.Add(
                        $"Published rule '{ruleId}' does not reference '{missingCaseId}'.");
                }
            }

            results.Add(new ProgressionRuleReferenceValidationResult(
                ruleId,
                status,
                testCaseRefs,
                errors,
                rulePath));
        }

        return results;
    }

    private static ProgressionReferenceCaseValidationResult ValidateCase(
        JsonObject referenceCase,
        string casePath,
        bool expectedValidity,
        Dictionary<string, JsonObject> classes,
        Dictionary<string, JsonObject> rules)
    {
        var caseId = GetString(referenceCase, "id", casePath);
        var expectedPoints = GetInt32(referenceCase, "expectedEarnedPoints", casePath);
        var expectedErrorCode = referenceCase["expectedErrorCode"]?.GetValue<string>();
        string? errorCode = null;

        if (GetString(referenceCase, "schemaVersion", casePath) != "1.0.0" ||
            GetString(referenceCase, "rulesetId", casePath) != RulesetId)
        {
            errorCode = "reference-case-version-mismatch";
        }

        var ruleId = GetString(referenceCase, "progressionRuleId", casePath);
        var classId = GetString(referenceCase, "classId", casePath);
        var evolutionId = GetString(referenceCase, "evolutionId", casePath);
        var level = GetInt32(referenceCase, "level", casePath);
        var completedQuestIds = GetStringArray(referenceCase, "completedQuestIds", casePath);

        if (!rules.TryGetValue(ruleId, out var rule))
        {
            errorCode ??= "progression-rule-not-found";
        }

        if (!classes.TryGetValue(classId, out var characterClass))
        {
            errorCode ??= "class-not-found";
        }

        if (rule is not null &&
            !GetStringArray(rule, "appliesToClassIds", ruleId).Contains(
                classId,
                StringComparer.Ordinal))
        {
            errorCode ??= "rule-does-not-apply-to-class";
        }

        var evolution = characterClass?["evolutions"]?
            .AsArray()
            .Select(item => item?.AsObject())
            .SingleOrDefault(item => item?["id"]?.GetValue<string>() == evolutionId);
        if (evolution is null)
        {
            errorCode ??= "evolution-does-not-belong-to-class";
        }

        if (level < 1)
        {
            errorCode ??= "level-out-of-range";
        }

        int? actualPoints = null;
        if (rule is not null)
        {
            var levelPoints = rule["levelPoints"]?.AsObject()
                ?? throw new InvalidDataException(
                    $"Progression rule '{ruleId}' does not define levelPoints.");
            var pointsPerLevel = GetInt32(levelPoints, "pointsPerLevel", ruleId);
            var firstAwardedLevel = GetInt32(levelPoints, "firstAwardedLevel", ruleId);
            actualPoints = Math.Max(0, level - firstAwardedLevel + 1) * pointsPerLevel;

            if (completedQuestIds.Length > 0)
            {
                var questBonus = rule["questBonus"]?.AsObject();
                if (questBonus is null)
                {
                    errorCode ??= "quest-not-supported";
                }
                else
                {
                    var questId = GetString(questBonus, "questId", ruleId);
                    if (completedQuestIds.Length != 1 ||
                        !completedQuestIds.Contains(questId, StringComparer.Ordinal))
                    {
                        errorCode ??= "quest-not-supported";
                    }
                    else if (level < GetInt32(questBonus, "minimumLevel", ruleId))
                    {
                        errorCode ??= "quest-minimum-level-not-met";
                    }
                    else if (!GetStringArray(
                                 questBonus,
                                 "eligibleEvolutionIds",
                                 ruleId)
                             .Contains(evolutionId, StringComparer.Ordinal))
                    {
                        errorCode ??= "quest-ineligible-evolution";
                    }
                    else
                    {
                        var retroactiveFromLevel = GetInt32(
                            questBonus,
                            "retroactiveFromLevel",
                            ruleId);
                        var additionalPointsPerLevel = GetInt32(
                            questBonus,
                            "additionalPointsPerLevel",
                            ruleId);
                        actualPoints +=
                            Math.Max(0, level - retroactiveFromLevel + 1) *
                            additionalPointsPerLevel;
                    }
                }
            }
        }

        if (errorCode is null && actualPoints != expectedPoints)
        {
            errorCode = "earned-points-mismatch";
        }

        var actualValidity = errorCode is null;
        return new ProgressionReferenceCaseValidationResult(
            caseId,
            expectedValidity,
            actualValidity,
            expectedPoints,
            actualPoints,
            expectedErrorCode,
            errorCode,
            casePath);
    }

    private static Dictionary<string, JsonObject> LoadRecords(string directory)
    {
        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Ruleset directory was not found: {directory}");
        }

        return Directory.GetFiles(directory, "*.json")
            .Select(path => (Path: path, Record: ParseObject(path)))
            .ToDictionary(
                item => GetString(item.Record, "id", item.Path),
                item => item.Record,
                StringComparer.Ordinal);
    }

    private static JsonObject ParseObject(string path) =>
        JsonNode.Parse(File.ReadAllText(path))?.AsObject()
        ?? throw new InvalidDataException($"JSON object was expected: {path}");

    private static string GetString(JsonObject instance, string propertyName, string context) =>
        instance[propertyName]?.GetValue<string>()
        ?? throw new InvalidDataException(
            $"'{propertyName}' is required in progression data '{context}'.");

    private static int GetInt32(JsonObject instance, string propertyName, string context) =>
        instance[propertyName]?.GetValue<int>()
        ?? throw new InvalidDataException(
            $"'{propertyName}' is required in progression data '{context}'.");

    private static string[] GetStringArray(
        JsonObject instance,
        string propertyName,
        string context) =>
        instance[propertyName]?.AsArray()
            .Select(item => item?.GetValue<string>()
                ?? throw new InvalidDataException(
                    $"'{propertyName}' contains a null value in progression data '{context}'."))
            .ToArray()
        ?? throw new InvalidDataException(
            $"'{propertyName}' is required in progression data '{context}'.");
}
