using System.Text.Json;
using MuOnline.BuildPlanner.Application.Formulas;
using MuOnline.BuildPlanner.Application.Progression;
using MuOnline.BuildPlanner.Application.Stats;
using MuOnline.BuildPlanner.Domain.Formulas;
using MuOnline.BuildPlanner.Domain.Progression;
using MuOnline.BuildPlanner.Domain.Stats;
using MuOnline.SchemaValidator;
using Xunit;

namespace MuOnline.BuildPlanner.Application.IntegrationTests;

public sealed class BuildCalculationTraceApplicationIntegrationTests
{
    private static readonly string CanonicalSnapshotRoot = FindCanonicalSnapshotRoot();
    private static readonly string RepositoryRoot = FindRepositoryRoot();

    [Fact]
    public void TraceExposesDeterministicOrderContextAndPositionsForEveryClassAndEvolution()
    {
        var progressionCatalog =
            new JsonProgressionRulesetSnapshotReader().Read(CanonicalSnapshotRoot);
        var formulaCatalog =
            new JsonExecutableFormulaSnapshotReader().Read(CanonicalSnapshotRoot);
        var useCase = new CalculateCharacterBuildUseCase(
            progressionCatalog,
            formulaCatalog);

        foreach (var characterClass in progressionCatalog.Classes)
        {
            foreach (var evolutionId in characterClass.EvolutionIds)
            {
                var evaluation = useCase.Execute(
                    new ProgressionPointBudgetRequest(
                        characterClass.Id,
                        evolutionId,
                        1,
                        []),
                    new ResetPointInputs(0, 0),
                    characterClass.StatIds.ToDictionary(
                        statId => statId,
                        _ => 0L,
                        StringComparer.Ordinal));
                var trace = BuildCalculationTraceFactory.Create(
                    evaluation,
                    $"trace-{characterClass.Id}");

                Assert.Equal("1.0.0", trace.SchemaVersion);
                Assert.Equal($"trace-{characterClass.Id}", trace.Id);
                Assert.Equal(evaluation.State.Budget.RulesetId, trace.RulesetId);
                Assert.Equal(characterClass.Id, trace.Context.CharacterClassId);
                Assert.Equal(evolutionId, trace.Context.EvolutionId);
                Assert.Equal(
                    evaluation.Formulas
                        .Select(item => item.Formula.Reference)
                        .Select(reference => (reference.Id, reference.Version)),
                    trace.Sequence
                        .Select(entry => (
                            entry.FormulaRef.Id,
                            entry.FormulaRef.Version)));
                Assert.Equal(
                    Enumerable.Range(0, evaluation.Formulas.Length),
                    trace.Sequence.Select(entry => (int)entry.Position));
            }
        }
    }

    [Fact]
    public void TraceOutputsParityWithBatchEvaluationEverywhere()
    {
        var progressionCatalog =
            new JsonProgressionRulesetSnapshotReader().Read(CanonicalSnapshotRoot);
        var formulaCatalog =
            new JsonExecutableFormulaSnapshotReader().Read(CanonicalSnapshotRoot);
        var useCase = new CalculateCharacterBuildUseCase(
            progressionCatalog,
            formulaCatalog);

        foreach (var characterClass in progressionCatalog.Classes)
        {
            foreach (var evolutionId in characterClass.EvolutionIds)
            {
                var evaluation = useCase.Execute(
                    new ProgressionPointBudgetRequest(
                        characterClass.Id,
                        evolutionId,
                        1,
                        []),
                    new ResetPointInputs(0, 0),
                    characterClass.StatIds.ToDictionary(
                        statId => statId,
                        _ => 0L,
                        StringComparer.Ordinal));
                var trace = BuildCalculationTraceFactory.Create(
                    evaluation,
                    $"trace-{characterClass.Id}");

                for (var index = 0; index < evaluation.Formulas.Length; index++)
                {
                    var entry = trace.Sequence[index];
                    var formulaEvaluation = evaluation.Formulas[index];
                    Assert.Equal(formulaEvaluation.Formula.Output.Id, entry.OutputId);
                    Assert.Equal(formulaEvaluation.Formula.Output.Unit, entry.OutputUnit);
                    Assert.Equal(
                        formulaEvaluation.Calculation.RawOutput,
                        entry.RawOutput);
                    Assert.Equal(
                        formulaEvaluation.Calculation.VisibleOutput,
                        entry.VisibleOutput);
                }
            }
        }
    }

    [Fact]
    public void TraceDependenciesReferenceOnlySequenceFormulasAndDeclaredInputs()
    {
        var progressionCatalog =
            new JsonProgressionRulesetSnapshotReader().Read(CanonicalSnapshotRoot);
        var formulaCatalog =
            new JsonExecutableFormulaSnapshotReader().Read(CanonicalSnapshotRoot);
        var useCase = new CalculateCharacterBuildUseCase(
            progressionCatalog,
            formulaCatalog);

        foreach (var characterClass in progressionCatalog.Classes)
        {
            foreach (var evolutionId in characterClass.EvolutionIds)
            {
                var evaluation = useCase.Execute(
                    new ProgressionPointBudgetRequest(
                        characterClass.Id,
                        evolutionId,
                        1,
                        []),
                    new ResetPointInputs(0, 0),
                    characterClass.StatIds.ToDictionary(
                        statId => statId,
                        _ => 0L,
                        StringComparer.Ordinal));
                var trace = BuildCalculationTraceFactory.Create(
                    evaluation,
                    $"trace-{characterClass.Id}");
                var sequenceReferences = trace.Sequence
                    .Select(entry =>
                        $"{entry.FormulaRef.Id}@{entry.FormulaRef.Version}")
                    .ToHashSet(StringComparer.Ordinal);

                for (var index = 0; index < trace.Sequence.Count; index++)
                {
                    var entry = trace.Sequence[index];
                    var formulaEvaluation = evaluation.Formulas[index];
                    var declaredDependencies =
                        new List<(string InputId, FormulaReference SourceFormula, FormulaOutputStage OutputStage)>();
                    foreach (var input in formulaEvaluation.Formula.Inputs.Where(
                                 input =>
                                     input.Source.Kind ==
                                     FormulaInputSourceKind.FormulaOutput))
                    {
                        declaredDependencies.Add((
                            input.Id,
                            input.Source.FormulaReference
                                ?? throw new InvalidOperationException(
                                    $"Formula input '{input.Id}' has no dependency reference."),
                            input.Source.OutputStage
                                ?? throw new InvalidOperationException(
                                    $"Formula input '{input.Id}' has no dependency output stage.")));
                    }

                    Assert.Equal(declaredDependencies.Count, entry.Dependencies.Count);
                    var declaredByName = declaredDependencies.ToDictionary(
                        item => item.InputId,
                        StringComparer.Ordinal);
                    var dependencyKeys = new HashSet<string>(StringComparer.Ordinal);
                    foreach (var dependency in entry.Dependencies)
                    {
                        var sourceKey =
                            $"{dependency.SourceFormulaRef.Id}@{dependency.SourceFormulaRef.Version}";
                        Assert.Contains(sourceKey, sequenceReferences);
                        Assert.True(
                            declaredByName.ContainsKey(dependency.InputId),
                            $"Trace dependency '{dependency.InputId}' is not a declared formula-output input.");
                        var declared = declaredByName[dependency.InputId];
                        Assert.Equal(
                            declared.SourceFormula.Id,
                            dependency.SourceFormulaRef.Id);
                        Assert.Equal(
                            declared.SourceFormula.Version,
                            dependency.SourceFormulaRef.Version);
                        Assert.Equal(
                            declared.OutputStage.ToString().ToUpperInvariant(),
                            dependency.OutputStage);
                        Assert.True(dependencyKeys.Add(
                            $"{dependency.InputId}@{sourceKey}"),
                            $"Duplicate trace dependency '{dependency.InputId}@{sourceKey}'.");
                    }
                }
            }
        }
    }

    [Fact]
    public void SerializedTraceConformsToBuildCalculationTraceContractEverywhere()
    {
        var progressionCatalog =
            new JsonProgressionRulesetSnapshotReader().Read(CanonicalSnapshotRoot);
        var formulaCatalog =
            new JsonExecutableFormulaSnapshotReader().Read(CanonicalSnapshotRoot);
        var useCase = new CalculateCharacterBuildUseCase(
            progressionCatalog,
            formulaCatalog);

        foreach (var characterClass in progressionCatalog.Classes)
        {
            foreach (var evolutionId in characterClass.EvolutionIds)
            {
                var evaluation = useCase.Execute(
                    new ProgressionPointBudgetRequest(
                        characterClass.Id,
                        evolutionId,
                        1,
                        []),
                    new ResetPointInputs(0, 0),
                    characterClass.StatIds.ToDictionary(
                        statId => statId,
                        _ => 0L,
                        StringComparer.Ordinal));
                var trace = BuildCalculationTraceFactory.Create(
                    evaluation,
                    $"trace-{characterClass.Id}");
                using var document = JsonDocument.Parse(
                    JsonSerializer.Serialize(trace));
                var element = document.RootElement;

                Assert.Equal(
                    "1.0.0",
                    element.GetProperty("schemaVersion").GetString());
                Assert.True(SchemaContractValidator.ValidateInstance(
                    RepositoryRoot,
                    "build-calculation-trace",
                    element),
                    $"The emitted macro trace did not conform for class " +
                    $"'{characterClass.Id}' evolution '{evolutionId}'.");
            }
        }
    }

    private static string FindCanonicalSnapshotRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine(
                current.FullName,
                "packages",
                "rulesets",
                "mu-s4-global-reference",
                "v1");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate the canonical Season 4 ruleset snapshot.");
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "MUOnline.BuildPlanner.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the repository root.");
    }
}