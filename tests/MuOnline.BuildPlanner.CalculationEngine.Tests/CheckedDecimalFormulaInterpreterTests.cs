using MuOnline.BuildPlanner.CalculationEngine.Formulas;
using MuOnline.BuildPlanner.Domain.Formulas;
using Xunit;

namespace MuOnline.BuildPlanner.CalculationEngine.Tests;

public sealed class CheckedDecimalFormulaInterpreterTests
{
    [Fact]
    public void ExactDecimalProgramPreservesFractionUntilDeclaredTruncation()
    {
        var result = Calculate(level: 2, vitality: 20);

        Assert.Equal(91.5m, result.RawOutput);
        Assert.Equal(91, result.VisibleOutput);
        Assert.Equal(
            [50m, 1m, 1.5m, 40m, 91.5m, 91m],
            result.Trace.Steps.Select(step => step.Value));
    }

    [Fact]
    public void ExactDecimalProgramDoesNotRoundIndependentContributions()
    {
        var result = Calculate(level: 4, vitality: 20);

        Assert.Equal(94.5m, result.RawOutput);
        Assert.Equal(94, result.VisibleOutput);
        Assert.Equal(
            4.5m,
            result.Trace.Steps.Single(
                step => step.StepId == "level-contribution").Value);
    }

    [Fact]
    public void RoundedOutputOutsideInt64IsReportedAsArithmeticOverflow()
    {
        var exception = Assert.Throws<FormulaCalculationException>(
            () => Calculate(level: 1, vitality: 4611686018427387904));

        Assert.Equal(
            FormulaCalculationErrorCodes.ArithmeticOverflow,
            exception.Code);
    }

    [Fact]
    public void DecimalProgramRejectsIntegerLiteralOperandInstances()
    {
        var definition = CreateDefinition(
            new CheckedDecimalFormulaProgram(
            [
                new(
                    "raw-hp",
                    CheckedIntegerOperation.Constant,
                    [new FormulaLiteralOperand(1)]),
                new(
                    "visible-hp",
                    CheckedIntegerOperation.ApplyRounding,
                    [new FormulaStepOperand("raw-hp")]),
            ]),
            ["raw-hp", "visible-hp"],
            "raw-hp");

        var exception = Assert.Throws<FormulaCalculationException>(
            () => CheckedDecimalFormulaInterpreter.Calculate(
                definition,
                Request(level: 1, vitality: 20)));

        Assert.Equal(FormulaCalculationErrorCodes.ProgramInvalid, exception.Code);
    }

    private static FormulaCalculationResult Calculate(long level, long vitality) =>
        CheckedDecimalFormulaInterpreter.Calculate(
            CreateDefinition(CreateProgram()),
            Request(level, vitality));

    private static FormulaCalculationRequest Request(long level, long vitality) =>
        new(
            new FormulaCalculationContext("class-synthetic", "evolution-synthetic"),
            new Dictionary<string, long>
            {
                ["character-level"] = level,
                ["vitality"] = vitality,
            });

    private static FormulaDefinition CreateDefinition(
        FormulaProgram program,
        IEnumerable<string>? stepIds = null,
        string rawOutputStepId = "raw-hp") =>
        new(
            new FormulaReference("formula-synthetic-decimal", "0.1.0"),
            "ruleset-synthetic",
            FormulaStatus.Published,
            FormulaConfidence.Unverified,
            new FormulaApplicability(
                "class-synthetic",
                ["evolution-synthetic"]),
            [
                Input("character-level", FormulaNumericType.Signed32Bit, 1),
                Input("vitality", FormulaNumericType.Signed64Bit, 20),
            ],
            new FormulaOutputDefinition("hp", "hp-point"),
            program,
            new FormulaRoundingDefinition(
                FormulaRoundingMode.Truncate,
                "visible-hp",
                decimalPlaces: 0),
            new FormulaTraceDefinition(
                stepIds ??
                [
                    "base",
                    "level-offset",
                    "level-contribution",
                    "vitality-contribution",
                    "raw-hp",
                    "visible-hp",
                ],
                rawOutputStepId,
                "visible-hp"),
            ["evidence-synthetic"]);

    private static FormulaInputDefinition Input(
        string id,
        FormulaNumericType type,
        long minimum) =>
        new(
            id,
            type,
            "synthetic-unit",
            new FormulaNumericBounds(
                minimum,
                true,
                null,
                false,
                FormulaBoundsClassification.Technical),
            "synthetic-input-out-of-range",
            new FormulaInputSource(
                FormulaInputSourceKind.ContextValue,
                id));

    private static CheckedDecimalFormulaProgram CreateProgram() =>
        new(
        [
            new(
                "base",
                CheckedIntegerOperation.Constant,
                [new FormulaDecimalLiteralOperand(50m)]),
            new(
                "level-offset",
                CheckedIntegerOperation.Subtract,
                [
                    new FormulaInputOperand("character-level"),
                    new FormulaDecimalLiteralOperand(1m),
                ]),
            new(
                "level-contribution",
                CheckedIntegerOperation.Multiply,
                [
                    new FormulaStepOperand("level-offset"),
                    new FormulaDecimalLiteralOperand(1.5m),
                ]),
            new(
                "vitality-contribution",
                CheckedIntegerOperation.Multiply,
                [
                    new FormulaInputOperand("vitality"),
                    new FormulaDecimalLiteralOperand(2m),
                ]),
            new(
                "raw-hp",
                CheckedIntegerOperation.Add,
                [
                    new FormulaStepOperand("base"),
                    new FormulaStepOperand("level-contribution"),
                    new FormulaStepOperand("vitality-contribution"),
                ]),
            new(
                "visible-hp",
                CheckedIntegerOperation.ApplyRounding,
                [new FormulaStepOperand("raw-hp")]),
        ]);
}
