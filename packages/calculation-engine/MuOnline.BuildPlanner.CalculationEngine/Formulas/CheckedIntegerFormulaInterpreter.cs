using MuOnline.BuildPlanner.Domain.Formulas;

namespace MuOnline.BuildPlanner.CalculationEngine.Formulas;

public static class CheckedIntegerFormulaInterpreter
{
    public static FormulaCalculationResult Calculate(
        FormulaDefinition definition,
        FormulaCalculationRequest request)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(request);

        EnsurePublished(definition);
        EnsureApplicable(definition, request);
        EnsureExactAndValidInputs(definition, request);

        if (definition.Program is not CheckedIntegerFormulaProgram program)
        {
            throw Error(
                FormulaCalculationErrorCodes.ProgramNotSupported,
                $"Execution model '{definition.Program.ExecutionModel}' is not supported.");
        }

        var valuesByStep = new Dictionary<string, long>(StringComparer.Ordinal);
        var traceSteps = new List<FormulaCalculationTraceStep>(program.Steps.Length);

        try
        {
            foreach (var step in program.Steps)
            {
                if (valuesByStep.ContainsKey(step.Id))
                {
                    throw InvalidProgram($"Step ID '{step.Id}' is duplicated.");
                }

                var value = EvaluateStep(
                    step,
                    definition,
                    request.Inputs,
                    valuesByStep);
                valuesByStep.Add(step.Id, value);
                traceSteps.Add(new FormulaCalculationTraceStep(step.Id, value));
            }
        }
        catch (OverflowException)
        {
            throw Error(
                FormulaCalculationErrorCodes.ArithmeticOverflow,
                "Formula arithmetic exceeds the supported 64-bit integer range.");
        }

        EnsureTraceContract(definition, traceSteps);

        if (!valuesByStep.TryGetValue(
                definition.Trace.RawOutputStepId,
                out var rawOutput) ||
            !valuesByStep.TryGetValue(
                definition.Trace.VisibleOutputStepId,
                out var visibleOutput))
        {
            throw InvalidProgram("The declared output steps were not evaluated.");
        }

        var trace = new FormulaCalculationTrace(
            definition.RulesetId,
            definition.Reference,
            request.Context,
            request.Inputs,
            traceSteps,
            definition.Rounding,
            rawOutput,
            visibleOutput,
            definition.EvidenceRefs,
            definition.ConflictIds);

        return new FormulaCalculationResult(
            definition.Output.Id,
            rawOutput,
            visibleOutput,
            trace);
    }

    private static void EnsurePublished(FormulaDefinition definition)
    {
        if (definition.Status != FormulaStatus.Published)
        {
            throw Error(
                FormulaCalculationErrorCodes.FormulaNotPublished,
                $"Formula '{definition.Reference.Id}' version " +
                $"'{definition.Reference.Version}' is not published.");
        }
    }

    private static void EnsureApplicable(
        FormulaDefinition definition,
        FormulaCalculationRequest request)
    {
        if (request.Context.CharacterClassId !=
                definition.Applicability.CharacterClassId ||
            !definition.Applicability.EvolutionIds.Contains(
                request.Context.EvolutionId))
        {
            throw Error(
                FormulaCalculationErrorCodes.FormulaNotApplicable,
                $"Formula '{definition.Reference.Id}' does not apply to class " +
                $"'{request.Context.CharacterClassId}' and evolution " +
                $"'{request.Context.EvolutionId}'.");
        }
    }

    private static void EnsureExactAndValidInputs(
        FormulaDefinition definition,
        FormulaCalculationRequest request)
    {
        var inputDefinitions = definition.Inputs.ToDictionary(
            input => input.Id,
            StringComparer.Ordinal);

        var undeclaredInput = request.Inputs.Keys.FirstOrDefault(
            inputId => !inputDefinitions.ContainsKey(inputId));
        if (undeclaredInput is not null)
        {
            throw Error(
                FormulaCalculationErrorCodes.InputNotDeclared,
                $"Input '{undeclaredInput}' is not declared by the formula.");
        }

        var missingInput = inputDefinitions.Keys.FirstOrDefault(
            inputId => !request.Inputs.ContainsKey(inputId));
        if (missingInput is not null)
        {
            throw Error(
                FormulaCalculationErrorCodes.InputMissing,
                $"Input '{missingInput}' is required by the formula.");
        }

        foreach (var inputDefinition in definition.Inputs)
        {
            var value = request.Inputs[inputDefinition.Id];
            var matchesNumericType =
                inputDefinition.NumericType != FormulaNumericType.Signed32Bit ||
                value is >= int.MinValue and <= int.MaxValue;
            if (!matchesNumericType ||
                !inputDefinition.NumericBounds.Contains(value))
            {
                throw Error(
                    inputDefinition.RangeErrorCode,
                    $"Input '{inputDefinition.Id}' is outside its declared range.");
            }
        }
    }

    private static long EvaluateStep(
        CheckedIntegerFormulaStep step,
        FormulaDefinition definition,
        IReadOnlyDictionary<string, long> inputs,
        IReadOnlyDictionary<string, long> valuesByStep) =>
        step.Operation switch
        {
            CheckedIntegerOperation.Constant =>
                EvaluateConstant(step),
            CheckedIntegerOperation.Add =>
                EvaluateAdd(step, inputs, valuesByStep),
            CheckedIntegerOperation.Subtract =>
                EvaluateBinary(
                    step,
                    inputs,
                    valuesByStep,
                    static (left, right) => checked(left - right)),
            CheckedIntegerOperation.Multiply =>
                EvaluateBinary(
                    step,
                    inputs,
                    valuesByStep,
                    static (left, right) => checked(left * right)),
            CheckedIntegerOperation.ApplyRounding =>
                EvaluateRounding(step, definition, inputs, valuesByStep),
            _ => throw InvalidProgram(
                $"Operation '{step.Operation}' is not supported."),
        };

    private static long EvaluateConstant(CheckedIntegerFormulaStep step)
    {
        if (step.Operands is not [FormulaLiteralOperand literal])
        {
            throw InvalidProgram(
                $"CONSTANT step '{step.Id}' requires exactly one literal operand.");
        }

        return literal.Value;
    }

    private static long EvaluateAdd(
        CheckedIntegerFormulaStep step,
        IReadOnlyDictionary<string, long> inputs,
        IReadOnlyDictionary<string, long> valuesByStep)
    {
        if (step.Operands.Length < 2)
        {
            throw InvalidProgram(
                $"ADD step '{step.Id}' requires at least two operands.");
        }

        var result = ResolveOperand(step.Operands[0], inputs, valuesByStep);
        for (var index = 1; index < step.Operands.Length; index++)
        {
            result = checked(
                result + ResolveOperand(step.Operands[index], inputs, valuesByStep));
        }

        return result;
    }

    private static long EvaluateBinary(
        CheckedIntegerFormulaStep step,
        IReadOnlyDictionary<string, long> inputs,
        IReadOnlyDictionary<string, long> valuesByStep,
        Func<long, long, long> operation)
    {
        if (step.Operands.Length != 2)
        {
            throw InvalidProgram(
                $"{step.Operation} step '{step.Id}' requires exactly two operands.");
        }

        return operation(
            ResolveOperand(step.Operands[0], inputs, valuesByStep),
            ResolveOperand(step.Operands[1], inputs, valuesByStep));
    }

    private static long EvaluateRounding(
        CheckedIntegerFormulaStep step,
        FormulaDefinition definition,
        IReadOnlyDictionary<string, long> inputs,
        IReadOnlyDictionary<string, long> valuesByStep)
    {
        if (step.Id != definition.Rounding.StageId ||
            step.Operands is not [FormulaStepOperand])
        {
            throw InvalidProgram(
                $"APPLY_ROUNDING step '{step.Id}' does not match the rounding definition.");
        }

        // CHECKED_INT64_V1 has integral intermediates, so every declared
        // rounding mode is an identity operation at any decimal precision.
        return ResolveOperand(step.Operands[0], inputs, valuesByStep);
    }

    private static long ResolveOperand(
        CheckedIntegerOperand operand,
        IReadOnlyDictionary<string, long> inputs,
        IReadOnlyDictionary<string, long> valuesByStep) =>
        operand switch
        {
            FormulaInputOperand input when inputs.TryGetValue(input.InputId, out var value) =>
                value,
            FormulaInputOperand input =>
                throw InvalidProgram(
                    $"Program references unavailable input '{input.InputId}'."),
            FormulaStepOperand step when valuesByStep.TryGetValue(step.StepId, out var value) =>
                value,
            FormulaStepOperand step =>
                throw InvalidProgram(
                    $"Program references step '{step.StepId}' before it is available."),
            FormulaLiteralOperand literal => literal.Value,
            _ => throw InvalidProgram("Program contains an unsupported operand."),
        };

    private static void EnsureTraceContract(
        FormulaDefinition definition,
        List<FormulaCalculationTraceStep> traceSteps)
    {
        if (traceSteps.Count != definition.Trace.StepIds.Length ||
            traceSteps
                .Select(step => step.StepId)
                .Where((stepId, index) =>
                    stepId != definition.Trace.StepIds[index])
                .Any())
        {
            throw InvalidProgram(
                "Evaluated steps do not match the declared trace order.");
        }

        if (definition.Rounding.StageId != definition.Trace.VisibleOutputStepId)
        {
            throw InvalidProgram(
                "The rounding stage does not match the visible output step.");
        }
    }

    private static FormulaCalculationException InvalidProgram(string message) =>
        Error(FormulaCalculationErrorCodes.ProgramInvalid, message);

    private static FormulaCalculationException Error(string code, string message) =>
        new(code, message);
}
