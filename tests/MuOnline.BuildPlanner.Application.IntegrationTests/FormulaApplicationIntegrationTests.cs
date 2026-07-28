using System.Text.Json;
using System.Text.Json.Nodes;
using MuOnline.BuildPlanner.Application.Formulas;
using MuOnline.BuildPlanner.Domain.Formulas;
using Xunit;

namespace MuOnline.BuildPlanner.Application.IntegrationTests;

public sealed record FormulaReferenceCase(
    string Id,
    FormulaReference FormulaReference,
    FormulaCalculationContext Context,
    IReadOnlyDictionary<string, long> Inputs,
    ExpectedFormulaTrace? ExpectedTrace,
    string? ExpectedErrorCode);

public sealed record ExpectedFormulaTrace(
    string RulesetId,
    FormulaReference FormulaReference,
    FormulaCalculationContext Context,
    IReadOnlyDictionary<string, long> Inputs,
    IReadOnlyList<FormulaCalculationTraceStep> Steps,
    FormulaRoundingDefinition Rounding,
    decimal RawOutput,
    long VisibleOutput,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> ConflictIds);

public sealed class FormulaApplicationIntegrationTests
{
    private static readonly string CanonicalSnapshotRoot = FindCanonicalSnapshotRoot();
    private static readonly HashSet<FormulaReference> ExecutableReferences =
        LoadExecutableReferences();
    private static readonly FormulaReferenceCase[] ValidCases =
        LoadReferenceCases("valid")
            .Where(item => ExecutableReferences.Contains(item.FormulaReference))
            .ToArray();
    private static readonly FormulaReferenceCase[] InvalidCases =
        LoadReferenceCases("invalid")
            .Where(item => ExecutableReferences.Contains(item.FormulaReference))
            .ToArray();

    public static TheoryData<FormulaReferenceCase> ApprovedCases =>
        CreateTheoryData(ValidCases);

    public static TheoryData<FormulaReferenceCase> RejectedCases =>
        CreateTheoryData(InvalidCases);

    [Theory]
    [MemberData(nameof(ApprovedCases))]
    public void UseCaseReproducesApprovedTracesFromExecutableCanonicalFormula(
        FormulaReferenceCase referenceCase)
    {
        var catalog = ReadCatalog(CanonicalSnapshotRoot);
        var useCase = new CalculatePublishedFormulaUseCase(catalog);

        var result = useCase.Execute(
            referenceCase.FormulaReference,
            ToRequest(referenceCase));

        var expected = Assert.IsType<ExpectedFormulaTrace>(
            referenceCase.ExpectedTrace);
        var definition = catalog.Resolve(referenceCase.FormulaReference);
        Assert.Equal(definition.Output.Id, result.OutputId);
        Assert.Equal(expected.RawOutput, result.RawOutput);
        Assert.Equal(expected.VisibleOutput, result.VisibleOutput);
        AssertTrace(expected, result.Trace);
    }

    [Theory]
    [MemberData(nameof(RejectedCases))]
    public void UseCaseReproducesApprovedErrorsFromExecutableCanonicalFormula(
        FormulaReferenceCase referenceCase)
    {
        var useCase = new CalculatePublishedFormulaUseCase(
            ReadCatalog(CanonicalSnapshotRoot));

        var exception = Assert.Throws<FormulaCalculationException>(
            () => useCase.Execute(
                referenceCase.FormulaReference,
                ToRequest(referenceCase)));

        Assert.Equal(referenceCase.ExpectedErrorCode, exception.Code);
    }

    [Fact]
    public void UseCaseRejectsHistoricalTextDefinitionAsNotExecutable()
    {
        var historicalPath = Path.Combine(
            CanonicalSnapshotRoot,
            "formulas",
            "hp-dark-wizard.json");
        using var historicalDocument = JsonDocument.Parse(
            File.ReadAllText(historicalPath));
        var historicalReference = new FormulaReference(
            RequiredString(historicalDocument.RootElement, "id"),
            RequiredString(historicalDocument.RootElement, "version"));
        var requestSource = LoadReferenceCases("valid").First(
            item => item.FormulaReference == historicalReference);
        var useCase = new CalculatePublishedFormulaUseCase(
            ReadCatalog(CanonicalSnapshotRoot));

        var exception = Assert.Throws<FormulaExecutionException>(
            () => useCase.Execute(
                historicalReference,
                ToRequest(requestSource)));

        Assert.Equal(
            FormulaExecutionErrorCodes.FormulaNotExecutable,
            exception.Code);
    }

    [Fact]
    public void ReaderMaterializesOnlyExactExecutableReferences()
    {
        var catalog = ReadCatalog(CanonicalSnapshotRoot);
        var historicalPath = Path.Combine(
            CanonicalSnapshotRoot,
            "formulas",
            "hp-dark-wizard.json");
        using var historicalDocument = JsonDocument.Parse(
            File.ReadAllText(historicalPath));

        Assert.Equal(17, catalog.Formulas.Length);
        Assert.Equal(
            [
                "2.0.0",
                "2.0.0",
                "2.0.0",
                "2.0.0",
                "2.0.0",
                "2.0.0",
                "2.0.0",
                "2.1.0",
                "2.1.0",
                "2.1.0",
                "2.1.0",
                "2.1.0",
                "2.1.0",
                "2.1.0",
                "2.1.0",
                "2.1.0",
                "2.1.0",
            ],
            catalog.Formulas
                .Select(ReadSchemaVersion)
                .Order(StringComparer.Ordinal));
        Assert.DoesNotContain(
            catalog.Formulas,
            formula =>
                formula.Reference.Id ==
                    RequiredString(historicalDocument.RootElement, "id") &&
                formula.Reference.Version ==
                    RequiredString(historicalDocument.RootElement, "version"));
    }

    [Fact]
    public void ReaderFailsClosedWhenExecutableFormulaIsNotPublished()
    {
        using var snapshot = TemporaryFormulaSnapshot.CopyFrom(
            CanonicalSnapshotRoot);
        UpdateExecutableFormula(snapshot.FormulasDirectory, root =>
            root["status"] = "REVIEWED");

        var exception = Assert.Throws<FormulaSnapshotException>(
            () => ReadCatalog(snapshot.Root));

        Assert.Equal(
            FormulaSnapshotErrorCodes.FormulaNotPublished,
            exception.Code);
    }

    [Fact]
    public void ReaderFailsClosedWhenApplicabilityReferencesAnotherFamily()
    {
        using var snapshot = TemporaryFormulaSnapshot.CopyFrom(
            CanonicalSnapshotRoot);
        UpdateExecutableFormula(snapshot.FormulasDirectory, root =>
        {
            var applicability = root["applicability"]?.AsObject()
                ?? throw new InvalidDataException("Applicability object expected.");
            applicability["evolutionIds"] = new JsonArray("evolution-fairy-elf");
        });

        var exception = Assert.Throws<FormulaSnapshotException>(
            () => ReadCatalog(snapshot.Root));

        Assert.Equal(
            FormulaSnapshotErrorCodes.ReferenceIncoherent,
            exception.Code);
    }

    [Fact]
    public void ReaderFailsClosedWhenProgramReferencesAFutureStep()
    {
        using var snapshot = TemporaryFormulaSnapshot.CopyFrom(
            CanonicalSnapshotRoot);
        UpdateExecutableFormula(snapshot.FormulasDirectory, root =>
        {
            var strategy = root["strategy"]?.AsObject()
                ?? throw new InvalidDataException("Strategy object expected.");
            var steps = strategy["steps"]?.AsArray()
                ?? throw new InvalidDataException("Steps array expected.");
            var operand = steps[1]?["operands"]?[1]?.AsObject()
                ?? throw new InvalidDataException("Operand object expected.");
            operand.Clear();
            operand["kind"] = "STEP";
            operand["stepId"] = "raw-hp";
        });

        var exception = Assert.Throws<FormulaSnapshotException>(
            () => ReadCatalog(snapshot.Root));

        Assert.Equal(
            FormulaSnapshotErrorCodes.ReferenceIncoherent,
            exception.Code);
    }

    [Fact]
    public void ReaderFailsClosedWhenExactFormulaReferenceIsDuplicated()
    {
        using var snapshot = TemporaryFormulaSnapshot.CopyFrom(
            CanonicalSnapshotRoot);
        var executablePath = FindExecutableFormulaPath(snapshot.FormulasDirectory);
        File.Copy(
            executablePath,
            Path.Combine(snapshot.FormulasDirectory, "duplicate.json"));

        var exception = Assert.Throws<FormulaSnapshotException>(
            () => ReadCatalog(snapshot.Root));

        Assert.Equal(
            FormulaSnapshotErrorCodes.DuplicateReference,
            exception.Code);
    }

    private static ExecutableFormulaCatalog ReadCatalog(string snapshotRoot) =>
        new JsonExecutableFormulaSnapshotReader().Read(snapshotRoot);

    private static FormulaCalculationRequest ToRequest(
        FormulaReferenceCase referenceCase) =>
        new(referenceCase.Context, referenceCase.Inputs);

    private static void AssertTrace(
        ExpectedFormulaTrace expected,
        FormulaCalculationTrace actual)
    {
        Assert.Equal(expected.RulesetId, actual.RulesetId);
        Assert.Equal(expected.FormulaReference, actual.FormulaReference);
        Assert.Equal(expected.Context.CharacterClassId, actual.Context.CharacterClassId);
        Assert.Equal(expected.Context.EvolutionId, actual.Context.EvolutionId);
        Assert.Equal(expected.Inputs, actual.Inputs);
        Assert.Equal(expected.Steps, actual.Steps);
        Assert.Equal(expected.Rounding, actual.Rounding);
        Assert.Equal(expected.RawOutput, actual.RawOutput);
        Assert.Equal(expected.VisibleOutput, actual.VisibleOutput);
        Assert.Equal(expected.EvidenceRefs, actual.EvidenceRefs);
        Assert.Equal(expected.ConflictIds, actual.ConflictIds);
    }

    private static FormulaReferenceCase[] LoadReferenceCases(
        string classification)
    {
        var directory = Path.Combine(
            CanonicalSnapshotRoot,
            "reference-cases",
            "formulas",
            classification);

        return Directory.GetFiles(directory, "*.json")
            .Order(StringComparer.Ordinal)
            .Select(ParseReferenceCase)
            .ToArray();
    }

    private static HashSet<FormulaReference> LoadExecutableReferences() =>
        Directory.GetFiles(
                Path.Combine(CanonicalSnapshotRoot, "formulas"),
                "*.json")
            .Select(path =>
            {
                using var document = JsonDocument.Parse(File.ReadAllText(path));
                var root = document.RootElement;
                return (
                    SchemaVersion: RequiredString(root, "schemaVersion"),
                    Reference: new FormulaReference(
                        RequiredString(root, "id"),
                        RequiredString(root, "version")));
            })
            .Where(item => item.SchemaVersion == "2.0.0")
            .Select(item => item.Reference)
            .ToHashSet();

    private static FormulaReferenceCase ParseReferenceCase(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var element = document.RootElement;
        var formulaReference = ParseReference(element.GetProperty("formulaRef"));
        var context = ParseContext(element.GetProperty("context"));
        var inputs = ParseValues(element.GetProperty("inputs"));

        return new FormulaReferenceCase(
            RequiredString(element, "id"),
            formulaReference,
            context,
            inputs,
            element.TryGetProperty("expectedTrace", out var expectedTrace)
                ? ParseExpectedTrace(expectedTrace)
                : null,
            element.TryGetProperty("expectedErrorCode", out var expectedError)
                ? expectedError.GetString()
                : null);
    }

    private static ExpectedFormulaTrace ParseExpectedTrace(JsonElement element)
    {
        var rounding = element.GetProperty("rounding");
        return new ExpectedFormulaTrace(
            RequiredString(element, "rulesetId"),
            ParseReference(element.GetProperty("formulaRef")),
            ParseContext(element.GetProperty("context")),
            ParseValues(element.GetProperty("inputs")),
            element.GetProperty("steps")
                .EnumerateArray()
                .Select(step => new FormulaCalculationTraceStep(
                    RequiredString(step, "stepId"),
                    step.GetProperty("value").GetDecimal()))
                .ToArray(),
            new FormulaRoundingDefinition(
                ParseRoundingMode(RequiredString(rounding, "mode")),
                RequiredString(rounding, "stage"),
                rounding.TryGetProperty("decimalPlaces", out var decimalPlaces)
                    ? decimalPlaces.GetInt32()
                    : null),
            element.GetProperty("rawOutput").GetDecimal(),
            element.GetProperty("visibleOutput").GetInt64(),
            StringArray(element, "evidenceRefs"),
            OptionalStringArray(element, "conflictIds"));
    }

    private static FormulaReference ParseReference(JsonElement element) =>
        new(
            RequiredString(element, "id"),
            RequiredString(element, "version"));

    private static FormulaCalculationContext ParseContext(JsonElement element) =>
        new(
            RequiredString(element, "characterClassId"),
            RequiredString(element, "evolutionId"));

    private static Dictionary<string, long> ParseValues(
        JsonElement element) =>
        element.EnumerateObject().ToDictionary(
            property => property.Name,
            property => property.Value.GetInt64(),
            StringComparer.Ordinal);

    private static FormulaRoundingMode ParseRoundingMode(string value) =>
        value switch
        {
            "NONE" => FormulaRoundingMode.None,
            "FLOOR" => FormulaRoundingMode.Floor,
            "CEILING" => FormulaRoundingMode.Ceiling,
            "TRUNCATE" => FormulaRoundingMode.Truncate,
            "HALF_UP" => FormulaRoundingMode.HalfUp,
            "HALF_EVEN" => FormulaRoundingMode.HalfEven,
            _ => throw new InvalidDataException($"Unknown rounding mode '{value}'."),
        };

    private static TheoryData<FormulaReferenceCase> CreateTheoryData(
        IEnumerable<FormulaReferenceCase> cases)
    {
        var data = new TheoryData<FormulaReferenceCase>();
        foreach (var referenceCase in cases)
        {
            data.Add(referenceCase);
        }

        return data;
    }

    private static void UpdateExecutableFormula(
        string formulasDirectory,
        Action<JsonObject> update)
    {
        var path = FindExecutableFormulaPath(formulasDirectory);
        var root = JsonNode.Parse(File.ReadAllText(path))?.AsObject()
            ?? throw new InvalidDataException($"JSON object expected in '{path}'.");
        update(root);
        File.WriteAllText(
            path,
            root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    private static string FindExecutableFormulaPath(string formulasDirectory) =>
        Path.Combine(formulasDirectory, "hp-dark-wizard-1.1.0.json");

    private static string ReadSchemaVersion(FormulaDefinition definition)
    {
        var path = Directory.GetFiles(
                Path.Combine(CanonicalSnapshotRoot, "formulas"),
                "*.json")
            .Single(candidate =>
            {
                using var candidateDocument = JsonDocument.Parse(
                    File.ReadAllText(candidate));
                var root = candidateDocument.RootElement;
                return RequiredString(root, "id") == definition.Reference.Id &&
                       RequiredString(root, "version") ==
                       definition.Reference.Version;
            });
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        Assert.Equal(
            new FormulaReference(
                RequiredString(document.RootElement, "id"),
                RequiredString(document.RootElement, "version")),
            definition.Reference);
        return RequiredString(document.RootElement, "schemaVersion");
    }

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

    private static string[] OptionalStringArray(
        JsonElement element,
        string propertyName) =>
        element.TryGetProperty(propertyName, out var values)
            ? values.EnumerateArray()
                .Select(item => item.GetString()
                    ?? throw new InvalidDataException(
                        $"'{propertyName}' cannot contain null values."))
                .ToArray()
            : [];

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

        throw new DirectoryNotFoundException("Canonical ruleset snapshot was not found.");
    }

    private sealed class TemporaryFormulaSnapshot : IDisposable
    {
        private TemporaryFormulaSnapshot(string root)
        {
            Root = root;
        }

        public string Root { get; }

        public string FormulasDirectory => Path.Combine(Root, "formulas");

        public static TemporaryFormulaSnapshot CopyFrom(string sourceRoot)
        {
            var root = Path.Combine(
                Path.GetTempPath(),
                $"mu-build-planner-formulas-{Guid.NewGuid():N}");
            Directory.CreateDirectory(root);
            CopyDirectory(Path.Combine(sourceRoot, "character-classes"), root);
            CopyDirectory(Path.Combine(sourceRoot, "formulas"), root);
            return new TemporaryFormulaSnapshot(root);
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }

        private static void CopyDirectory(
            string sourceDirectory,
            string destinationRoot)
        {
            var destinationDirectory = Path.Combine(
                destinationRoot,
                Path.GetFileName(sourceDirectory));
            Directory.CreateDirectory(destinationDirectory);

            foreach (var sourcePath in Directory.GetFiles(sourceDirectory, "*.json"))
            {
                File.Copy(
                    sourcePath,
                    Path.Combine(destinationDirectory, Path.GetFileName(sourcePath)));
            }
        }
    }
}
