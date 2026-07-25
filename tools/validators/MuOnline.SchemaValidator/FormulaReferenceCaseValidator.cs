using System.Text.Json;
using System.Text.Json.Nodes;

namespace MuOnline.SchemaValidator;

public sealed record FormulaReferenceValidationResult(
    string FormulaId,
    string FormulaVersion,
    string Status,
    IReadOnlyList<string> PositiveCaseIds,
    IReadOnlyList<string> NegativeCaseIds,
    IReadOnlyList<string> Errors,
    string FormulaPath)
{
    public bool IsValid => Errors.Count == 0;
}

public static class FormulaReferenceCaseValidator
{
    private const string RulesetId = "mu-s4-global-reference";

    public static IReadOnlyList<FormulaReferenceValidationResult> ValidateRepository(
        string repositoryRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryRoot);

        var fullRoot = Path.GetFullPath(repositoryRoot);
        var rulesetRoot = Path.Combine(
            fullRoot,
            "packages",
            "rulesets",
            RulesetId,
            "v1");
        var classes = LoadRecords(Path.Combine(rulesetRoot, "character-classes"));
        var formulas = LoadRecords(Path.Combine(rulesetRoot, "formulas"));
        var positiveCases = LoadRecords(
            Path.Combine(rulesetRoot, "reference-cases", "formulas", "valid"));
        var negativeCases = LoadRecords(
            Path.Combine(rulesetRoot, "reference-cases", "formulas", "invalid"));
        var results = new List<FormulaReferenceValidationResult>();
        var orphanCaseErrors = FindOrphanCaseErrors(
            formulas,
            positiveCases.Values.Concat(negativeCases.Values));

        foreach (var formulaEntry in formulas.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            var formula = formulaEntry.Value.Record;
            var formulaId = GetString(formula, "id", formulaEntry.Value.Path);
            var formulaVersion = GetString(formula, "version", formulaEntry.Value.Path);
            var status = GetString(formula, "status", formulaEntry.Value.Path);
            var errors = new List<string>();
            errors.AddRange(orphanCaseErrors);
            var matchingPositiveCases = SelectCases(
                positiveCases,
                formulaId,
                formulaVersion);
            var matchingNegativeCases = SelectCases(
                negativeCases,
                formulaId,
                formulaVersion);

            ValidateFormulaCatalogRelations(
                formula,
                formulaEntry.Value.Path,
                classes,
                errors);

            foreach (var referenceCase in matchingPositiveCases)
            {
                ValidateCase(
                    fullRoot,
                    formula,
                    referenceCase.Value.Record,
                    referenceCase.Value.Path,
                    classes,
                    isPositive: true,
                    errors);
            }

            foreach (var referenceCase in matchingNegativeCases)
            {
                ValidateCase(
                    fullRoot,
                    formula,
                    referenceCase.Value.Record,
                    referenceCase.Value.Path,
                    classes,
                    isPositive: false,
                    errors);
            }

            ValidatePositiveCaseCoverage(
                formula,
                formulaEntry.Value.Path,
                matchingPositiveCases,
                negativeCases,
                errors);

            results.Add(new FormulaReferenceValidationResult(
                formulaId,
                formulaVersion,
                status,
                matchingPositiveCases.Keys.Order(StringComparer.Ordinal).ToArray(),
                matchingNegativeCases.Keys.Order(StringComparer.Ordinal).ToArray(),
                errors,
                formulaEntry.Value.Path));
        }

        return results;
    }

    private static void ValidateFormulaCatalogRelations(
        JsonObject formula,
        string formulaPath,
        Dictionary<string, RecordWithPath> classes,
        List<string> errors)
    {
        if (GetString(formula, "rulesetId", formulaPath) != RulesetId)
        {
            errors.Add($"Formula '{GetString(formula, "id", formulaPath)}' uses another ruleset.");
        }

        var applicability = GetObject(formula, "applicability", formulaPath);
        var classId = GetString(applicability, "characterClassId", formulaPath);
        if (!classes.TryGetValue(classId, out var characterClass))
        {
            errors.Add($"Applicable class '{classId}' does not resolve in the ruleset.");
            return;
        }

        if (GetString(characterClass.Record, "rulesetId", characterClass.Path) != RulesetId)
        {
            errors.Add($"Applicable class '{classId}' belongs to another ruleset.");
        }

        var catalogEvolutionIds = GetEvolutionIds(characterClass.Record, characterClass.Path);
        foreach (var evolutionId in GetStringArray(
                     applicability,
                     "evolutionIds",
                     formulaPath))
        {
            if (!catalogEvolutionIds.Contains(evolutionId))
            {
                errors.Add(
                    $"Applicable evolution '{evolutionId}' does not belong to class '{classId}'.");
            }
        }

        var evidenceRefs = GetStringArray(formula, "evidenceRefs", formulaPath);
        if (evidenceRefs.Length == 0)
        {
            errors.Add($"Formula '{GetString(formula, "id", formulaPath)}' has no evidence.");
        }

        if (GetOptionalStringArray(formula, "conflictIds", formulaPath).Length == 0)
        {
            errors.Add(
                $"Formula '{GetString(formula, "id", formulaPath)}' has no traced conflict.");
        }

        foreach (var input in GetArray(formula, "inputs", formulaPath)
                     .Select(item => item?.AsObject()
                         ?? throw new InvalidDataException(
                             $"Formula '{formulaPath}' contains a null input.")))
        {
            var bounds = input["numericBounds"]?.AsObject();
            if (bounds is null ||
                GetString(bounds, "classification", formulaPath) != "FACTUAL")
            {
                continue;
            }

            foreach (var boundEvidenceRef in GetStringArray(
                         bounds,
                         "evidenceRefs",
                         formulaPath))
            {
                if (!evidenceRefs.Contains(boundEvidenceRef, StringComparer.Ordinal))
                {
                    errors.Add(
                        $"Factual bound evidence '{boundEvidenceRef}' is not traced by the formula.");
                }
            }
        }
    }

    private static void ValidateCase(
        string repositoryRoot,
        JsonObject formula,
        JsonObject referenceCase,
        string casePath,
        Dictionary<string, RecordWithPath> classes,
        bool isPositive,
        List<string> errors)
    {
        var caseId = GetString(referenceCase, "id", casePath);
        using (var caseDocument = JsonDocument.Parse(referenceCase.ToJsonString()))
        {
            if (!SchemaContractValidator.ValidateInstance(
                    repositoryRoot,
                    "formula-test-case",
                    caseDocument.RootElement))
            {
                errors.Add($"Case '{caseId}' does not match formula-test-case.schema.json.");
                return;
            }
        }

        var formulaId = GetString(formula, "id", casePath);
        var formulaVersion = GetString(formula, "version", casePath);
        var formulaRef = GetObject(referenceCase, "formulaRef", casePath);
        if (GetString(referenceCase, "rulesetId", casePath) !=
            GetString(formula, "rulesetId", casePath) ||
            GetString(formulaRef, "id", casePath) != formulaId ||
            GetString(formulaRef, "version", casePath) != formulaVersion)
        {
            errors.Add($"Case '{caseId}' does not preserve exact formula identity.");
        }

        var context = GetObject(referenceCase, "context", casePath);
        var classId = GetString(context, "characterClassId", casePath);
        var evolutionId = GetString(context, "evolutionId", casePath);
        if (!classes.TryGetValue(classId, out var characterClass))
        {
            errors.Add($"Case '{caseId}' references unknown class '{classId}'.");
        }
        else if (!GetEvolutionIds(characterClass.Record, characterClass.Path)
                     .Contains(evolutionId))
        {
            errors.Add(
                $"Case '{caseId}' evolution '{evolutionId}' does not belong to '{classId}'.");
        }

        var expectedInputIds = GetArray(formula, "inputs", casePath)
            .Select(input => GetString(
                input?.AsObject()
                ?? throw new InvalidDataException($"Formula '{formulaId}' has a null input."),
                "id",
                casePath))
            .Order(StringComparer.Ordinal)
            .ToArray();
        var actualInputIds = GetObject(referenceCase, "inputs", casePath)
            .Select(item => item.Key)
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (!expectedInputIds.SequenceEqual(actualInputIds, StringComparer.Ordinal))
        {
            errors.Add($"Case '{caseId}' does not provide the exact formula input set.");
        }

        if (!isPositive)
        {
            return;
        }

        var applicability = GetObject(formula, "applicability", casePath);
        if (classId != GetString(applicability, "characterClassId", casePath) ||
            !GetStringArray(applicability, "evolutionIds", casePath)
                .Contains(evolutionId, StringComparer.Ordinal))
        {
            errors.Add($"Positive case '{caseId}' is outside formula applicability.");
        }

        var trace = GetObject(referenceCase, "expectedTrace", casePath);
        if (GetString(trace, "rulesetId", casePath) !=
            GetString(referenceCase, "rulesetId", casePath) ||
            !JsonNode.DeepEquals(trace["formulaRef"], referenceCase["formulaRef"]) ||
            !JsonNode.DeepEquals(trace["context"], referenceCase["context"]) ||
            !JsonNode.DeepEquals(trace["inputs"], referenceCase["inputs"]))
        {
            errors.Add($"Positive case '{caseId}' trace does not preserve case identity.");
        }

        var traceDefinition = GetObject(formula, "trace", casePath);
        var expectedStepIds = GetStringArray(traceDefinition, "stepIds", casePath);
        var actualSteps = GetArray(trace, "steps", casePath)
            .Select(item => item?.AsObject()
                ?? throw new InvalidDataException($"Trace in '{caseId}' contains a null step."))
            .ToArray();
        var actualStepIds = actualSteps
            .Select(step => GetString(step, "stepId", casePath))
            .ToArray();
        if (!expectedStepIds.SequenceEqual(actualStepIds, StringComparer.Ordinal))
        {
            errors.Add($"Positive case '{caseId}' does not preserve formula trace order.");
        }

        if (!JsonNode.DeepEquals(trace["rounding"], formula["rounding"]))
        {
            errors.Add($"Positive case '{caseId}' does not preserve formula rounding.");
        }

        ValidateOutputStep(
            caseId,
            trace,
            actualSteps,
            GetString(traceDefinition, "rawOutputStepId", casePath),
            "rawOutput",
            errors);
        ValidateOutputStep(
            caseId,
            trace,
            actualSteps,
            GetString(traceDefinition, "visibleOutputStepId", casePath),
            "visibleOutput",
            errors);

        if (!GetStringArray(formula, "evidenceRefs", casePath)
                .SequenceEqual(
                    GetStringArray(trace, "evidenceRefs", casePath),
                    StringComparer.Ordinal) ||
            !GetOptionalStringArray(formula, "conflictIds", casePath)
                .SequenceEqual(
                    GetStringArray(trace, "conflictIds", casePath),
                    StringComparer.Ordinal))
        {
            errors.Add(
                $"Positive case '{caseId}' does not inherit evidence and conflicts exactly.");
        }
    }

    private static void ValidatePositiveCaseCoverage(
        JsonObject formula,
        string formulaPath,
        Dictionary<string, RecordWithPath> positiveCases,
        Dictionary<string, RecordWithPath> allNegativeCases,
        List<string> errors)
    {
        var formulaId = GetString(formula, "id", formulaPath);
        var testCaseRefs = GetArray(formula, "testCaseRefs", formulaPath)
            .Select(item => item?.AsObject()
                ?? throw new InvalidDataException(
                    $"Formula '{formulaId}' contains a null test case reference."))
            .ToArray();
        var referencedIds = testCaseRefs
            .Select(item => GetString(item, "id", formulaPath))
            .ToArray();
        var expectedIds = positiveCases.Keys.ToHashSet(StringComparer.Ordinal);

        foreach (var testCaseRef in testCaseRefs)
        {
            var caseId = GetString(testCaseRef, "id", formulaPath);
            if (allNegativeCases.ContainsKey(caseId))
            {
                errors.Add($"Negative control '{caseId}' cannot be linked by formula '{formulaId}'.");
                continue;
            }

            if (!positiveCases.TryGetValue(caseId, out var referenceCase))
            {
                errors.Add($"Formula test case reference '{caseId}' does not resolve.");
                continue;
            }

            var referencedVersion = GetString(testCaseRef, "version", formulaPath);
            var caseFormulaVersion = GetString(
                GetObject(referenceCase.Record, "formulaRef", referenceCase.Path),
                "version",
                referenceCase.Path);
            if (referencedVersion != caseFormulaVersion)
            {
                errors.Add(
                    $"Formula test case reference '{caseId}' uses version " +
                    $"'{referencedVersion}', not formula version '{caseFormulaVersion}'.");
            }
        }

        foreach (var missingCaseId in expectedIds
                     .Except(referencedIds, StringComparer.Ordinal)
                     .Order(StringComparer.Ordinal))
        {
            errors.Add($"Formula '{formulaId}' does not link positive case '{missingCaseId}'.");
        }

        foreach (var unexpectedCaseId in referencedIds
                     .Except(expectedIds, StringComparer.Ordinal)
                     .Order(StringComparer.Ordinal))
        {
            errors.Add(
                $"Formula '{formulaId}' links non-positive case '{unexpectedCaseId}'.");
        }
    }

    private static void ValidateOutputStep(
        string caseId,
        JsonObject trace,
        IEnumerable<JsonObject> steps,
        string outputStepId,
        string outputPropertyName,
        List<string> errors)
    {
        var outputStep = steps.SingleOrDefault(
            step => GetString(step, "stepId", caseId) == outputStepId);
        if (outputStep is null ||
            !JsonNode.DeepEquals(outputStep["value"], trace[outputPropertyName]))
        {
            errors.Add(
                $"Positive case '{caseId}' {outputPropertyName} does not match " +
                $"step '{outputStepId}'.");
        }
    }

    private static Dictionary<string, RecordWithPath> SelectCases(
        Dictionary<string, RecordWithPath> cases,
        string formulaId,
        string formulaVersion) =>
        cases
            .Where(item =>
            {
                var formulaRef = GetObject(
                    item.Value.Record,
                    "formulaRef",
                    item.Value.Path);
                return GetString(formulaRef, "id", item.Value.Path) == formulaId &&
                       GetString(formulaRef, "version", item.Value.Path) == formulaVersion;
            })
            .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);

    private static List<string> FindOrphanCaseErrors(
        Dictionary<string, RecordWithPath> formulas,
        IEnumerable<RecordWithPath> referenceCases)
    {
        var formulaVersions = formulas.ToDictionary(
            item => item.Key,
            item => GetString(item.Value.Record, "version", item.Value.Path),
            StringComparer.Ordinal);
        var errors = new List<string>();

        foreach (var referenceCase in referenceCases)
        {
            var caseId = GetString(referenceCase.Record, "id", referenceCase.Path);
            var formulaRef = GetObject(
                referenceCase.Record,
                "formulaRef",
                referenceCase.Path);
            var formulaId = GetString(formulaRef, "id", referenceCase.Path);
            var formulaVersion = GetString(formulaRef, "version", referenceCase.Path);

            if (!formulaVersions.TryGetValue(formulaId, out var resolvedVersion) ||
                resolvedVersion != formulaVersion)
            {
                errors.Add(
                    $"Case '{caseId}' references unresolved formula " +
                    $"'{formulaId}' version '{formulaVersion}'.");
            }
        }

        return errors;
    }

    private static Dictionary<string, RecordWithPath> LoadRecords(string directory)
    {
        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Ruleset directory was not found: {directory}");
        }

        return Directory.GetFiles(directory, "*.json")
            .Select(path => new RecordWithPath(ParseObject(path), path))
            .ToDictionary(
                item => GetString(item.Record, "id", item.Path),
                StringComparer.Ordinal);
    }

    private static JsonObject ParseObject(string path) =>
        JsonNode.Parse(File.ReadAllText(path))?.AsObject()
        ?? throw new InvalidDataException($"JSON object was expected: {path}");

    private static JsonObject GetObject(
        JsonObject instance,
        string propertyName,
        string context) =>
        instance[propertyName]?.AsObject()
        ?? throw new InvalidDataException(
            $"'{propertyName}' is required in formula data '{context}'.");

    private static JsonArray GetArray(
        JsonObject instance,
        string propertyName,
        string context) =>
        instance[propertyName]?.AsArray()
        ?? throw new InvalidDataException(
            $"'{propertyName}' is required in formula data '{context}'.");

    private static string GetString(
        JsonObject instance,
        string propertyName,
        string context) =>
        instance[propertyName]?.GetValue<string>()
        ?? throw new InvalidDataException(
            $"'{propertyName}' is required in formula data '{context}'.");

    private static string[] GetStringArray(
        JsonObject instance,
        string propertyName,
        string context) =>
        GetArray(instance, propertyName, context)
            .Select(item => item?.GetValue<string>()
                ?? throw new InvalidDataException(
                    $"'{propertyName}' contains a null value in formula data '{context}'."))
            .ToArray();

    private static string[] GetOptionalStringArray(
        JsonObject instance,
        string propertyName,
        string context) =>
        instance[propertyName] is null
            ? []
            : GetStringArray(instance, propertyName, context);

    private static HashSet<string> GetEvolutionIds(JsonObject characterClass, string context) =>
        GetArray(characterClass, "evolutions", context)
            .Select(item => GetString(
                item?.AsObject()
                ?? throw new InvalidDataException(
                    $"Character class '{context}' contains a null evolution."),
                "id",
                context))
            .ToHashSet(StringComparer.Ordinal);

    private sealed record RecordWithPath(JsonObject Record, string Path);
}
