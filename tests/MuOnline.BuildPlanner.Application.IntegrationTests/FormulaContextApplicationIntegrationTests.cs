using System.Text.Json;
using MuOnline.BuildPlanner.Application.Formulas;
using MuOnline.BuildPlanner.Application.Progression;
using MuOnline.BuildPlanner.Application.Stats;
using MuOnline.BuildPlanner.Domain.Formulas;
using MuOnline.BuildPlanner.Domain.Progression;
using MuOnline.BuildPlanner.Domain.Stats;
using Xunit;

namespace MuOnline.BuildPlanner.Application.IntegrationTests;

public sealed class FormulaContextApplicationIntegrationTests
{
    private static readonly string CanonicalSnapshotRoot = FindCanonicalSnapshotRoot();

    [Fact]
    public void ReadersPreserveCanonicalBaseStatsEvidenceAndContextValueIds()
    {
        var progressionCatalog =
            new JsonProgressionRulesetSnapshotReader().Read(CanonicalSnapshotRoot);
        var formulaCatalog =
            new JsonExecutableFormulaSnapshotReader().Read(CanonicalSnapshotRoot);

        foreach (var path in Directory.GetFiles(
                     Path.Combine(CanonicalSnapshotRoot, "character-classes"),
                     "*.json"))
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            var root = document.RootElement;
            var characterClass = progressionCatalog.Classes.Single(
                item => item.Id == RequiredString(root, "id"));

            foreach (var stat in root.GetProperty("stats").EnumerateObject())
            {
                var materialized = characterClass.BaseStats[stat.Name];
                Assert.Equal(
                    stat.Value.GetProperty("baseValue").GetInt64(),
                    materialized.BaseValue);
                Assert.Equal(
                    StringArray(stat.Value, "evidenceRefs"),
                    materialized.EvidenceRefs);
            }
        }

        foreach (var definition in formulaCatalog.Formulas)
        {
            var executablePath = FindFormulaPath(definition.Reference);
            using var formulaDocument = JsonDocument.Parse(
                File.ReadAllText(executablePath));
            var expectedSources = formulaDocument.RootElement.GetProperty("inputs")
                .EnumerateArray()
                .Where(input =>
                    RequiredString(input.GetProperty("source"), "kind") ==
                    "CONTEXT_VALUE")
                .ToDictionary(
                    input => RequiredString(input, "id"),
                    input => RequiredString(input.GetProperty("source"), "valueId"),
                    StringComparer.Ordinal);

            Assert.Equal(
                expectedSources,
                definition.Inputs
                    .Where(input =>
                        input.Source.Kind ==
                        FormulaInputSourceKind.ContextValue)
                    .ToDictionary(
                    input => input.Id,
                    input => input.Source.ValueId!,
                    StringComparer.Ordinal));
        }
    }

    [Fact]
    public void ProductCompositionReproducesAllCanonicalPositiveCases()
    {
        var progressionCatalog =
            new JsonProgressionRulesetSnapshotReader().Read(CanonicalSnapshotRoot);
        var formulaCatalog =
            new JsonExecutableFormulaSnapshotReader().Read(CanonicalSnapshotRoot);
        var useCase = new CalculateCharacterFormulaUseCase(
            progressionCatalog,
            formulaCatalog);

        foreach (var referenceCase in LoadPositiveCases())
        {
            var formula = formulaCatalog.Resolve(referenceCase.Reference);
            var characterClass = progressionCatalog.Classes.Single(
                item => item.Id == referenceCase.CharacterClassId);
            var levelInput = formula.Inputs.SingleOrDefault(
                input => input.Source.ValueId == "character-level");
            var allocations = characterClass.StatIds.ToDictionary(
                statId => statId,
                _ => 0L,
                StringComparer.Ordinal);
            foreach (var statInput in formula.Inputs.Where(
                         input =>
                             input.Source.Kind ==
                                 FormulaInputSourceKind.ContextValue &&
                             input.Source.ValueId!.StartsWith(
                             "resolved-",
                             StringComparison.Ordinal)))
            {
                var statId = statInput.Source.ValueId!["resolved-".Length..];
                allocations[statId] = checked((long)(
                    referenceCase.Inputs[statInput.Id] -
                    characterClass.BaseStats[statId].BaseValue));
            }

            var result = useCase.Execute(
                referenceCase.Reference,
                new ProgressionPointBudgetRequest(
                    referenceCase.CharacterClassId,
                    referenceCase.EvolutionId,
                    levelInput is null
                        ? 1
                        : checked((int)referenceCase.Inputs[levelInput.Id]),
                    []),
                new ResetPointInputs(
                    1,
                    allocations.Values.Aggregate(
                        0L,
                        (sum, value) => checked(sum + value))),
                allocations);

            Assert.Equal(referenceCase.RawOutput, result.Formula.RawOutput);
            Assert.Equal(referenceCase.VisibleOutput, result.Formula.VisibleOutput);
            Assert.Equal(referenceCase.Steps, result.Formula.Trace.Steps);
            var resolvedInputs = result.ContextTrace
                .Select(item => KeyValuePair.Create(
                    item.InputId,
                    (decimal)item.ResolvedValue))
                .Concat(result.DependencyTrace.Select(
                    item => KeyValuePair.Create(item.InputId, item.ResolvedValue)))
                .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
            Assert.Equal(
                referenceCase.Inputs.OrderBy(item => item.Key, StringComparer.Ordinal),
                resolvedInputs.OrderBy(item => item.Key, StringComparer.Ordinal));
            Assert.All(
                result.ContextTrace,
                item => Assert.Equal(
                    referenceCase.Inputs[item.InputId],
                    item.ResolvedValue));
            Assert.All(
                result.DependencyTrace,
                item => Assert.Equal(
                    referenceCase.Inputs[item.InputId],
                    item.ResolvedValue));
        }
    }

    [Fact]
    public void ContextResolverFailsClosedForUnknownContextSource()
    {
        var fixture = CreateCanonicalState();
        var formula = fixture.Formula;
        var unknown = CopyFormula(
            formula,
            formula.Inputs.Select(input =>
                input.Id == formula.Inputs[0].Id
                    ? CopyInput(
                        input,
                        new FormulaInputSource(
                            FormulaInputSourceKind.ContextValue,
                            "unknown-context-value"))
                    : input));
        var unknownException = Assert.Throws<FormulaContextException>(
            () => FormulaContextValueResolver.Resolve(unknown, fixture.State));
        Assert.Equal(
            FormulaContextErrorCodes.ValueNotResolvable,
            unknownException.Code);

    }

    [Fact]
    public void ProductCompositionRejectsFormulaStateMismatch()
    {
        var progressionCatalog =
            new JsonProgressionRulesetSnapshotReader().Read(CanonicalSnapshotRoot);
        var formulaCatalog =
            new JsonExecutableFormulaSnapshotReader().Read(CanonicalSnapshotRoot);
        var formula = FindDarkWizardFormula(formulaCatalog);
        var otherClass = progressionCatalog.Classes.First(
            item => item.Id != formula.Applicability.CharacterClassId);

        var exception = Assert.Throws<FormulaContextException>(
            () => new CalculateCharacterFormulaUseCase(
                    progressionCatalog,
                    formulaCatalog)
                .Execute(
                    formula.Reference,
                    new ProgressionPointBudgetRequest(
                        otherClass.Id,
                        otherClass.EvolutionIds.First(),
                        1,
                        []),
                    new ResetPointInputs(0, 0),
                    otherClass.StatIds.ToDictionary(
                        statId => statId,
                        _ => 0L,
                        StringComparer.Ordinal)));

        Assert.Equal(FormulaContextErrorCodes.StateMismatch, exception.Code);
    }

    [Fact]
    public void ProductCompositionSelectsRawAndVisibleDependencyOutputsExactly()
    {
        var progressionCatalog =
            new JsonProgressionRulesetSnapshotReader().Read(CanonicalSnapshotRoot);
        var characterClass = progressionCatalog.Classes.Single(
            item => item.Id == "class-dark-wizard");
        var evolutionId = characterClass.EvolutionIds.First();
        var sourceReference = new FormulaReference(
            "formula-synthetic-dependency-source",
            "1.0.0");
        var consumerReference = new FormulaReference(
            "formula-synthetic-dependency-consumer",
            "1.0.0");
        var applicability = new FormulaApplicability(
            characterClass.Id,
            characterClass.EvolutionIds);
        var source = CreateSyntheticSourceFormula(
            sourceReference,
            applicability);
        var consumer = CreateSyntheticConsumerFormula(
            consumerReference,
            sourceReference,
            applicability);
        var useCase = new CalculateCharacterFormulaUseCase(
            progressionCatalog,
            new ExecutableFormulaCatalog(
                progressionCatalog.RulesetId,
                [source, consumer]));
        var allocations = characterClass.StatIds.ToDictionary(
            statId => statId,
            _ => 0L,
            StringComparer.Ordinal);
        allocations["agility"] = 1;

        var result = useCase.Execute(
            consumerReference,
            new ProgressionPointBudgetRequest(
                characterClass.Id,
                evolutionId,
                1,
                []),
            new ResetPointInputs(1, 1),
            allocations);

        Assert.Equal(18.5m, result.Formula.RawOutput);
        Assert.Equal(18, result.Formula.VisibleOutput);
        Assert.Collection(
            result.DependencyTrace,
            raw =>
            {
                Assert.Equal(consumerReference, raw.ConsumerFormulaReference);
                Assert.Equal("raw-dependency", raw.InputId);
                Assert.Equal(FormulaOutputStage.Raw, raw.OutputStage);
                Assert.Equal(9.5m, raw.ResolvedValue);
                Assert.Equal(9.5m, raw.FormulaTrace.RawOutput);
                Assert.Equal(9, raw.FormulaTrace.VisibleOutput);
                Assert.Single(raw.ContextTrace);
                Assert.Equal("agility", raw.ContextTrace[0].StatId);
                Assert.Equal(19, raw.ContextTrace[0].ResolvedValue);
            },
            visible =>
            {
                Assert.Equal("visible-dependency", visible.InputId);
                Assert.Equal(FormulaOutputStage.Visible, visible.OutputStage);
                Assert.Equal(9m, visible.ResolvedValue);
                Assert.Equal(sourceReference, visible.FormulaReference);
            });
        Assert.Empty(result.ContextTrace);
    }

    [Fact]
    public void ContextResolverDistinguishesMissingBaseAndAllocation()
    {
        var fixture = CreateCanonicalState();
        var statInput = fixture.Formula.Inputs.Single(
            input =>
                input.Source.Kind == FormulaInputSourceKind.ContextValue &&
                input.Source.ValueId!.StartsWith(
                "resolved-",
                StringComparison.Ordinal));
        var statId = statInput.Source.ValueId!["resolved-".Length..];
        var classWithoutBase = new CharacterProgressionDefinition(
            fixture.State.CharacterClass.Id,
            fixture.State.CharacterClass.RulesetId,
            fixture.State.CharacterClass.StatIds,
            fixture.State.CharacterClass.EvolutionIds,
            fixture.State.CharacterClass.ProgressionRuleRefs,
            fixture.State.CharacterClass.BaseStats.Values.Where(
                stat => stat.StatId != statId));
        var stateWithoutBase = new ResolvedCharacterState(
            fixture.State.ProgressionRequest,
            fixture.State.Budget,
            fixture.State.Distribution,
            classWithoutBase);
        var baseException = Assert.Throws<FormulaContextException>(
            () => FormulaContextValueResolver.Resolve(
                fixture.Formula,
                stateWithoutBase));
        Assert.Equal(
            FormulaContextErrorCodes.BaseStatMissing,
            baseException.Code);

        var allocationsWithoutStat = fixture.State.Distribution.Allocations
            .Where(item => item.Key != statId)
            .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
        var distributionWithoutAllocation = fixture.State.Distribution with
        {
            Allocations = allocationsWithoutStat,
        };
        var stateWithoutAllocation = new ResolvedCharacterState(
            fixture.State.ProgressionRequest,
            fixture.State.Budget,
            distributionWithoutAllocation,
            fixture.State.CharacterClass);
        var allocationException = Assert.Throws<FormulaContextException>(
            () => FormulaContextValueResolver.Resolve(
                fixture.Formula,
                stateWithoutAllocation));
        Assert.Equal(
            FormulaContextErrorCodes.AllocationMissing,
            allocationException.Code);
    }

    [Fact]
    public void ProductCompositionReportsResolvedStatOverflow()
    {
        var progressionCatalog =
            new JsonProgressionRulesetSnapshotReader().Read(CanonicalSnapshotRoot);
        var formulaCatalog =
            new JsonExecutableFormulaSnapshotReader().Read(CanonicalSnapshotRoot);
        var formula = FindDarkWizardFormula(formulaCatalog);
        var characterClass = progressionCatalog.Classes.Single(
            item => item.Id == formula.Applicability.CharacterClassId);
        var allocations = characterClass.StatIds.ToDictionary(
            statId => statId,
            _ => 0L,
            StringComparer.Ordinal);
        var statInput = formula.Inputs.Single(
            input =>
                input.Source.Kind == FormulaInputSourceKind.ContextValue &&
                input.Source.ValueId!.StartsWith(
                "resolved-",
                StringComparison.Ordinal));
        allocations[statInput.Source.ValueId!["resolved-".Length..]] = long.MaxValue;

        var exception = Assert.Throws<FormulaContextException>(
            () => new CalculateCharacterFormulaUseCase(
                    progressionCatalog,
                    formulaCatalog)
                .Execute(
                    formula.Reference,
                    new ProgressionPointBudgetRequest(
                        characterClass.Id,
                        formula.Applicability.EvolutionIds.First(),
                        1,
                        []),
                    new ResetPointInputs(1, long.MaxValue),
                    allocations));

        Assert.Equal(
            FormulaContextErrorCodes.ArithmeticOverflow,
            exception.Code);
    }

    [Fact]
    public void ProductCompositionRejectsInvalidOriginInputsBeforeContextResolution()
    {
        var progressionCatalog =
            new JsonProgressionRulesetSnapshotReader().Read(CanonicalSnapshotRoot);
        var formulaCatalog =
            new JsonExecutableFormulaSnapshotReader().Read(CanonicalSnapshotRoot);
        var formula = FindDarkWizardFormula(formulaCatalog);
        var characterClass = progressionCatalog.Classes.Single(
            item => item.Id == formula.Applicability.CharacterClassId);
        var allocations = characterClass.StatIds.ToDictionary(
            statId => statId,
            _ => 0L,
            StringComparer.Ordinal);
        var useCase = new CalculateCharacterFormulaUseCase(
            progressionCatalog,
            formulaCatalog);

        var progressionException = Assert.Throws<ProgressionPointBudgetException>(
            () => useCase.Execute(
                formula.Reference,
                new ProgressionPointBudgetRequest(
                    characterClass.Id,
                    formula.Applicability.EvolutionIds.First(),
                    0,
                    []),
                new ResetPointInputs(0, 0),
                allocations));
        Assert.Equal(
            ProgressionPointBudgetErrorCodes.LevelOutOfRange,
            progressionException.Code);

        allocations[characterClass.StatIds.First()] = -1;
        var distributionException = Assert.Throws<StatDistributionException>(
            () => useCase.Execute(
                formula.Reference,
                new ProgressionPointBudgetRequest(
                    characterClass.Id,
                    formula.Applicability.EvolutionIds.First(),
                    1,
                    []),
                new ResetPointInputs(0, 0),
                allocations));
        Assert.Equal(
            StatDistributionErrorCodes.AllocationNegative,
            distributionException.Code);
    }

    [Fact]
    public void ResolvedStateAndTraceDoNotObserveLaterAllocationMutation()
    {
        var progressionCatalog =
            new JsonProgressionRulesetSnapshotReader().Read(CanonicalSnapshotRoot);
        var formulaCatalog =
            new JsonExecutableFormulaSnapshotReader().Read(CanonicalSnapshotRoot);
        var formula = FindDarkWizardFormula(formulaCatalog);
        var characterClass = progressionCatalog.Classes.Single(
            item => item.Id == formula.Applicability.CharacterClassId);
        var allocations = characterClass.StatIds.ToDictionary(
            statId => statId,
            _ => 0L,
            StringComparer.Ordinal);
        var result = new CalculateCharacterFormulaUseCase(
                progressionCatalog,
                formulaCatalog)
            .Execute(
                formula.Reference,
                new ProgressionPointBudgetRequest(
                    characterClass.Id,
                    formula.Applicability.EvolutionIds.First(),
                    1,
                    []),
                new ResetPointInputs(0, 0),
                allocations);

        var statTrace = result.ContextTrace.Single(
            item => item.Kind == FormulaContextResolutionKind.ResolvedStat);
        allocations[statTrace.StatId!] = 1;

        Assert.Equal(0, result.State.Distribution.Allocations[statTrace.StatId!]);
        Assert.Equal(0, statTrace.Allocation);
    }

    private static (FormulaDefinition Formula, ResolvedCharacterState State)
        CreateCanonicalState()
    {
        var progressionCatalog =
            new JsonProgressionRulesetSnapshotReader().Read(CanonicalSnapshotRoot);
        var formula = FindDarkWizardFormula(
            new JsonExecutableFormulaSnapshotReader()
                .Read(CanonicalSnapshotRoot));
        var characterClass = progressionCatalog.Classes.Single(
            item => item.Id == formula.Applicability.CharacterClassId);
        var request = new ProgressionPointBudgetRequest(
            characterClass.Id,
            formula.Applicability.EvolutionIds.First(),
            1,
            []);
        var budget = new CalculateProgressionPointBudgetUseCase(
            progressionCatalog).Execute(request);
        var distribution = new CalculateStatDistributionUseCase(
                progressionCatalog)
            .Execute(
                budget,
                new ResetPointInputs(0, 0),
                characterClass.StatIds.ToDictionary(
                    statId => statId,
                    _ => 0L,
                    StringComparer.Ordinal));
        return (
            formula,
            new ResolvedCharacterState(
                request,
                budget,
                distribution,
                characterClass));
    }

    private static FormulaDefinition CopyFormula(
        FormulaDefinition source,
        IEnumerable<FormulaInputDefinition> inputs) =>
        new(
            source.Reference,
            source.RulesetId,
            source.Status,
            source.Confidence,
            source.Applicability,
            inputs,
            source.Output,
            source.Program,
            source.Rounding,
            source.Trace,
            source.EvidenceRefs,
            source.ConflictIds,
            source.DependencyFormulaRefs);

    private static FormulaInputDefinition CopyInput(
        FormulaInputDefinition source,
        FormulaInputSource inputSource) =>
        new(
            source.Id,
            source.NumericType,
            source.Unit,
            source.NumericBounds,
            source.RangeErrorCode,
            inputSource);

    private static FormulaDefinition CreateSyntheticSourceFormula(
        FormulaReference reference,
        FormulaApplicability applicability) =>
        new(
            reference,
            "mu-s4-global-reference",
            FormulaStatus.Published,
            FormulaConfidence.Unverified,
            applicability,
            [
                new FormulaInputDefinition(
                    "agility",
                    FormulaNumericType.Signed64Bit,
                    "synthetic-stat",
                    TechnicalMinimum(0),
                    "synthetic-input-out-of-range",
                    new FormulaInputSource(
                        FormulaInputSourceKind.ContextValue,
                        "resolved-agility")),
            ],
            new FormulaOutputDefinition("synthetic-source", "synthetic-point"),
            new CheckedDecimalFormulaProgram(
            [
                new CheckedIntegerFormulaStep(
                    "raw-source",
                    CheckedIntegerOperation.Multiply,
                    [
                        new FormulaInputOperand("agility"),
                        new FormulaDecimalLiteralOperand(0.5m),
                    ]),
                new CheckedIntegerFormulaStep(
                    "visible-source",
                    CheckedIntegerOperation.ApplyRounding,
                    [new FormulaStepOperand("raw-source")]),
            ]),
            new FormulaRoundingDefinition(
                FormulaRoundingMode.Truncate,
                "visible-source",
                0),
            new FormulaTraceDefinition(
                ["raw-source", "visible-source"],
                "raw-source",
                "visible-source"),
            ["evidence-synthetic"]);

    private static FormulaDefinition CreateSyntheticConsumerFormula(
        FormulaReference reference,
        FormulaReference sourceReference,
        FormulaApplicability applicability) =>
        new(
            reference,
            "mu-s4-global-reference",
            FormulaStatus.Published,
            FormulaConfidence.Unverified,
            applicability,
            [
                new FormulaInputDefinition(
                    "raw-dependency",
                    FormulaNumericType.ExactBase10,
                    "synthetic-point",
                    TechnicalMinimum(0),
                    "synthetic-input-out-of-range",
                    new FormulaInputSource(
                        sourceReference,
                        FormulaOutputStage.Raw)),
                new FormulaInputDefinition(
                    "visible-dependency",
                    FormulaNumericType.Signed64Bit,
                    "synthetic-point",
                    TechnicalMinimum(0),
                    "synthetic-input-out-of-range",
                    new FormulaInputSource(
                        sourceReference,
                        FormulaOutputStage.Visible)),
            ],
            new FormulaOutputDefinition("synthetic-consumer", "synthetic-point"),
            new CheckedDecimalFormulaProgram(
            [
                new CheckedIntegerFormulaStep(
                    "raw-consumer",
                    CheckedIntegerOperation.Add,
                    [
                        new FormulaInputOperand("raw-dependency"),
                        new FormulaInputOperand("visible-dependency"),
                    ]),
                new CheckedIntegerFormulaStep(
                    "visible-consumer",
                    CheckedIntegerOperation.ApplyRounding,
                    [new FormulaStepOperand("raw-consumer")]),
            ]),
            new FormulaRoundingDefinition(
                FormulaRoundingMode.Truncate,
                "visible-consumer",
                0),
            new FormulaTraceDefinition(
                ["raw-consumer", "visible-consumer"],
                "raw-consumer",
                "visible-consumer"),
            ["evidence-synthetic"],
            dependencyFormulaRefs: [sourceReference]);

    private static FormulaNumericBounds TechnicalMinimum(long minimum) =>
        new(
            minimum,
            true,
            null,
            false,
            FormulaBoundsClassification.Technical);

    private static PositiveFormulaCase[] LoadPositiveCases()
    {
        var directory = Path.Combine(
            CanonicalSnapshotRoot,
            "reference-cases",
            "formulas",
            "valid");
        return Directory.GetFiles(directory, "*.json")
            .Order(StringComparer.Ordinal)
            .Select(path =>
            {
                using var document = JsonDocument.Parse(File.ReadAllText(path));
                var root = document.RootElement;
                var reference = root.GetProperty("formulaRef");
                var context = root.GetProperty("context");
                var expected = root.GetProperty("expectedTrace");
                return new PositiveFormulaCase(
                    new FormulaReference(
                        RequiredString(reference, "id"),
                        RequiredString(reference, "version")),
                    RequiredString(context, "characterClassId"),
                    RequiredString(context, "evolutionId"),
                    ReadValues(root.GetProperty("inputs")),
                    expected.GetProperty("rawOutput").GetDecimal(),
                    expected.GetProperty("visibleOutput").GetInt64(),
                    expected.GetProperty("steps")
                        .EnumerateArray()
                        .Select(step => new FormulaCalculationTraceStep(
                            RequiredString(step, "stepId"),
                            step.GetProperty("value").GetDecimal()))
                        .ToArray());
            })
            .Where(item => FindExecutableReferences().Contains(item.Reference))
            .ToArray();
    }

    private static FormulaDefinition FindDarkWizardFormula(
        ExecutableFormulaCatalog catalog) =>
        catalog.Formulas.Single(
            formula => formula.Reference.Id == "formula-hp-dark-wizard");

    private static string FindFormulaPath(FormulaReference reference) =>
        Directory.GetFiles(
                Path.Combine(CanonicalSnapshotRoot, "formulas"),
                "*.json")
            .Single(path =>
            {
                using var document = JsonDocument.Parse(File.ReadAllText(path));
                var root = document.RootElement;
                return RequiredString(root, "id") == reference.Id &&
                       RequiredString(root, "version") == reference.Version;
            });

    private static HashSet<FormulaReference> FindExecutableReferences() =>
        new JsonExecutableFormulaSnapshotReader()
            .Read(CanonicalSnapshotRoot)
            .Formulas
            .Select(formula => formula.Reference)
            .ToHashSet();

    private static Dictionary<string, decimal> ReadValues(JsonElement element) =>
        element.EnumerateObject().ToDictionary(
            property => property.Name,
            property => property.Value.GetDecimal(),
            StringComparer.Ordinal);

    private static string RequiredString(JsonElement element, string propertyName) =>
        element.GetProperty(propertyName).GetString()
        ?? throw new InvalidDataException($"'{propertyName}' cannot be null.");

    private static string[] StringArray(JsonElement element, string propertyName) =>
        element.GetProperty(propertyName)
            .EnumerateArray()
            .Select(item => item.GetString()
                ?? throw new InvalidDataException(
                    $"'{propertyName}' cannot contain null values."))
            .ToArray();

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

    private sealed record PositiveFormulaCase(
        FormulaReference Reference,
        string CharacterClassId,
        string EvolutionId,
        IReadOnlyDictionary<string, decimal> Inputs,
        decimal RawOutput,
        long VisibleOutput,
        IReadOnlyList<FormulaCalculationTraceStep> Steps);
}
