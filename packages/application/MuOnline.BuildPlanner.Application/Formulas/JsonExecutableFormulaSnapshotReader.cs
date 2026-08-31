using System.Text.Json;
using MuOnline.BuildPlanner.Domain.Formulas;

namespace MuOnline.BuildPlanner.Application.Formulas;

public sealed class JsonExecutableFormulaSnapshotReader
    : IExecutableFormulaSnapshotReader
{
    private const string IntegerExecutableSchemaVersion = "2.0.0";
    private const string DecimalExecutableSchemaVersion = "2.1.0";
    private const string HistoricalSchemaVersion = "1.1.0";

    public ExecutableFormulaCatalog Read(string snapshotRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(snapshotRoot);

        var characterClassDirectory = Path.Combine(snapshotRoot, "character-classes");
        var formulaDirectory = Path.Combine(snapshotRoot, "formulas");
        EnsureDirectoryExists(characterClassDirectory);
        EnsureDirectoryExists(formulaDirectory);

        try
        {
            var characterClasses = LoadFiles(
                characterClassDirectory,
                ParseCharacterClass);
            var formulaRecords = LoadFiles(formulaDirectory, ParseFormulaRecord);

            ValidateUniqueClasses(characterClasses);
            ValidateUniqueFormulaReferences(formulaRecords);

            var rulesetId = ResolveRulesetId(characterClasses, formulaRecords);
            var formulas = formulaRecords
                .Where(record => record.Definition is not null)
                .Select(record => record.Definition!)
                .ToArray();
            if (formulas.Length == 0)
            {
                throw Invalid("The snapshot contains no executable formulas.");
            }

            ValidateExecutableFormulas(characterClasses, formulas);
            return new ExecutableFormulaCatalog(rulesetId, formulas);
        }
        catch (FormulaSnapshotException)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is JsonException or
            InvalidOperationException or
            KeyNotFoundException or
            FormatException or
            OverflowException or
            IOException or
            UnauthorizedAccessException or
            ArgumentException)
        {
            throw new FormulaSnapshotException(
                FormulaSnapshotErrorCodes.SnapshotInvalid,
                $"Formula snapshot '{snapshotRoot}' could not be materialized.",
                exception);
        }
    }

    private static FormulaRecord ParseFormulaRecord(JsonElement element)
    {
        var schemaVersion = RequiredString(element, "schemaVersion");
        var reference = new FormulaReference(
            RequiredString(element, "id"),
            RequiredString(element, "version"));
        var rulesetId = RequiredString(element, "rulesetId");

        return schemaVersion switch
        {
            HistoricalSchemaVersion => new FormulaRecord(
                reference,
                rulesetId,
                null),
            IntegerExecutableSchemaVersion or DecimalExecutableSchemaVersion =>
                new FormulaRecord(
                reference,
                rulesetId,
                ParseExecutableFormula(
                    element,
                    reference,
                    rulesetId,
                    schemaVersion)),
            _ => throw Invalid(
                $"Formula '{reference.Id}' version '{reference.Version}' uses " +
                $"unsupported schema version '{schemaVersion}'."),
        };
    }

    private static FormulaDefinition ParseExecutableFormula(
        JsonElement element,
        FormulaReference reference,
        string rulesetId,
        string schemaVersion)
    {
        var applicabilityElement = element.GetProperty("applicability");
        var strategyElement = element.GetProperty("strategy");
        if (RequiredString(strategyElement, "kind") != "PROGRAM")
        {
            throw Invalid("Executable formulas require strategy kind 'PROGRAM'.");
        }

        var executionModel = RequiredString(strategyElement, "executionModel");
        var expectedExecutionModel = schemaVersion switch
        {
            IntegerExecutableSchemaVersion => CheckedIntegerFormulaProgram.ModelId,
            DecimalExecutableSchemaVersion => CheckedDecimalFormulaProgram.ModelId,
            _ => throw Invalid($"Unsupported executable schema version '{schemaVersion}'."),
        };
        if (executionModel != expectedExecutionModel)
        {
            throw Invalid(
                $"Schema version '{schemaVersion}' requires execution model " +
                $"'{expectedExecutionModel}'.");
        }

        var dependencies = strategyElement.TryGetProperty(
                "dependencyFormulaRefs",
                out var dependencyElements)
            ? dependencyElements
                .EnumerateArray()
                .Select(ParseReference)
                .ToArray()
            : [];

        var inputs = element.GetProperty("inputs")
            .EnumerateArray()
            .Select(ParseInput)
            .ToArray();
        if (executionModel == CheckedIntegerFormulaProgram.ModelId &&
            inputs.Any(input =>
                input.NumericType == FormulaNumericType.ExactBase10))
        {
            throw Invalid(
                "CHECKED_INT64_V1 cannot consume DECIMAL inputs.");
        }
        var inputIds = inputs
            .Select(input => input.Id)
            .ToHashSet(StringComparer.Ordinal);
        var steps = ParseAndValidateSteps(
            strategyElement.GetProperty("steps"),
            inputIds,
            executionModel);
        var rounding = ParseRounding(element.GetProperty("rounding"));
        var trace = ParseTrace(element.GetProperty("trace"));
        ValidateProgramTraceRelationship(steps, rounding, trace);

        var outputElement = element.GetProperty("output");
        if (RequiredString(outputElement, "numericType") != "INT64")
        {
            throw Invalid("Executable formula output must use INT64.");
        }

        return new FormulaDefinition(
            reference,
            rulesetId,
            ParseStatus(RequiredString(element, "status")),
            ParseConfidence(RequiredString(element, "confidence")),
            new FormulaApplicability(
                RequiredString(applicabilityElement, "characterClassId"),
                StringArray(applicabilityElement, "evolutionIds")),
            inputs,
            new FormulaOutputDefinition(
                RequiredString(outputElement, "id"),
                RequiredString(outputElement, "unit"),
                outputElement.TryGetProperty("numericBounds", out var outputBounds)
                    ? ParseBounds(outputBounds)
                    : null),
            executionModel == CheckedIntegerFormulaProgram.ModelId
                ? new CheckedIntegerFormulaProgram(steps)
                : new CheckedDecimalFormulaProgram(steps),
            rounding,
            trace,
            StringArray(element, "evidenceRefs"),
            OptionalStringArray(element, "conflictIds"),
            dependencies);
    }

    private static FormulaInputDefinition ParseInput(JsonElement element)
    {
        var source = element.GetProperty("source");
        var sourceKind = RequiredString(source, "kind");
        FormulaInputSource parsedSource;
        if (sourceKind == "CONTEXT_VALUE")
        {
            parsedSource = new FormulaInputSource(
                FormulaInputSourceKind.ContextValue,
                RequiredString(source, "valueId"));
        }
        else if (sourceKind == "FORMULA_OUTPUT")
        {
            parsedSource = new FormulaInputSource(
                new FormulaReference(
                    RequiredString(source, "formulaId"),
                    RequiredString(source, "formulaVersion")),
                ParseOutputStage(RequiredString(source, "outputStage")));
        }
        else
        {
            throw Invalid($"Unsupported formula input source kind '{sourceKind}'.");
        }

        return new FormulaInputDefinition(
            RequiredString(element, "id"),
            ParseNumericType(RequiredString(element, "numericType")),
            RequiredString(element, "unit"),
            ParseBounds(element.GetProperty("numericBounds")),
            RequiredString(element, "rangeErrorCode"),
            parsedSource);
    }

    private static FormulaNumericBounds ParseBounds(JsonElement element)
    {
        var hasMinimum = element.TryGetProperty("minimum", out var minimum);
        var hasMinimumInclusive = element.TryGetProperty(
            "minimumInclusive",
            out var minimumInclusive);
        var hasMaximum = element.TryGetProperty("maximum", out var maximum);
        var hasMaximumInclusive = element.TryGetProperty(
            "maximumInclusive",
            out var maximumInclusive);
        if (hasMinimum != hasMinimumInclusive ||
            hasMaximum != hasMaximumInclusive)
        {
            throw Invalid(
                "Every numeric bound must declare whether it is inclusive.");
        }

        return new FormulaNumericBounds(
            hasMinimum ? minimum.GetInt64() : null,
            hasMinimumInclusive && minimumInclusive.GetBoolean(),
            hasMaximum ? maximum.GetInt64() : null,
            hasMaximumInclusive && maximumInclusive.GetBoolean(),
            ParseBoundsClassification(RequiredString(element, "classification")),
            OptionalStringArray(element, "evidenceRefs"));
    }

    private static CheckedIntegerFormulaStep[] ParseAndValidateSteps(
        JsonElement stepsElement,
        IReadOnlySet<string> inputIds,
        string executionModel)
    {
        var steps = new List<CheckedIntegerFormulaStep>();
        var previousStepIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var stepElement in stepsElement.EnumerateArray())
        {
            var id = RequiredString(stepElement, "id");
            if (!previousStepIds.Add(id))
            {
                throw Incoherent($"Formula step '{id}' is duplicated.");
            }

            var operation = ParseOperation(RequiredString(stepElement, "operation"));
            var operands = stepElement.GetProperty("operands")
                .EnumerateArray()
                .Select(operand => ParseOperand(
                    operand,
                    inputIds,
                    previousStepIds,
                    executionModel))
                .ToArray();
            ValidateArity(id, operation, operands, executionModel);
            steps.Add(new CheckedIntegerFormulaStep(id, operation, operands));
        }

        return steps.ToArray();
    }

    private static CheckedIntegerOperand ParseOperand(
        JsonElement element,
        IReadOnlySet<string> inputIds,
        IReadOnlySet<string> previousStepIds,
        string executionModel)
    {
        var kind = RequiredString(element, "kind");
        return kind switch
        {
            "INPUT" => ParseInputOperand(element, inputIds),
            "STEP" => ParseStepOperand(element, previousStepIds),
            "LITERAL" when executionModel == CheckedIntegerFormulaProgram.ModelId =>
                new FormulaLiteralOperand(element.GetProperty("value").GetInt64()),
            "LITERAL" when executionModel == CheckedDecimalFormulaProgram.ModelId =>
                new FormulaDecimalLiteralOperand(
                    element.GetProperty("value").GetDecimal()),
            _ => throw Invalid($"Unsupported formula operand kind '{kind}'."),
        };
    }

    private static FormulaInputOperand ParseInputOperand(
        JsonElement element,
        IReadOnlySet<string> inputIds)
    {
        var inputId = RequiredString(element, "inputId");
        if (!inputIds.Contains(inputId))
        {
            throw Incoherent(
                $"Program references undeclared input '{inputId}'.");
        }

        return new FormulaInputOperand(inputId);
    }

    private static FormulaStepOperand ParseStepOperand(
        JsonElement element,
        IReadOnlySet<string> previousStepIds)
    {
        var stepId = RequiredString(element, "stepId");
        if (!previousStepIds.Contains(stepId))
        {
            throw Incoherent(
                $"Program references step '{stepId}' before it is available.");
        }

        return new FormulaStepOperand(stepId);
    }

    private static void ValidateArity(
        string stepId,
        CheckedIntegerOperation operation,
        CheckedIntegerOperand[] operands,
        string executionModel)
    {
        var hasExpectedLiteral = executionModel switch
        {
            CheckedIntegerFormulaProgram.ModelId =>
                operands is [FormulaLiteralOperand],
            CheckedDecimalFormulaProgram.ModelId =>
                operands is [FormulaDecimalLiteralOperand],
            _ => false,
        };
        var valid = operation switch
        {
            CheckedIntegerOperation.Constant =>
                hasExpectedLiteral,
            CheckedIntegerOperation.Add => operands.Length >= 2,
            CheckedIntegerOperation.Subtract or
            CheckedIntegerOperation.Multiply => operands.Length == 2,
            CheckedIntegerOperation.Divide =>
                executionModel == CheckedDecimalFormulaProgram.ModelId &&
                operands.Length == 2,
            CheckedIntegerOperation.ApplyRounding =>
                operands is [FormulaStepOperand],
            _ => false,
        };
        if (!valid)
        {
            throw Incoherent(
                $"Step '{stepId}' has invalid operands for operation '{operation}'.");
        }
    }

    private static FormulaRoundingDefinition ParseRounding(JsonElement element) =>
        new(
            ParseRoundingMode(RequiredString(element, "mode")),
            RequiredString(element, "stage"),
            element.TryGetProperty("decimalPlaces", out var decimalPlaces)
                ? decimalPlaces.GetInt32()
                : null);

    private static FormulaTraceDefinition ParseTrace(JsonElement element) =>
        new(
            StringArray(element, "stepIds"),
            RequiredString(element, "rawOutputStepId"),
            RequiredString(element, "visibleOutputStepId"));

    private static void ValidateProgramTraceRelationship(
        CheckedIntegerFormulaStep[] steps,
        FormulaRoundingDefinition rounding,
        FormulaTraceDefinition trace)
    {
        if (steps.Length != trace.StepIds.Length ||
            steps.Where((step, index) => step.Id != trace.StepIds[index]).Any())
        {
            throw Incoherent(
                "Program steps must exactly match the declared trace order.");
        }

        var stepIds = steps
            .Select(step => step.Id)
            .ToHashSet(StringComparer.Ordinal);
        if (!stepIds.Contains(trace.RawOutputStepId) ||
            !stepIds.Contains(trace.VisibleOutputStepId) ||
            rounding.StageId != trace.VisibleOutputStepId)
        {
            throw Incoherent(
                "Raw, visible, and rounding stages must resolve coherently.");
        }

        var visibleStep = steps.Single(step => step.Id == trace.VisibleOutputStepId);
        if (visibleStep.Operation != CheckedIntegerOperation.ApplyRounding ||
            visibleStep.Operands is not [FormulaStepOperand visibleInput] ||
            visibleInput.StepId != trace.RawOutputStepId)
        {
            throw Incoherent(
                "The visible output step must apply rounding to the raw output step.");
        }
    }

    private static CharacterRecord ParseCharacterClass(JsonElement element) =>
        new(
            RequiredString(element, "id"),
            RequiredString(element, "rulesetId"),
            element.GetProperty("evolutions")
                .EnumerateArray()
                .Select(evolution => RequiredString(evolution, "id"))
                .ToArray());

    private static void ValidateUniqueClasses(CharacterRecord[] classes)
    {
        if (classes.Length == 0)
        {
            throw Invalid("The snapshot contains no character classes.");
        }

        var duplicateClassId = Duplicate(classes.Select(item => item.Id));
        if (duplicateClassId is not null)
        {
            throw Incoherent(
                $"Character class ID '{duplicateClassId}' is duplicated.");
        }

        var duplicateEvolutionId = Duplicate(
            classes.SelectMany(item => item.EvolutionIds));
        if (duplicateEvolutionId is not null)
        {
            throw Incoherent(
                $"Evolution ID '{duplicateEvolutionId}' is duplicated.");
        }
    }

    private static void ValidateUniqueFormulaReferences(FormulaRecord[] records)
    {
        var duplicate = records
            .GroupBy(
                record => (record.Reference.Id, record.Reference.Version))
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new FormulaSnapshotException(
                FormulaSnapshotErrorCodes.DuplicateReference,
                $"Formula '{duplicate.Key.Id}' version '{duplicate.Key.Version}' " +
                "is duplicated.");
        }
    }

    private static string ResolveRulesetId(
        CharacterRecord[] classes,
        FormulaRecord[] formulas)
    {
        var rulesetIds = classes
            .Select(item => item.RulesetId)
            .Concat(formulas.Select(item => item.RulesetId))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (rulesetIds.Length != 1)
        {
            throw new FormulaSnapshotException(
                FormulaSnapshotErrorCodes.RulesetMismatch,
                "All formula records and character classes must belong to one ruleset.");
        }

        return rulesetIds[0];
    }

    private static void ValidateExecutableFormulas(
        CharacterRecord[] classes,
        FormulaDefinition[] formulas)
    {
        var classById = classes.ToDictionary(item => item.Id, StringComparer.Ordinal);
        foreach (var formula in formulas)
        {
            if (formula.Status != FormulaStatus.Published)
            {
                throw new FormulaSnapshotException(
                    FormulaSnapshotErrorCodes.FormulaNotPublished,
                    $"Formula '{formula.Reference.Id}' version " +
                    $"'{formula.Reference.Version}' is not PUBLISHED.");
            }

            if (!classById.TryGetValue(
                    formula.Applicability.CharacterClassId,
                    out var characterClass) ||
                formula.Applicability.EvolutionIds.Any(
                    evolutionId => !characterClass.EvolutionIds.Contains(
                        evolutionId,
                        StringComparer.Ordinal)))
            {
                throw Incoherent(
                    $"Formula '{formula.Reference.Id}' version " +
                    $"'{formula.Reference.Version}' has incoherent applicability.");
            }

            foreach (var dependencyReference in formula.DependencyFormulaRefs)
            {
                var dependency = formulas.SingleOrDefault(
                    candidate => candidate.Reference == dependencyReference)
                    ?? throw Incoherent(
                        $"Formula '{formula.Reference.Id}' version " +
                        $"'{formula.Reference.Version}' references unavailable " +
                        $"dependency '{dependencyReference.Id}' version " +
                        $"'{dependencyReference.Version}'.");
                if (dependency.RulesetId != formula.RulesetId ||
                    dependency.Applicability.CharacterClassId !=
                        formula.Applicability.CharacterClassId ||
                    formula.Applicability.EvolutionIds.Any(
                        evolutionId =>
                            !dependency.Applicability.EvolutionIds.Contains(
                                evolutionId)))
                {
                    throw Incoherent(
                        $"Formula dependency '{dependencyReference.Id}' version " +
                        $"'{dependencyReference.Version}' has incompatible applicability.");
                }
            }
        }

        ValidateDependencyGraph(formulas);
    }

    private static void ValidateDependencyGraph(FormulaDefinition[] formulas)
    {
        var byReference = formulas.ToDictionary(
            formula => formula.Reference,
            formula => formula);
        var completed = new HashSet<FormulaReference>();
        var active = new HashSet<FormulaReference>();

        foreach (var formula in formulas)
        {
            Visit(formula);
        }

        void Visit(FormulaDefinition formula)
        {
            if (completed.Contains(formula.Reference))
            {
                return;
            }

            if (!active.Add(formula.Reference))
            {
                throw Incoherent(
                    $"Formula dependency cycle detected at '{formula.Reference.Id}' " +
                    $"version '{formula.Reference.Version}'.");
            }

            foreach (var dependencyReference in formula.DependencyFormulaRefs)
            {
                Visit(byReference[dependencyReference]);
            }

            active.Remove(formula.Reference);
            completed.Add(formula.Reference);
        }
    }

    private static FormulaStatus ParseStatus(string value) =>
        value switch
        {
            "DRAFT" => FormulaStatus.Draft,
            "REVIEWED" => FormulaStatus.Reviewed,
            "PUBLISHED" => FormulaStatus.Published,
            "DEPRECATED" => FormulaStatus.Deprecated,
            _ => throw Invalid($"Unknown formula status '{value}'."),
        };

    private static FormulaConfidence ParseConfidence(string value) =>
        value switch
        {
            "UNVERIFIED" => FormulaConfidence.Unverified,
            "PARTIAL" => FormulaConfidence.Partial,
            "VERIFIED" => FormulaConfidence.Verified,
            "DISPUTED" => FormulaConfidence.Disputed,
            "DEPRECATED" => FormulaConfidence.Deprecated,
            _ => throw Invalid($"Unknown formula confidence '{value}'."),
        };

    private static FormulaNumericType ParseNumericType(string value) =>
        value switch
        {
            "INT32" => FormulaNumericType.Signed32Bit,
            "INT64" => FormulaNumericType.Signed64Bit,
            "DECIMAL" => FormulaNumericType.ExactBase10,
            _ => throw Invalid($"Unknown formula numeric type '{value}'."),
        };

    private static FormulaOutputStage ParseOutputStage(string value) =>
        value switch
        {
            "RAW" => FormulaOutputStage.Raw,
            "VISIBLE" => FormulaOutputStage.Visible,
            _ => throw Invalid($"Unknown formula output stage '{value}'."),
        };

    private static FormulaBoundsClassification ParseBoundsClassification(
        string value) =>
        value switch
        {
            "TECHNICAL" => FormulaBoundsClassification.Technical,
            "FACTUAL" => FormulaBoundsClassification.Factual,
            _ => throw Invalid($"Unknown bounds classification '{value}'."),
        };

    private static CheckedIntegerOperation ParseOperation(string value) =>
        value switch
        {
            "CONSTANT" => CheckedIntegerOperation.Constant,
            "ADD" => CheckedIntegerOperation.Add,
            "SUBTRACT" => CheckedIntegerOperation.Subtract,
            "MULTIPLY" => CheckedIntegerOperation.Multiply,
            "DIVIDE" => CheckedIntegerOperation.Divide,
            "APPLY_ROUNDING" => CheckedIntegerOperation.ApplyRounding,
            _ => throw Invalid($"Unknown formula operation '{value}'."),
        };

    private static FormulaRoundingMode ParseRoundingMode(string value) =>
        value switch
        {
            "NONE" => FormulaRoundingMode.None,
            "FLOOR" => FormulaRoundingMode.Floor,
            "CEILING" => FormulaRoundingMode.Ceiling,
            "TRUNCATE" => FormulaRoundingMode.Truncate,
            "HALF_UP" => FormulaRoundingMode.HalfUp,
            "HALF_EVEN" => FormulaRoundingMode.HalfEven,
            _ => throw Invalid($"Unknown rounding mode '{value}'."),
        };

    private static T[] LoadFiles<T>(
        string directory,
        Func<JsonElement, T> parse)
    {
        var paths = Directory.GetFiles(directory, "*.json")
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (paths.Length == 0)
        {
            throw Invalid($"Snapshot directory '{directory}' contains no JSON records.");
        }

        return paths.Select(path =>
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            return parse(document.RootElement);
        }).ToArray();
    }

    private static void EnsureDirectoryExists(string directory)
    {
        if (!Directory.Exists(directory))
        {
            throw new FormulaSnapshotException(
                FormulaSnapshotErrorCodes.SnapshotNotFound,
                $"Snapshot directory '{directory}' was not found.");
        }
    }

    private static string? Duplicate(IEnumerable<string> ids) =>
        ids.GroupBy(id => id, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1)
            ?.Key;

    private static string RequiredString(
        JsonElement element,
        string propertyName) =>
        element.GetProperty(propertyName).GetString()
        ?? throw Invalid($"'{propertyName}' cannot be null.");

    private static string[] StringArray(
        JsonElement element,
        string propertyName) =>
        element.GetProperty(propertyName)
            .EnumerateArray()
            .Select(item => item.GetString()
                ?? throw Invalid($"'{propertyName}' cannot contain null values."))
            .ToArray();

    private static string[] OptionalStringArray(
        JsonElement element,
        string propertyName) =>
        element.TryGetProperty(propertyName, out var values)
            ? values.EnumerateArray()
                .Select(item => item.GetString()
                    ?? throw Invalid(
                        $"'{propertyName}' cannot contain null values."))
                .ToArray()
            : [];

    private static FormulaReference ParseReference(JsonElement element) =>
        new(
            RequiredString(element, "id"),
            RequiredString(element, "version"));

    private static FormulaSnapshotException Invalid(string message) =>
        new(FormulaSnapshotErrorCodes.SnapshotInvalid, message);

    private static FormulaSnapshotException Incoherent(string message) =>
        new(FormulaSnapshotErrorCodes.ReferenceIncoherent, message);

    private sealed record CharacterRecord(
        string Id,
        string RulesetId,
        IReadOnlyList<string> EvolutionIds);

    private sealed record FormulaRecord(
        FormulaReference Reference,
        string RulesetId,
        FormulaDefinition? Definition);
}
