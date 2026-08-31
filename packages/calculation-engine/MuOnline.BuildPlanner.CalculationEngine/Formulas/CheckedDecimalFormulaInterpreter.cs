using MuOnline.BuildPlanner.Domain.Formulas;

namespace MuOnline.BuildPlanner.CalculationEngine.Formulas;

public static class CheckedDecimalFormulaInterpreter
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

        if (definition.Program is not CheckedDecimalFormulaProgram program)
        {
            throw Error(
                FormulaCalculationErrorCodes.ProgramNotSupported,
                $"Execution model '{definition.Program.ExecutionModel}' is not supported.");
        }

        var valuesByStep = new Dictionary<string, decimal>(StringComparer.Ordinal);
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
                "Formula arithmetic exceeds the supported exact decimal or Int64 output range.");
        }

        EnsureTraceContract(definition, traceSteps);

        if (!valuesByStep.TryGetValue(
                definition.Trace.RawOutputStepId,
                out var rawOutput) ||
            !valuesByStep.TryGetValue(
                definition.Trace.VisibleOutputStepId,
                out var roundedOutput))
        {
            throw InvalidProgram("The declared output steps were not evaluated.");
        }

        long visibleOutput;
        if (roundedOutput != decimal.Truncate(roundedOutput))
        {
            throw InvalidProgram(
                "The declared rounding step did not produce an integral Int64 output.");
        }

        try
        {
            visibleOutput = checked((long)roundedOutput);
        }
        catch (OverflowException)
        {
            throw Error(
                FormulaCalculationErrorCodes.ArithmeticOverflow,
                "The rounded formula output exceeds the supported Int64 range.");
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
            var isIntegral = value == decimal.Truncate(value);
            var matchesNumericType = inputDefinition.NumericType switch
            {
                FormulaNumericType.Signed32Bit =>
                    isIntegral && value is >= int.MinValue and <= int.MaxValue,
                FormulaNumericType.Signed64Bit =>
                    isIntegral && value is >= long.MinValue and <= long.MaxValue,
                FormulaNumericType.ExactBase10 => true,
                _ => false,
            };
            if (!matchesNumericType ||
                !inputDefinition.NumericBounds.Contains(value))
            {
                throw Error(
                    inputDefinition.RangeErrorCode,
                    $"Input '{inputDefinition.Id}' is outside its declared range.");
            }
        }
    }

    private static decimal EvaluateStep(
        CheckedIntegerFormulaStep step,
        FormulaDefinition definition,
        IReadOnlyDictionary<string, decimal> inputs,
        IReadOnlyDictionary<string, decimal> valuesByStep) =>
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
            CheckedIntegerOperation.Divide =>
                EvaluateDivide(step, inputs, valuesByStep),
            CheckedIntegerOperation.ApplyRounding =>
                EvaluateRounding(step, definition, inputs, valuesByStep),
            _ => throw InvalidProgram(
                $"Operation '{step.Operation}' is not supported."),
        };

    private static decimal EvaluateConstant(CheckedIntegerFormulaStep step)
    {
        if (step.Operands is not [FormulaDecimalLiteralOperand literal])
        {
            throw InvalidProgram(
                $"CONSTANT step '{step.Id}' requires exactly one decimal literal operand.");
        }

        return literal.Value;
    }

    private static decimal EvaluateAdd(
        CheckedIntegerFormulaStep step,
        IReadOnlyDictionary<string, decimal> inputs,
        IReadOnlyDictionary<string, decimal> valuesByStep)
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

    private static decimal EvaluateBinary(
        CheckedIntegerFormulaStep step,
        IReadOnlyDictionary<string, decimal> inputs,
        IReadOnlyDictionary<string, decimal> valuesByStep,
        Func<decimal, decimal, decimal> operation)
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

    private static decimal EvaluateDivide(
        CheckedIntegerFormulaStep step,
        IReadOnlyDictionary<string, decimal> inputs,
        IReadOnlyDictionary<string, decimal> valuesByStep)
    {
        if (step.Operands.Length != 2)
        {
            throw InvalidProgram(
                $"DIVIDE step '{step.Id}' requires exactly two operands.");
        }

        var divisor = ResolveOperand(step.Operands[1], inputs, valuesByStep);
        if (divisor == 0)
        {
            throw InvalidProgram(
                $"DIVIDE step '{step.Id}' cannot divide by zero.");
        }

        return checked(
            ResolveOperand(step.Operands[0], inputs, valuesByStep) / divisor);
    }

    private static decimal EvaluateRounding(
        CheckedIntegerFormulaStep step,
        FormulaDefinition definition,
        IReadOnlyDictionary<string, decimal> inputs,
        IReadOnlyDictionary<string, decimal> valuesByStep)
    {
        if (step.Operands is not [FormulaStepOperand])
        {
            throw InvalidProgram(
                $"APPLY_ROUNDING step '{step.Id}' requires exactly one step operand.");
        }

        var value = ResolveOperand(step.Operands[0], inputs, valuesByStep);
        var decimalPlaces = definition.Rounding.DecimalPlaces ?? 0;
        return definition.Rounding.Mode switch
        {
            FormulaRoundingMode.None => value,
            FormulaRoundingMode.Floor => decimal.Floor(value * PowerOfTen(decimalPlaces)) /
                PowerOfTen(decimalPlaces),
            FormulaRoundingMode.Ceiling => decimal.Ceiling(value * PowerOfTen(decimalPlaces)) /
                PowerOfTen(decimalPlaces),
            FormulaRoundingMode.Truncate => decimal.Truncate(value * PowerOfTen(decimalPlaces)) /
                PowerOfTen(decimalPlaces),
            FormulaRoundingMode.HalfUp => RoundHalfUp(value, decimalPlaces),
            FormulaRoundingMode.HalfEven =>
                decimal.Round(value, decimalPlaces, MidpointRounding.ToEven),
            _ => throw InvalidProgram(
                $"Rounding mode '{definition.Rounding.Mode}' is not supported."),
        };
    }

    private static decimal RoundHalfUp(decimal value, int decimalPlaces)
    {
        var scale = PowerOfTen(decimalPlaces);
        var scaled = value * scale;
        var rounded = scaled >= 0
            ? decimal.Floor(scaled + 0.5m)
            : decimal.Ceiling(scaled - 0.5m);
        return rounded / scale;
    }

    private static decimal PowerOfTen(int decimalPlaces)
    {
        var result = 1m;
        for (var index = 0; index < decimalPlaces; index++)
        {
            result = checked(result * 10m);
        }

        return result;
    }

    private static decimal ResolveOperand(
        CheckedIntegerOperand operand,
        IReadOnlyDictionary<string, decimal> inputs,
        IReadOnlyDictionary<string, decimal> valuesByStep) =>
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
            FormulaDecimalLiteralOperand literal => literal.Value,
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
