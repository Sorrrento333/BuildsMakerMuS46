using System.Data;
using System.IO;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using MuOnline.BuildPlanner.Application.Builds;
using MuOnline.BuildPlanner.Application.Formulas;
using MuOnline.BuildPlanner.Application.Progression;
using MuOnline.BuildPlanner.Application.Stats;
using MuOnline.BuildPlanner.Data;
using MuOnline.BuildPlanner.Domain.Formulas;
using MuOnline.BuildPlanner.Domain.Progression;
using MuOnline.BuildPlanner.Domain.Stats;

namespace MuOnline.BuildPlanner.App;

internal static class PublicationSmokeRunner
{
    private const string ExpectedPersistedValue = "persisted-across-update";
    private const string BackupFileName = "publication-smoke.backup.sqlite";
    private const string BuildDraftId = "publication-smoke-draft";

    private static readonly SqliteMigration SyntheticMigration = new(
        2,
        "create_publication_smoke_probe",
        """
        CREATE TABLE publication_smoke_probe (
            id INTEGER NOT NULL PRIMARY KEY,
            value TEXT NOT NULL
        );
        INSERT INTO publication_smoke_probe (id, value) VALUES (1, 'seed');
        """);

    public static PublicationSmokeReport Run(PublicationSmokeOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        EnsureDataOutsideBinaryDirectory(options.DataDirectory);
        Directory.CreateDirectory(options.DataDirectory);
        var progressionVerification = VerifyPublishedProgressionRuleset();
        var formulaVerification = VerifyPublishedCharacterFormulas();
        var buildDraftServices = PublishedBuildDraftServices.Create(
            options.DataDirectory,
            [SyntheticMigration]);

        var databasePath = buildDraftServices.DatabasePath;
        var backupPath = Path.Combine(options.DataDirectory, BackupFileName);

        return options.Phase switch
        {
            PublicationSmokePhase.Initialize => Initialize(
                databasePath,
                backupPath,
                options,
                progressionVerification,
                formulaVerification,
                buildDraftServices),
            PublicationSmokePhase.VerifyUpdate => VerifyUpdate(
                databasePath,
                backupPath,
                options,
                progressionVerification,
                formulaVerification,
                buildDraftServices),
            _ => throw new ArgumentOutOfRangeException(nameof(options)),
        };
    }

    private static PublicationSmokeReport Initialize(
        string databasePath,
        string backupPath,
        PublicationSmokeOptions options,
        ProgressionVerificationResult progressionVerification,
        PublishedFormulaVerification formulaVerification,
        PublishedBuildDraftServices buildDraftServices)
    {
        if (!File.Exists(databasePath) || File.Exists(backupPath))
        {
            throw new InvalidOperationException(
                "The initialize phase requires only the freshly migrated build-draft database.");
        }

        string sqliteVersion;
        MigrationApplicationResult migrationResult;

        using (var connection = OpenConnection(databasePath))
        {
            sqliteVersion = connection.ServerVersion;
            migrationResult = buildDraftServices.MigrationResult;
            ExecuteNonQuery(
                connection,
                "INSERT INTO publication_smoke_probe (id, value) VALUES (2, $value);",
                ExpectedPersistedValue);
            SaveAndVerifyBuildDraft(buildDraftServices, progressionVerification);
            SqliteBackupService.CreateVerifiedBackup(connection, backupPath);
            ExecuteNonQuery(
                connection,
                "UPDATE publication_smoke_probe SET value = $value WHERE id = 2;",
                "must-be-restored");
            SqliteBackupService.RestoreVerifiedBackup(backupPath, connection);

            EnsureExpectedDatabaseState(connection);
        }

        using (var reopenedConnection = OpenConnection(databasePath))
        {
            EnsureExpectedDatabaseState(reopenedConnection);
        }
        VerifyBuildDraft(buildDraftServices, progressionVerification);

        return CreateSuccessfulReport(
            options,
            databasePath,
            backupPath,
            sqliteVersion,
            migrationResult,
            progressionVerification,
            formulaVerification,
            buildDraftServices);
    }

    private static PublicationSmokeReport VerifyUpdate(
        string databasePath,
        string backupPath,
        PublicationSmokeOptions options,
        ProgressionVerificationResult progressionVerification,
        PublishedFormulaVerification formulaVerification,
        PublishedBuildDraftServices buildDraftServices)
    {
        if (!File.Exists(databasePath) || !File.Exists(backupPath))
        {
            throw new FileNotFoundException(
                "The verify-update phase requires the database and backup created by initialize.");
        }

        using var connection = OpenConnection(databasePath);
        var migrationResult = buildDraftServices.MigrationResult;
        EnsureExpectedDatabaseState(connection);
        VerifyBuildDraft(buildDraftServices, progressionVerification);

        return CreateSuccessfulReport(
            options,
            databasePath,
            backupPath,
            connection.ServerVersion,
            migrationResult,
            progressionVerification,
            formulaVerification,
            buildDraftServices);
    }

    private static PublicationSmokeReport CreateSuccessfulReport(
        PublicationSmokeOptions options,
        string databasePath,
        string backupPath,
        string sqliteVersion,
        MigrationApplicationResult migrationResult,
        ProgressionVerificationResult progressionVerification,
        PublishedFormulaVerification formulaVerification,
        PublishedBuildDraftServices buildDraftServices) => new(
            Success: true,
            Phase: options.Phase == PublicationSmokePhase.Initialize ? "initialize" : "verify-update",
            SqliteVersion: sqliteVersion,
            IntegrityResult: "ok",
            PersistedValue: ExpectedPersistedValue,
            DataDirectory: options.DataDirectory,
            DatabasePath: databasePath,
            BackupPath: backupPath,
            BinaryDirectory: AppContext.BaseDirectory,
            DataOutsideBinaryDirectory: true,
            RulesetId: progressionVerification.RulesetId,
            RulesetSnapshotPath: PublishedProgressionRuleset.SnapshotRoot,
            ApprovedProgressionCaseCount: progressionVerification.ApprovedCaseCount,
            RejectedProgressionCaseCount: progressionVerification.RejectedCaseCount,
            SyntheticStatDistributionVerified:
                progressionVerification.SyntheticDistribution.Verified,
            SyntheticStatDistributionStatCount:
                progressionVerification.SyntheticDistribution.StatCount,
            SyntheticStatDistributionSpentPoints:
                progressionVerification.SyntheticDistribution.SpentPoints,
            SyntheticStatDistributionRemainingPoints:
                progressionVerification.SyntheticDistribution.RemainingPoints,
            SyntheticResetCount:
                progressionVerification.SyntheticDistribution.ResetInputs.ResetCount,
            SyntheticPointsPerReset:
                progressionVerification.SyntheticDistribution.ResetInputs.PointsPerReset,
            SyntheticResetPoints:
                progressionVerification.SyntheticDistribution.ResetPoints,
            SyntheticTotalDistributablePoints:
                progressionVerification.SyntheticDistribution.TotalDistributablePoints,
            PublishedFormulaContextVerified: formulaVerification.Verified,
            PublishedFormulaCount: formulaVerification.FormulaReferences.Length,
            PublishedFormulaReferences: formulaVerification.FormulaReferences
                .Select(reference => $"{reference.Id}@{reference.Version}")
                .ToArray(),
            ApprovedPublishedFormulaCaseCount: formulaVerification.ApprovedCaseCount,
            BuildDraftPersistenceVerified: true,
            BuildDraftId: BuildDraftId,
            BuildDraftDatasetVersion:
                buildDraftServices.RuntimeContext.Dataset.Version,
            BuildDraftDatasetHash:
                buildDraftServices.RuntimeContext.Dataset.Hash,
            AppliedMigrationCount: migrationResult.AppliedCount,
            AlreadyAppliedMigrationCount: migrationResult.AlreadyAppliedCount,
            ErrorType: null,
            ErrorMessage: null);

    private static PublishedFormulaVerification VerifyPublishedCharacterFormulas()
    {
        var progressionCatalog = PublishedProgressionRuleset.Catalog;
        var formulaCatalog = PublishedProgressionRuleset.FormulaCatalog;
        var useCase = PublishedProgressionRuleset.CreateCharacterFormulaUseCase();
        var cases = LoadPublishedFormulaCases();
        var verifiedReferences = new HashSet<FormulaReference>();

        foreach (var referenceCase in cases)
        {
            var formula = formulaCatalog.Resolve(referenceCase.FormulaReference);
            verifiedReferences.Add(formula.Reference);

            var characterClass = progressionCatalog.Classes.Single(
                item => item.Id == referenceCase.CharacterClassId);
            var levelInput = formula.Inputs.SingleOrDefault(
                input => input.Source.ValueId == "character-level");
            var allocations = characterClass.StatIds.ToDictionary(
                statId => statId,
                _ => 0L,
                StringComparer.Ordinal);
            foreach (var statInput in formula.Inputs.Where(
                         input => input.Source.ValueId.StartsWith(
                             "resolved-",
                             StringComparison.Ordinal)))
            {
                var statId = statInput.Source.ValueId["resolved-".Length..];
                allocations[statId] = checked(
                    referenceCase.Inputs[statInput.Id] -
                    characterClass.BaseStats[statId].BaseValue);
            }

            var configuredPoints = allocations.Values.Aggregate(
                0L,
                (sum, value) => checked(sum + value));
            var result = useCase.Execute(
                referenceCase.FormulaReference,
                new ProgressionPointBudgetRequest(
                    referenceCase.CharacterClassId,
                    referenceCase.EvolutionId,
                    levelInput is null
                        ? 1
                        : checked((int)referenceCase.Inputs[levelInput.Id]),
                    []),
                new ResetPointInputs(1, configuredPoints),
                allocations);
            if (result.Formula.RawOutput != referenceCase.RawOutput ||
                result.Formula.VisibleOutput != referenceCase.VisibleOutput ||
                !result.Formula.Trace.Steps.SequenceEqual(referenceCase.Steps) ||
                result.ContextTrace.Length != formula.Inputs.Length ||
                result.ContextTrace.Any(
                    item =>
                        !referenceCase.Inputs.TryGetValue(
                            item.InputId,
                            out var expectedInput) ||
                        item.ResolvedValue != expectedInput))
            {
                throw new InvalidOperationException(
                    $"Published formula case '{referenceCase.Id}' did not reproduce its approved context and arithmetic traces.");
            }
        }

        var expectedReferences = formulaCatalog.Formulas
            .Select(formula => formula.Reference)
            .ToHashSet();
        if (!verifiedReferences.SetEquals(expectedReferences))
        {
            throw new InvalidOperationException(
                "Published formula smoke cases do not cover every executable formula reference.");
        }

        return new PublishedFormulaVerification(
            Verified: true,
            FormulaReferences: verifiedReferences
                .OrderBy(reference => reference.Id, StringComparer.Ordinal)
                .ThenBy(reference => reference.Version, StringComparer.Ordinal)
                .ToArray(),
            ApprovedCaseCount: cases.Length);
    }

    private static ProgressionVerificationResult VerifyPublishedProgressionRuleset()
    {
        var catalog = PublishedProgressionRuleset.Catalog;
        var useCase = PublishedProgressionRuleset.CreateUseCase();
        var referenceCaseRoot = Path.Combine(
            PublishedProgressionRuleset.SnapshotRoot,
            "reference-cases",
            "progression");
        var approvedCases = LoadReferenceCases(Path.Combine(referenceCaseRoot, "valid"));
        var rejectedCases = LoadReferenceCases(Path.Combine(referenceCaseRoot, "invalid"));

        foreach (var referenceCase in approvedCases)
        {
            var result = useCase.Execute(referenceCase.ToRequest());
            if (result.RulesetId != referenceCase.RulesetId ||
                result.ProgressionRuleId != referenceCase.ProgressionRuleId ||
                result.EarnedPoints != referenceCase.ExpectedEarnedPoints ||
                result.Contributions.Sum(item => item.EarnedPoints) != result.EarnedPoints)
            {
                throw new InvalidOperationException(
                    $"Published progression case '{referenceCase.Id}' did not reproduce its approved result.");
            }
        }

        foreach (var referenceCase in rejectedCases)
        {
            try
            {
                _ = useCase.Execute(referenceCase.ToRequest());
            }
            catch (ProgressionPointBudgetException exception)
                when (exception.Code == referenceCase.ExpectedErrorCode)
            {
                continue;
            }

            throw new InvalidOperationException(
                $"Published progression case '{referenceCase.Id}' did not reproduce rejection " +
                $"'{referenceCase.ExpectedErrorCode}'.");
        }

        var syntheticDistribution = VerifySyntheticStatDistribution(
            catalog,
            useCase,
            approvedCases);

        return new ProgressionVerificationResult(
            catalog.RulesetId,
            approvedCases.Length,
            rejectedCases.Length,
            syntheticDistribution);
    }

    private static SyntheticStatDistributionVerification VerifySyntheticStatDistribution(
        ProgressionRulesetCatalog catalog,
        CalculateProgressionPointBudgetUseCase progressionUseCase,
        IReadOnlyCollection<PublishedProgressionReferenceCase> approvedCases)
    {
        var sourceCase = approvedCases.First(item => item.ExpectedEarnedPoints > 0);
        var budget = progressionUseCase.Execute(sourceCase.ToRequest());
        var characterClass = catalog.Classes.Single(
            item => item.Id == budget.CharacterClassId);
        var statIds = characterClass.StatIds
            .Order(StringComparer.Ordinal)
            .ToArray();
        var allocations = statIds.ToDictionary(
            statId => statId,
            _ => 0L,
            StringComparer.Ordinal);
        var resetInputs = new ResetPointInputs(2, 100);
        allocations[statIds[0]] = 201;

        var result = new CalculateStatDistributionUseCase(catalog).Execute(
            budget,
            resetInputs,
            allocations);
        if (result.ResetPoints != 200 ||
            result.TotalDistributablePoints != budget.EarnedPoints + 200 ||
            result.SpentPoints != 201 ||
            result.RemainingPoints != budget.EarnedPoints - 1 ||
            !result.Allocations.Keys.ToHashSet(StringComparer.Ordinal)
                .SetEquals(characterClass.StatIds))
        {
            throw new InvalidOperationException(
                "The published snapshot did not reproduce the synthetic stat distribution.");
        }

        return new SyntheticStatDistributionVerification(
            Verified: true,
            StatCount: statIds.Length,
            SpentPoints: result.SpentPoints,
            RemainingPoints: result.RemainingPoints,
            ResetInputs: new BuildDraftResetInputs(
                resetInputs.ResetCount,
                resetInputs.PointsPerReset),
            ResetPoints: result.ResetPoints,
            TotalDistributablePoints: result.TotalDistributablePoints,
            ProgressionInputs: new BuildDraftProgressionInputs(
                sourceCase.ClassId,
                sourceCase.EvolutionId,
                sourceCase.Level,
                sourceCase.CompletedQuestIds),
            Allocations: allocations);
    }

    private static void SaveAndVerifyBuildDraft(
        PublishedBuildDraftServices services,
        ProgressionVerificationResult progressionVerification)
    {
        _ = services.SaveUseCase.ExecuteAsync(
                new SaveBuildDraftRequest(
                    BuildDraftId,
                    progressionVerification.SyntheticDistribution.ProgressionInputs,
                    progressionVerification.SyntheticDistribution.ResetInputs,
                    progressionVerification.SyntheticDistribution.Allocations))
            .GetAwaiter()
            .GetResult();
        VerifyBuildDraft(services, progressionVerification);
    }

    private static void VerifyBuildDraft(
        PublishedBuildDraftServices services,
        ProgressionVerificationResult progressionVerification)
    {
        var draft = services.LoadUseCase.ExecuteAsync(BuildDraftId)
            .GetAwaiter()
            .GetResult();
        if (draft.Id != BuildDraftId ||
            draft.Ruleset != services.RuntimeContext.Ruleset ||
            draft.Dataset != services.RuntimeContext.Dataset ||
            draft.EngineVersion != services.RuntimeContext.EngineVersion ||
            draft.ResetInputs !=
                progressionVerification.SyntheticDistribution.ResetInputs ||
            draft.StatDistribution.ResetPoints !=
                progressionVerification.SyntheticDistribution.ResetPoints ||
            draft.StatDistribution.TotalDistributablePoints !=
                progressionVerification.SyntheticDistribution.TotalDistributablePoints ||
            draft.StatDistribution.SpentPoints !=
                progressionVerification.SyntheticDistribution.SpentPoints ||
            draft.StatDistribution.RemainingPoints !=
                progressionVerification.SyntheticDistribution.RemainingPoints ||
            !SameAllocations(
                draft.StatDistribution.Allocations,
                progressionVerification.SyntheticDistribution.Allocations))
        {
            throw new InvalidOperationException(
                "The build draft did not survive persistence and Application revalidation.");
        }
    }

    private static bool SameAllocations(
        IReadOnlyDictionary<string, long> first,
        IReadOnlyDictionary<string, long> second) =>
        first.Count == second.Count &&
        first.All(item =>
            second.TryGetValue(item.Key, out var value) &&
            item.Value == value);

    private static PublishedProgressionReferenceCase[] LoadReferenceCases(string directory)
    {
        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException(
                $"Published progression reference directory '{directory}' was not found.");
        }

        return Directory.GetFiles(directory, "*.json")
            .Order(StringComparer.Ordinal)
            .Select(path =>
            {
                using var document = JsonDocument.Parse(File.ReadAllText(path));
                var element = document.RootElement;
                return new PublishedProgressionReferenceCase(
                    RequiredString(element, "id"),
                    RequiredString(element, "rulesetId"),
                    RequiredString(element, "progressionRuleId"),
                    RequiredString(element, "classId"),
                    RequiredString(element, "evolutionId"),
                    element.GetProperty("level").GetInt32(),
                    StringArray(element, "completedQuestIds"),
                    element.GetProperty("expectedEarnedPoints").GetInt64(),
                    element.TryGetProperty("expectedErrorCode", out var errorCode)
                        ? errorCode.GetString()
                        : null);
            })
            .ToArray();
    }

    private static PublishedFormulaReferenceCase[] LoadPublishedFormulaCases()
    {
        var directory = Path.Combine(
            PublishedProgressionRuleset.SnapshotRoot,
            "reference-cases",
            "formulas",
            "valid");
        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException(
                $"Published formula reference directory '{directory}' was not found.");
        }

        var executableReferences = PublishedProgressionRuleset.FormulaCatalog.Formulas
            .Select(formula => formula.Reference)
            .ToHashSet();
        return Directory.GetFiles(directory, "*.json")
            .Order(StringComparer.Ordinal)
            .Select(path =>
            {
                using var document = JsonDocument.Parse(File.ReadAllText(path));
                var element = document.RootElement;
                var formulaRef = element.GetProperty("formulaRef");
                var context = element.GetProperty("context");
                var expectedTrace = element.GetProperty("expectedTrace");
                return new PublishedFormulaReferenceCase(
                    RequiredString(element, "id"),
                    new FormulaReference(
                        RequiredString(formulaRef, "id"),
                        RequiredString(formulaRef, "version")),
                    RequiredString(context, "characterClassId"),
                    RequiredString(context, "evolutionId"),
                    ReadLongValues(element.GetProperty("inputs")),
                    expectedTrace.GetProperty("rawOutput").GetDecimal(),
                    expectedTrace.GetProperty("visibleOutput").GetInt64(),
                    expectedTrace.GetProperty("steps")
                        .EnumerateArray()
                        .Select(step => new FormulaCalculationTraceStep(
                            RequiredString(step, "stepId"),
                            step.GetProperty("value").GetDecimal()))
                        .ToArray());
            })
            .Where(item => executableReferences.Contains(item.FormulaReference))
            .ToArray();
    }

    private static Dictionary<string, long> ReadLongValues(JsonElement element) =>
        element.EnumerateObject().ToDictionary(
            property => property.Name,
            property => property.Value.GetInt64(),
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

    private static SqliteConnection OpenConnection(string databasePath)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = false,
        }.ToString();
        var connection = new SqliteConnection(connectionString);
        connection.Open();
        return connection;
    }

    private static void EnsureExpectedDatabaseState(SqliteConnection connection)
    {
        if (connection.State != ConnectionState.Open)
        {
            throw new InvalidOperationException("The smoke database connection must be open.");
        }

        var integrityResult = ExecuteScalar<string>(connection, "PRAGMA integrity_check;");
        if (!string.Equals(integrityResult, "ok", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"SQLite integrity_check returned '{integrityResult}'.");
        }

        var value = ExecuteScalar<string>(
            connection,
            "SELECT value FROM publication_smoke_probe WHERE id = 2;");
        if (!string.Equals(value, ExpectedPersistedValue, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"The persisted smoke value was '{value}' instead of '{ExpectedPersistedValue}'.");
        }

        var ledgerCount = ExecuteScalar<long>(
            connection,
            "SELECT COUNT(*) FROM schema_migrations WHERE version = 1;");
        if (ledgerCount != 1)
        {
            throw new InvalidOperationException(
                $"The migration ledger contained {ledgerCount} entries for version 1.");
        }
    }

    private static void EnsureDataOutsideBinaryDirectory(string dataDirectory)
    {
        var normalizedDataDirectory = EnsureTrailingSeparator(Path.GetFullPath(dataDirectory));
        var normalizedBinaryDirectory = EnsureTrailingSeparator(Path.GetFullPath(AppContext.BaseDirectory));

        if (normalizedDataDirectory.StartsWith(normalizedBinaryDirectory, StringComparison.OrdinalIgnoreCase) ||
            normalizedBinaryDirectory.StartsWith(normalizedDataDirectory, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The smoke data directory must be separate from the published binary directory.");
        }
    }

    private static string EnsureTrailingSeparator(string path) =>
        Path.EndsInDirectorySeparator(path) ? path : string.Concat(path, Path.DirectorySeparatorChar);

    private static T ExecuteScalar<T>(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return (T)command.ExecuteScalar()!;
    }

    private static void ExecuteNonQuery(SqliteConnection connection, string sql, string value)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$value", value);
        command.ExecuteNonQuery();
    }

    private sealed record ProgressionVerificationResult(
        string RulesetId,
        int ApprovedCaseCount,
        int RejectedCaseCount,
        SyntheticStatDistributionVerification SyntheticDistribution);

    private sealed record SyntheticStatDistributionVerification(
        bool Verified,
        int StatCount,
        long SpentPoints,
        long RemainingPoints,
        BuildDraftResetInputs ResetInputs,
        long ResetPoints,
        long TotalDistributablePoints,
        BuildDraftProgressionInputs ProgressionInputs,
        IReadOnlyDictionary<string, long> Allocations);

    private sealed record PublishedFormulaVerification(
        bool Verified,
        FormulaReference[] FormulaReferences,
        int ApprovedCaseCount);

    private sealed record PublishedFormulaReferenceCase(
        string Id,
        FormulaReference FormulaReference,
        string CharacterClassId,
        string EvolutionId,
        IReadOnlyDictionary<string, long> Inputs,
        decimal RawOutput,
        long VisibleOutput,
        IReadOnlyList<FormulaCalculationTraceStep> Steps);

    private sealed record PublishedProgressionReferenceCase(
        string Id,
        string RulesetId,
        string ProgressionRuleId,
        string ClassId,
        string EvolutionId,
        int Level,
        string[] CompletedQuestIds,
        long ExpectedEarnedPoints,
        string? ExpectedErrorCode)
    {
        public ProgressionPointBudgetRequest ToRequest() =>
            new(ClassId, EvolutionId, Level, CompletedQuestIds);
    }
}
