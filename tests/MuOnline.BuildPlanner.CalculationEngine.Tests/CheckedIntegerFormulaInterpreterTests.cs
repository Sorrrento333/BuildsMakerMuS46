using MuOnline.BuildPlanner.CalculationEngine.Formulas;
using MuOnline.BuildPlanner.Domain.Formulas;
using Xunit;

namespace MuOnline.BuildPlanner.CalculationEngine.Tests;

public sealed class CheckedIntegerFormulaInterpreterTests
{
    private const string SyntheticRangeError = "synthetic-input-out-of-range";

    [Fact]
    public void SyntheticProgramExecutesEveryOperationAndReturnsOrderedTrace()
    {
        var result = Calculate(CreateDefinition(), ("input-alpha", 4));

        Assert.Equal("output-synthetic", result.OutputId);
        Assert.Equal(16, result.RawOutput);
        Assert.Equal(16, result.VisibleOutput);
        Assert.Equal(
            ["constant", "adjusted", "scaled", "raw-output", "visible-output"],
            result.Trace.Steps.Select(step => step.StepId));
        Assert.Equal([10L, 3L, 6L, 16L, 16L], result.Trace.Steps.Select(step => step.Value));
        Assert.Equal("ruleset-synthetic", result.Trace.RulesetId);
        Assert.Equal("formula-synthetic", result.Trace.FormulaReference.Id);
        Assert.Equal("0.1.0", result.Trace.FormulaReference.Version);
        Assert.Equal(["evidence-synthetic"], result.Trace.EvidenceRefs);
        Assert.Equal(["conflict-synthetic"], result.Trace.ConflictIds);
        Assert.Equal(4, result.Trace.Inputs["input-alpha"]);
    }

    [Fact]
    public void DefinitionAndRequestCopyCallerOwnedCollections()
    {
        var evolutions = new List<string> { "evolution-synthetic" };
        var evidence = new List<string> { "evidence-synthetic" };
        var inputValues = new Dictionary<string, long> { ["input-alpha"] = 4 };
        var definition = CreateDefinition(evolutionIds: evolutions, evidenceRefs: evidence);
        var request = new FormulaCalculationRequest(
            new FormulaCalculationContext("class-synthetic", "evolution-synthetic"),
            inputValues);

        evolutions.Add("evolution-after-construction");
        evidence.Add("evidence-after-construction");
        inputValues["input-alpha"] = 99;

        var result = CheckedIntegerFormulaInterpreter.Calculate(definition, request);

        Assert.DoesNotContain(
            "evolution-after-construction",
            definition.Applicability.EvolutionIds);
        Assert.DoesNotContain("evidence-after-construction", result.Trace.EvidenceRefs);
        Assert.Equal(4, result.Trace.Inputs["input-alpha"]);
    }

    [Theory]
    [InlineData(FormulaStatus.Draft)]
    [InlineData(FormulaStatus.Reviewed)]
    [InlineData(FormulaStatus.Deprecated)]
    public void FormulaMustBePublished(FormulaStatus status)
    {
        AssertError(
            CreateDefinition(status: status),
            FormulaCalculationErrorCodes.FormulaNotPublished,
            ("input-alpha", 4));
    }

    [Theory]
    [InlineData("class-other", "evolution-synthetic")]
    [InlineData("class-synthetic", "evolution-other")]
    public void FormulaMustApplyToClassAndEvolution(
        string characterClassId,
        string evolutionId)
    {
        var exception = Assert.Throws<FormulaCalculationException>(
            () => CheckedIntegerFormulaInterpreter.Calculate(
                CreateDefinition(),
                new FormulaCalculationRequest(
                    new FormulaCalculationContext(characterClassId, evolutionId),
                    new Dictionary<string, long> { ["input-alpha"] = 4 })));

        Assert.Equal(FormulaCalculationErrorCodes.FormulaNotApplicable, exception.Code);
    }

    [Fact]
    public void MissingInputIsRejected()
    {
        AssertError(
            CreateDefinition(),
            FormulaCalculationErrorCodes.InputMissing);
    }

    [Fact]
    public void UndeclaredInputIsRejected()
    {
        AssertError(
            CreateDefinition(),
            FormulaCalculationErrorCodes.InputNotDeclared,
            ("input-alpha", 4),
            ("input-extra", 1));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(2147483648)]
    public void InputBoundsAndNumericTypeUseMaterializedRangeCode(long value)
    {
        AssertError(CreateDefinition(), SyntheticRangeError, ("input-alpha", value));
    }

    [Fact]
    public void ExclusiveBoundsAreEnforced()
    {
        var bounds = new FormulaNumericBounds(
            0,
            false,
            10,
            false,
            FormulaBoundsClassification.Technical);

        AssertError(
            CreateDefinition(inputBounds: bounds),
            SyntheticRangeError,
            ("input-alpha", 0));
        AssertError(
            CreateDefinition(inputBounds: bounds),
            SyntheticRangeError,
            ("input-alpha", 10));
    }

    [Theory]
    [InlineData(FormulaRoundingMode.None)]
    [InlineData(FormulaRoundingMode.Floor)]
    [InlineData(FormulaRoundingMode.Ceiling)]
    [InlineData(FormulaRoundingMode.Truncate)]
    [InlineData(FormulaRoundingMode.HalfUp)]
    [InlineData(FormulaRoundingMode.HalfEven)]
    public void EveryDeclaredRoundingModeIsDeterministicForIntegerValues(
        FormulaRoundingMode mode)
    {
        var result = Calculate(CreateDefinition(roundingMode: mode), ("input-alpha", 4));

        Assert.Equal(16, result.VisibleOutput);
        Assert.Equal(mode, result.Trace.Rounding.Mode);
    }

    [Theory]
    [MemberData(nameof(OverflowPrograms))]
    public void ArithmeticOverflowIsRejected(CheckedIntegerFormulaProgram program)
    {
        AssertError(
            CreateDefinition(program: program),
            FormulaCalculationErrorCodes.ArithmeticOverflow,
            ("input-alpha", 1));
    }

    public static TheoryData<CheckedIntegerFormulaProgram> OverflowPrograms =>
        new()
        {
            CreateSingleOperationProgram(
                CheckedIntegerOperation.Add,
                new FormulaLiteralOperand(long.MaxValue),
                new FormulaLiteralOperand(1)),
            CreateSingleOperationProgram(
                CheckedIntegerOperation.Subtract,
                new FormulaLiteralOperand(long.MinValue),
                new FormulaLiteralOperand(1)),
            CreateSingleOperationProgram(
                CheckedIntegerOperation.Multiply,
                new FormulaLiteralOperand(long.MaxValue),
                new FormulaLiteralOperand(2)),
        };

    [Fact]
    public void ForwardStepReferenceIsRejectedWithoutPartialTrace()
    {
        var program = new CheckedIntegerFormulaProgram(
        [
            new(
                "raw-output",
                CheckedIntegerOperation.Add,
                [
                    new FormulaStepOperand("future-step"),
                    new FormulaLiteralOperand(1),
                ]),
            new(
                "future-step",
                CheckedIntegerOperation.Constant,
                [new FormulaLiteralOperand(1)]),
            new(
                "visible-output",
                CheckedIntegerOperation.ApplyRounding,
                [new FormulaStepOperand("raw-output")]),
        ]);

        AssertError(
            CreateDefinition(
                program: program,
                traceStepIds: ["raw-output", "future-step", "visible-output"]),
            FormulaCalculationErrorCodes.ProgramInvalid,
            ("input-alpha", 1));
    }

    [Fact]
    public void TraceOrderMismatchIsRejected()
    {
        AssertError(
            CreateDefinition(
                traceStepIds:
                [
                    "constant",
                    "scaled",
                    "adjusted",
                    "raw-output",
                    "visible-output",
                ]),
            FormulaCalculationErrorCodes.ProgramInvalid,
            ("input-alpha", 4));
    }

    [Fact]
    public void UnsupportedProgramIsRejected()
    {
        AssertError(
            CreateDefinition(program: new SyntheticUnsupportedProgram()),
            FormulaCalculationErrorCodes.ProgramNotSupported,
            ("input-alpha", 4));
    }

    private static FormulaCalculationResult Calculate(
        FormulaDefinition definition,
        params (string Id, long Value)[] inputs) =>
        CheckedIntegerFormulaInterpreter.Calculate(
            definition,
            new FormulaCalculationRequest(
                new FormulaCalculationContext(
                    "class-synthetic",
                    "evolution-synthetic"),
                inputs.Select(input =>
                    new KeyValuePair<string, long>(input.Id, input.Value))));

    private static void AssertError(
        FormulaDefinition definition,
        string expectedCode,
        params (string Id, long Value)[] inputs)
    {
        var exception = Assert.Throws<FormulaCalculationException>(
            () => Calculate(definition, inputs));

        Assert.Equal(expectedCode, exception.Code);
    }

    private static FormulaDefinition CreateDefinition(
        FormulaStatus status = FormulaStatus.Published,
        IEnumerable<string>? evolutionIds = null,
        IEnumerable<string>? evidenceRefs = null,
        FormulaNumericBounds? inputBounds = null,
        FormulaRoundingMode roundingMode = FormulaRoundingMode.Truncate,
        FormulaProgram? program = null,
        IEnumerable<string>? traceStepIds = null)
    {
        program ??= CreateCompleteProgram();
        traceStepIds ??=
        [
            "constant",
            "adjusted",
            "scaled",
            "raw-output",
            "visible-output",
        ];

        return new FormulaDefinition(
            new FormulaReference("formula-synthetic", "0.1.0"),
            "ruleset-synthetic",
            status,
            FormulaConfidence.Unverified,
            new FormulaApplicability(
                "class-synthetic",
                evolutionIds ?? ["evolution-synthetic"]),
            [
                new FormulaInputDefinition(
                    "input-alpha",
                    FormulaNumericType.Signed32Bit,
                    "synthetic-unit",
                    inputBounds ??
                        new FormulaNumericBounds(
                            0,
                            true,
                            100,
                            true,
                            FormulaBoundsClassification.Technical),
                    SyntheticRangeError,
                    new FormulaInputSource(
                        FormulaInputSourceKind.ContextValue,
                        "synthetic-alpha")),
            ],
            new FormulaOutputDefinition("output-synthetic", "synthetic-unit"),
            program,
            new FormulaRoundingDefinition(
                roundingMode,
                "visible-output",
                decimalPlaces: 0),
            new FormulaTraceDefinition(
                traceStepIds,
                "raw-output",
                "visible-output"),
            evidenceRefs ?? ["evidence-synthetic"],
            ["conflict-synthetic"]);
    }

    private static CheckedIntegerFormulaProgram CreateCompleteProgram() =>
        new(
        [
            new(
                "constant",
                CheckedIntegerOperation.Constant,
                [new FormulaLiteralOperand(10)]),
            new(
                "adjusted",
                CheckedIntegerOperation.Subtract,
                [
                    new FormulaInputOperand("input-alpha"),
                    new FormulaLiteralOperand(1),
                ]),
            new(
                "scaled",
                CheckedIntegerOperation.Multiply,
                [
                    new FormulaStepOperand("adjusted"),
                    new FormulaLiteralOperand(2),
                ]),
            new(
                "raw-output",
                CheckedIntegerOperation.Add,
                [
                    new FormulaStepOperand("constant"),
                    new FormulaStepOperand("scaled"),
                ]),
            new(
                "visible-output",
                CheckedIntegerOperation.ApplyRounding,
                [new FormulaStepOperand("raw-output")]),
        ]);

    private static CheckedIntegerFormulaProgram CreateSingleOperationProgram(
        CheckedIntegerOperation operation,
        CheckedIntegerOperand left,
        CheckedIntegerOperand right) =>
        new(
        [
            new("raw-output", operation, [left, right]),
            new(
                "visible-output",
                CheckedIntegerOperation.ApplyRounding,
                [new FormulaStepOperand("raw-output")]),
        ]);

    private sealed class SyntheticUnsupportedProgram : FormulaProgram
    {
        public SyntheticUnsupportedProgram()
            : base("SYNTHETIC_UNSUPPORTED")
        {
        }
    }
}
