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

    [Fact]
    public void ExactDecimalProgramDividesWithoutReplacingTheDivisorWithACoefficient()
    {
        var definition = CreateDefinition(
            new CheckedDecimalFormulaProgram(
            [
                new(
                    "raw-hp",
                    CheckedIntegerOperation.Divide,
                    [
                        new FormulaInputOperand("character-level"),
                        new FormulaDecimalLiteralOperand(30m),
                    ]),
                new(
                    "visible-hp",
                    CheckedIntegerOperation.ApplyRounding,
                    [new FormulaStepOperand("raw-hp")]),
            ]),
            ["raw-hp", "visible-hp"],
            "raw-hp");

        var result = CheckedDecimalFormulaInterpreter.Calculate(
            definition,
            Request(level: 1, vitality: 20));

        Assert.Equal(1m / 30m, result.RawOutput);
        Assert.Equal(0, result.VisibleOutput);

        var zeroDivisorDefinition = CreateDefinition(
            new CheckedDecimalFormulaProgram(
            [
                new(
                    "raw-hp",
                    CheckedIntegerOperation.Divide,
                    [
                        new FormulaInputOperand("character-level"),
                        new FormulaDecimalLiteralOperand(0m),
                    ]),
                new(
                    "visible-hp",
                    CheckedIntegerOperation.ApplyRounding,
                    [new FormulaStepOperand("raw-hp")]),
            ]),
            ["raw-hp", "visible-hp"],
            "raw-hp");
        var exception = Assert.Throws<FormulaCalculationException>(
            () => CheckedDecimalFormulaInterpreter.Calculate(
                zeroDivisorDefinition,
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

    [Fact]
    public void MultiTruncateProgramTruncatesEachTermBeforeSumming()
    {
        var result = CalculateSummonerSd(level: 1, strength: 21, agility: 21,
            vitality: 18, energy: 23, defense: 7);

        Assert.Equal(102m, result.RawOutput);
        Assert.Equal(102, result.VisibleOutput);
        Assert.Equal(
            [
                83m, 99.6m, 99m, 3.5m, 3m, 1m,
                0.0333333333333333333333333333m, 0m, 102m, 102m,
            ],
            result.Trace.Steps.Select(step => step.Value));
        Assert.All(
            SupersetOf(
                result.Trace.Steps,
                ["stat-term", "defense-term", "level-term"]),
            step => Assert.Equal(0, step.Value % 1m));
    }

    private static IEnumerable<FormulaCalculationTraceStep> SupersetOf(
        IEnumerable<FormulaCalculationTraceStep> steps,
        params string[] ids) =>
        steps.Where(step => ids.Contains(step.StepId));

    private static FormulaCalculationResult CalculateSummonerSd(
        long level, long strength, long agility, long vitality,
        long energy, long defense) =>
        CheckedDecimalFormulaInterpreter.Calculate(
            CreateSummonerSdDefinition(),
            new FormulaCalculationRequest(
                new FormulaCalculationContext(
                    "class-synthetic", "evolution-synthetic"),
                new Dictionary<string, long>
                {
                    ["character-level"] = level,
                    ["strength"] = strength,
                    ["agility"] = agility,
                    ["vitality"] = vitality,
                    ["energy"] = energy,
                    ["defense"] = defense,
                }));

    private static FormulaDefinition CreateSummonerSdDefinition() =>
        new(
            new FormulaReference("formula-synthetic-summoner-sd", "0.1.0"),
            "ruleset-synthetic",
            FormulaStatus.Published,
            FormulaConfidence.Unverified,
            new FormulaApplicability(
                "class-synthetic",
                ["evolution-synthetic"]),
            [
                Input("character-level", FormulaNumericType.Signed32Bit, 1),
                Input("strength", FormulaNumericType.Signed64Bit, 21),
                Input("agility", FormulaNumericType.Signed64Bit, 21),
                Input("vitality", FormulaNumericType.Signed64Bit, 18),
                Input("energy", FormulaNumericType.Signed64Bit, 23),
                Input("defense", FormulaNumericType.Signed64Bit, 0),
            ],
            new FormulaOutputDefinition("sd", "sd-point"),
            new CheckedDecimalFormulaProgram(
            [
                new(
                    "stat-sum",
                    CheckedIntegerOperation.Add,
                    [
                        new FormulaInputOperand("strength"),
                        new FormulaInputOperand("agility"),
                        new FormulaInputOperand("vitality"),
                        new FormulaInputOperand("energy"),
                    ]),
                new(
                    "stat-product",
                    CheckedIntegerOperation.Multiply,
                    [
                        new FormulaStepOperand("stat-sum"),
                        new FormulaDecimalLiteralOperand(1.2m),
                    ]),
                new(
                    "stat-term",
                    CheckedIntegerOperation.ApplyRounding,
                    [new FormulaStepOperand("stat-product")]),
                new(
                    "defense-product",
                    CheckedIntegerOperation.Divide,
                    [
                        new FormulaInputOperand("defense"),
                        new FormulaDecimalLiteralOperand(2m),
                    ]),
                new(
                    "defense-term",
                    CheckedIntegerOperation.ApplyRounding,
                    [new FormulaStepOperand("defense-product")]),
                new(
                    "level-square",
                    CheckedIntegerOperation.Multiply,
                    [
                        new FormulaInputOperand("character-level"),
                        new FormulaInputOperand("character-level"),
                    ]),
                new(
                    "level-product",
                    CheckedIntegerOperation.Divide,
                    [
                        new FormulaStepOperand("level-square"),
                        new FormulaDecimalLiteralOperand(30m),
                    ]),
                new(
                    "level-term",
                    CheckedIntegerOperation.ApplyRounding,
                    [new FormulaStepOperand("level-product")]),
                new(
                    "raw-sd",
                    CheckedIntegerOperation.Add,
                    [
                        new FormulaStepOperand("stat-term"),
                        new FormulaStepOperand("defense-term"),
                        new FormulaStepOperand("level-term"),
                    ]),
                new(
                    "visible-sd",
                    CheckedIntegerOperation.ApplyRounding,
                    [new FormulaStepOperand("raw-sd")]),
            ]),
            new FormulaRoundingDefinition(
                FormulaRoundingMode.Truncate,
                "visible-sd",
                decimalPlaces: 0),
            new FormulaTraceDefinition(
                [
                    "stat-sum",
                    "stat-product",
                    "stat-term",
                    "defense-product",
                    "defense-term",
                    "level-square",
                    "level-product",
                    "level-term",
                    "raw-sd",
                    "visible-sd",
                ],
                "raw-sd",
                "visible-sd"),
            ["evidence-synthetic"]);
}
