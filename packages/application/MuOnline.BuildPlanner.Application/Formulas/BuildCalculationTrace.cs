using System.Text.Json;
using System.Text.Json.Serialization;
using MuOnline.BuildPlanner.Domain.Formulas;

namespace MuOnline.BuildPlanner.Application.Formulas;

public sealed record BuildCalculationTrace(
    [property: JsonPropertyName("schemaVersion")] string SchemaVersion,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("rulesetId")] string RulesetId,
    [property: JsonPropertyName("context")] BuildCalculationTraceContext Context,
    [property: JsonPropertyName("sequence")] IReadOnlyList<BuildCalculationTraceEntry> Sequence)
{
    public const string CurrentSchemaVersion = "1.0.0";
}

public sealed record BuildCalculationTraceContext(
    [property: JsonPropertyName("characterClassId")] string CharacterClassId,
    [property: JsonPropertyName("evolutionId")] string EvolutionId);

public sealed record BuildCalculationTraceEntry(
    [property: JsonPropertyName("position")] int Position,
    [property: JsonPropertyName("formulaRef")] BuildCalculationTraceVersionedRef FormulaRef,
    [property: JsonPropertyName("outputId")] string OutputId,
    [property: JsonPropertyName("outputUnit")] string OutputUnit,
    [property: JsonPropertyName("rawOutput")] decimal RawOutput,
    [property: JsonPropertyName("visibleOutput")] long VisibleOutput,
    [property: JsonPropertyName("dependencies")] IReadOnlyList<BuildCalculationTraceDependency> Dependencies);

public sealed record BuildCalculationTraceDependency(
    [property: JsonPropertyName("inputId")] string InputId,
    [property: JsonPropertyName("sourceFormulaRef")] BuildCalculationTraceVersionedRef SourceFormulaRef,
    [property: JsonPropertyName("outputStage")] string OutputStage);

public sealed record BuildCalculationTraceVersionedRef(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("version")] string Version);

public static class BuildCalculationTraceFactory
{
    public static BuildCalculationTrace Create(
        CharacterBuildEvaluation evaluation,
        string id)
    {
        ArgumentNullException.ThrowIfNull(evaluation);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var sequence = evaluation.Formulas
            .Select((item, position) => new BuildCalculationTraceEntry(
                position,
                new BuildCalculationTraceVersionedRef(
                    item.Formula.Reference.Id,
                    item.Formula.Reference.Version),
                item.Formula.Output.Id,
                item.Formula.Output.Unit,
                item.Calculation.RawOutput,
                item.Calculation.VisibleOutput,
                item.Formula.Inputs
                    .Where(input => input.Source.Kind == FormulaInputSourceKind.FormulaOutput)
                    .Select(input => new BuildCalculationTraceDependency(
                        input.Id,
                        new BuildCalculationTraceVersionedRef(
                            InputDependencyReference(input).Id,
                            InputDependencyReference(input).Version),
                        FormatOutputStage(InputDependencyStage(input))))
                    .ToArray()))
            .ToArray();

        return new BuildCalculationTrace(
            BuildCalculationTrace.CurrentSchemaVersion,
            id,
            evaluation.State.Budget.RulesetId,
            new BuildCalculationTraceContext(
                evaluation.State.ProgressionRequest.ClassId,
                evaluation.State.ProgressionRequest.EvolutionId),
            sequence);
    }

    public static JsonElement SerializeToElement(
        CharacterBuildEvaluation evaluation,
        string id) =>
        JsonSerializer.SerializeToElement(Create(evaluation, id));

    public static string Serialize(
        CharacterBuildEvaluation evaluation,
        string id) =>
        JsonSerializer.Serialize(Create(evaluation, id));

    private static FormulaReference InputDependencyReference(
        FormulaInputDefinition input) =>
        input.Source.FormulaReference
            ?? throw Error(input, "has no dependency reference.");

    private static FormulaOutputStage InputDependencyStage(
        FormulaInputDefinition input) =>
        input.Source.OutputStage
            ?? throw Error(input, "has no dependency output stage.");

    private static FormulaContextException Error(
        FormulaInputDefinition input,
        string message) =>
        new(
            FormulaContextErrorCodes.DependencyIncoherent,
            $"Formula input '{input.Id}' {message}");

    private static string FormatOutputStage(FormulaOutputStage stage) =>
        stage == FormulaOutputStage.Raw ? "RAW" : "VISIBLE";
}