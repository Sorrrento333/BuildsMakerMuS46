using MuOnline.BuildPlanner.Application.Formulas;
using MuOnline.BuildPlanner.Application.Progression;
using MuOnline.BuildPlanner.Application.Stats;
using MuOnline.BuildPlanner.Domain.Formulas;
using MuOnline.BuildPlanner.Domain.Progression;
using MuOnline.BuildPlanner.Domain.Stats;
using Xunit;

namespace MuOnline.BuildPlanner.Application.IntegrationTests;

public sealed class CharacterBuildApplicationIntegrationTests
{
    private static readonly string CanonicalSnapshotRoot = FindCanonicalSnapshotRoot();

    [Fact]
    public void BatchCoversExactlyApplicableFormulasForEveryClassAndEvolution()
    {
        var progressionCatalog =
            new JsonProgressionRulesetSnapshotReader().Read(CanonicalSnapshotRoot);
        var formulaCatalog =
            new JsonExecutableFormulaSnapshotReader().Read(CanonicalSnapshotRoot);
        var useCase = new CalculateCharacterBuildUseCase(
            progressionCatalog,
            formulaCatalog);
        var expectedReferences = formulaCatalog.Formulas.Select(
            formula => formula.Reference).ToHashSet();

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
                var expected = formulaCatalog.Formulas
                    .Where(formula =>
                        formula.Applicability.CharacterClassId == characterClass.Id &&
                        formula.Applicability.EvolutionIds.Contains(evolutionId))
                    .Select(formula => formula.Reference)
                    .OrderBy(reference => reference.Id, StringComparer.Ordinal)
                    .ThenBy(reference => reference.Version, StringComparer.Ordinal)
                    .ToArray();

                Assert.NotEmpty(expected);
                Assert.Equal(evolutionId, evaluation.State.ProgressionRequest.EvolutionId);
                Assert.Equal(characterClass.Id, evaluation.State.CharacterClass.Id);
                Assert.Equal(
                    expected,
                    evaluation.Formulas.Select(
                        item => item.Formula.Reference).ToArray());
                Assert.All(
                    evaluation.Formulas,
                    item => Assert.Contains(
                        item.Formula.Reference,
                        expectedReferences));
            }
        }
    }

    [Fact]
    public void BatchOutputsParityWithSingleFormulaEvaluationEverywhere()
    {
        var progressionCatalog =
            new JsonProgressionRulesetSnapshotReader().Read(CanonicalSnapshotRoot);
        var formulaCatalog =
            new JsonExecutableFormulaSnapshotReader().Read(CanonicalSnapshotRoot);
        var batchUseCase = new CalculateCharacterBuildUseCase(
            progressionCatalog,
            formulaCatalog);
        var singleUseCase = new CalculateCharacterFormulaUseCase(
            progressionCatalog,
            formulaCatalog);

        foreach (var characterClass in progressionCatalog.Classes)
        {
            foreach (var evolutionId in characterClass.EvolutionIds)
            {
                var allocations = characterClass.StatIds.ToDictionary(
                    statId => statId,
                    _ => 0L,
                    StringComparer.Ordinal);
                var request = new ProgressionPointBudgetRequest(
                    characterClass.Id,
                    evolutionId,
                    1,
                    []);
                var resetInputs = new ResetPointInputs(0, 0);

                var evaluation = batchUseCase.Execute(
                    request,
                    resetInputs,
                    allocations);
                foreach (var formulaEvaluation in evaluation.Formulas)
                {
                    var single = singleUseCase.Execute(
                        formulaEvaluation.Formula.Reference,
                        request,
                        resetInputs,
                        allocations);

                    Assert.Equal(
                        single.Formula.RawOutput,
                        formulaEvaluation.Calculation.RawOutput);
                    Assert.Equal(
                        single.Formula.VisibleOutput,
                        formulaEvaluation.Calculation.VisibleOutput);
                    Assert.Equal(
                        single.Formula.Trace.Steps,
                        formulaEvaluation.Calculation.Trace.Steps);
                    Assert.Equal(
                        single.ContextTrace.Select(ProjectContextTraceEntry),
                        formulaEvaluation.ContextTrace.Select(ProjectContextTraceEntry));
                    Assert.Equal(
                        single.DependencyTrace.Select(ProjectDependencyTraceEntry),
                        formulaEvaluation.DependencyTrace.Select(
                            ProjectDependencyTraceEntry));
                }
            }
        }
    }

    [Fact]
    public void BatchKeepsNestedDependencyTraceAndSharedEvaluationConsistent()
    {
        var progressionCatalog =
            new JsonProgressionRulesetSnapshotReader().Read(CanonicalSnapshotRoot);
        var formulaCatalog =
            new JsonExecutableFormulaSnapshotReader().Read(CanonicalSnapshotRoot);
        var batchUseCase = new CalculateCharacterBuildUseCase(
            progressionCatalog,
            formulaCatalog);
        var singleUseCase = new CalculateCharacterFormulaUseCase(
            progressionCatalog,
            formulaCatalog);
        var consumerReference = formulaCatalog.Formulas
            .Single(formula =>
                formula.Reference.Id == "formula-mana-regen-dark-wizard")
            .Reference;
        var formula = formulaCatalog.Resolve(consumerReference);
        var sourceReference = formula.Inputs
            .Single(input => input.Source.Kind == FormulaInputSourceKind.FormulaOutput)
            .Source.FormulaReference;
        var characterClass = progressionCatalog.Classes.Single(
            item => item.Id == formula.Applicability.CharacterClassId);
        var request = new ProgressionPointBudgetRequest(
            characterClass.Id,
            formula.Applicability.EvolutionIds.First(),
            1,
            []);
        var resetInputs = new ResetPointInputs(1, 1);
        var allocations = characterClass.StatIds.ToDictionary(
            statId => statId,
            _ => 0L,
            StringComparer.Ordinal);

        var build = batchUseCase.Execute(request, resetInputs, allocations);
        var batchEvaluation = build.Formulas.Single(
            item => item.Formula.Reference == consumerReference);
        var single = singleUseCase.Execute(
            consumerReference,
            request,
            resetInputs,
            allocations);

        var directEntry = batchEvaluation.DependencyTrace.Single(
            item => item.ConsumerFormulaReference == consumerReference);
        Assert.Equal(sourceReference, directEntry.FormulaReference);
        Assert.Equal(
            single.DependencyTrace.Single(
                    item => item.ConsumerFormulaReference == consumerReference)
                .ResolvedValue,
            directEntry.ResolvedValue);
        Assert.Equal(
            single.DependencyTrace.Length,
            batchEvaluation.DependencyTrace.Length);
        Assert.Equal(
            build.Formulas.Single(
                    item => item.Formula.Reference == sourceReference)
                .Calculation.VisibleOutput,
            directEntry.ResolvedValue);
    }

    [Fact]
    public void BatchOrderingIsDeterministicByIdThenVersion()
    {
        var progressionCatalog =
            new JsonProgressionRulesetSnapshotReader().Read(CanonicalSnapshotRoot);
        var formulaCatalog =
            new JsonExecutableFormulaSnapshotReader().Read(CanonicalSnapshotRoot);
        var useCase = new CalculateCharacterBuildUseCase(
            progressionCatalog,
            formulaCatalog);
        var characterClass = progressionCatalog.Classes[0];
        var allocations = characterClass.StatIds.ToDictionary(
            statId => statId,
            _ => 0L,
            StringComparer.Ordinal);

        var references = useCase.Execute(
                new ProgressionPointBudgetRequest(
                    characterClass.Id,
                    characterClass.EvolutionIds.First(),
                    1,
                    []),
                new ResetPointInputs(0, 0),
                allocations)
            .Formulas
            .Select(item => item.Formula.Reference)
            .ToArray();

        Assert.Equal(
            references.Distinct().Count(),
            references.Length);
        Assert.Equal(
            references.OrderBy(reference => reference.Id, StringComparer.Ordinal)
                .ThenBy(reference => reference.Version, StringComparer.Ordinal),
            references);
    }

    [Fact]
    public void BatchFailsClosedOnInvalidLevelAndNegativeAllocation()
    {
        var progressionCatalog =
            new JsonProgressionRulesetSnapshotReader().Read(CanonicalSnapshotRoot);
        var formulaCatalog =
            new JsonExecutableFormulaSnapshotReader().Read(CanonicalSnapshotRoot);
        var useCase = new CalculateCharacterBuildUseCase(
            progressionCatalog,
            formulaCatalog);
        var characterClass = progressionCatalog.Classes[0];
        var allocations = characterClass.StatIds.ToDictionary(
            statId => statId,
            _ => 0L,
            StringComparer.Ordinal);
        var evolutionId = characterClass.EvolutionIds.First();

        var levelException = Assert.Throws<ProgressionPointBudgetException>(
            () => useCase.Execute(
                new ProgressionPointBudgetRequest(
                    characterClass.Id,
                    evolutionId,
                    0,
                    []),
                new ResetPointInputs(0, 0),
                allocations));
        Assert.Equal(
            ProgressionPointBudgetErrorCodes.LevelOutOfRange,
            levelException.Code);

        var negativeAllocations = new Dictionary<string, long>(allocations)
        {
            [characterClass.StatIds.First()] = -1,
        };
        var allocationException = Assert.Throws<StatDistributionException>(
            () => useCase.Execute(
                new ProgressionPointBudgetRequest(
                    characterClass.Id,
                    evolutionId,
                    1,
                    []),
                new ResetPointInputs(0, 0),
                negativeAllocations));
        Assert.Equal(
            StatDistributionErrorCodes.AllocationNegative,
            allocationException.Code);
    }

    [Fact]
    public void BatchRejectsCatalogWithoutAnyApplicableFormula()
    {
        var progressionCatalog =
            new JsonProgressionRulesetSnapshotReader().Read(CanonicalSnapshotRoot);
        var formulaCatalog =
            new JsonExecutableFormulaSnapshotReader().Read(CanonicalSnapshotRoot);
        var darkWizardFormula = formulaCatalog.Formulas.Single(
            formula => formula.Reference.Id == "formula-hp-dark-wizard");
        var singleFormulaCatalog = new ExecutableFormulaCatalog(
            progressionCatalog.RulesetId,
            [darkWizardFormula]);
        var inapplicableClass = progressionCatalog.Classes.First(
            item => item.Id != darkWizardFormula.Applicability.CharacterClassId);
        var useCase = new CalculateCharacterBuildUseCase(
            progressionCatalog,
            singleFormulaCatalog);
        var exception = Assert.Throws<FormulaContextException>(
            () => useCase.Execute(
                new ProgressionPointBudgetRequest(
                    inapplicableClass.Id,
                    inapplicableClass.EvolutionIds.First(),
                    1,
                    []),
                new ResetPointInputs(0, 0),
                inapplicableClass.StatIds.ToDictionary(
                    statId => statId,
                    _ => 0L,
                    StringComparer.Ordinal)));
        Assert.Equal(
            FormulaContextErrorCodes.NoApplicableFormula,
            exception.Code);
    }

    private static (
        string InputId,
        decimal ResolvedValue,
        FormulaContextResolutionKind Kind)
        ProjectContextTraceEntry(FormulaContextResolutionTraceEntry entry) =>
        (entry.InputId, entry.ResolvedValue, entry.Kind);

    private static (
        FormulaReference ConsumerFormulaReference,
        string InputId,
        FormulaReference FormulaReference,
        FormulaOutputStage OutputStage,
        decimal ResolvedValue)
        ProjectDependencyTraceEntry(FormulaDependencyResolutionTraceEntry entry) =>
        (
            entry.ConsumerFormulaReference,
            entry.InputId,
            entry.FormulaReference,
            entry.OutputStage,
            entry.ResolvedValue);

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
}