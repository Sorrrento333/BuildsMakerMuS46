using System.Collections.Immutable;

namespace MuOnline.BuildPlanner.Domain.Formulas;

public enum CheckedIntegerOperation
{
    Constant,
    Add,
    Subtract,
    Multiply,
    Divide,
    ApplyRounding,
}

public abstract record CheckedIntegerOperand;

public sealed record FormulaInputOperand : CheckedIntegerOperand
{
    public FormulaInputOperand(string inputId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputId);
        InputId = inputId;
    }

    public string InputId { get; }
}

public sealed record FormulaStepOperand : CheckedIntegerOperand
{
    public FormulaStepOperand(string stepId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stepId);
        StepId = stepId;
    }

    public string StepId { get; }
}

public sealed record FormulaLiteralOperand(long Value) : CheckedIntegerOperand;

public sealed record FormulaDecimalLiteralOperand(decimal Value) : CheckedIntegerOperand;

public sealed class CheckedIntegerFormulaStep
{
    public CheckedIntegerFormulaStep(
        string id,
        CheckedIntegerOperation operation,
        IEnumerable<CheckedIntegerOperand> operands)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(operands);

        var normalizedOperands = operands.ToImmutableArray();
        if (normalizedOperands.Any(operand => operand is null))
        {
            throw new ArgumentException(
                "Formula operands cannot contain null values.",
                nameof(operands));
        }

        Id = id;
        Operation = operation;
        Operands = normalizedOperands;
    }

    public string Id { get; }

    public CheckedIntegerOperation Operation { get; }

    public ImmutableArray<CheckedIntegerOperand> Operands { get; }
}

public sealed class CheckedIntegerFormulaProgram : FormulaProgram
{
    public const string ModelId = "CHECKED_INT64_V1";

    public CheckedIntegerFormulaProgram(
        IEnumerable<CheckedIntegerFormulaStep> steps)
        : base(ModelId)
    {
        ArgumentNullException.ThrowIfNull(steps);

        var normalizedSteps = steps.ToImmutableArray();
        if (normalizedSteps.Length == 0)
        {
            throw new ArgumentException(
                "At least one formula step is required.",
                nameof(steps));
        }

        if (normalizedSteps.Any(step => step is null))
        {
            throw new ArgumentException(
                "Formula steps cannot contain null values.",
                nameof(steps));
        }

        Steps = normalizedSteps;
    }

    public ImmutableArray<CheckedIntegerFormulaStep> Steps { get; }
}

public sealed class CheckedDecimalFormulaProgram : FormulaProgram
{
    public const string ModelId = "CHECKED_DECIMAL_V1";

    public CheckedDecimalFormulaProgram(
        IEnumerable<CheckedIntegerFormulaStep> steps)
        : base(ModelId)
    {
        ArgumentNullException.ThrowIfNull(steps);

        var normalizedSteps = steps.ToImmutableArray();
        if (normalizedSteps.Length == 0)
        {
            throw new ArgumentException(
                "At least one formula step is required.",
                nameof(steps));
        }

        if (normalizedSteps.Any(step => step is null))
        {
            throw new ArgumentException(
                "Formula steps cannot contain null values.",
                nameof(steps));
        }

        Steps = normalizedSteps;
    }

    public ImmutableArray<CheckedIntegerFormulaStep> Steps { get; }
}
