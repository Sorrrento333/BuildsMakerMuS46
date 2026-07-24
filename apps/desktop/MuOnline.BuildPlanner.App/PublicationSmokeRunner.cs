using System.Data;
using System.IO;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using MuOnline.BuildPlanner.Application.Progression;
using MuOnline.BuildPlanner.Application.Stats;
using MuOnline.BuildPlanner.Data;
using MuOnline.BuildPlanner.Domain.Progression;
using MuOnline.BuildPlanner.Domain.Stats;

namespace MuOnline.BuildPlanner.App;

internal static class PublicationSmokeRunner
{
    private const string ExpectedPersistedValue = "persisted-across-update";
    private const string DatabaseFileName = "publication-smoke.sqlite";
    private const string BackupFileName = "publication-smoke.backup.sqlite";

    private static readonly SqliteMigration SyntheticMigration = new(
        1,
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

        var databasePath = Path.Combine(options.DataDirectory, DatabaseFileName);
        var backupPath = Path.Combine(options.DataDirectory, BackupFileName);

        return options.Phase switch
        {
            PublicationSmokePhase.Initialize => Initialize(
                databasePath,
                backupPath,
                options,
                progressionVerification),
            PublicationSmokePhase.VerifyUpdate => VerifyUpdate(
                databasePath,
                backupPath,
                options,
                progressionVerification),
            _ => throw new ArgumentOutOfRangeException(nameof(options)),
        };
    }

    private static PublicationSmokeReport Initialize(
        string databasePath,
        string backupPath,
        PublicationSmokeOptions options,
        ProgressionVerificationResult progressionVerification)
    {
        if (File.Exists(databasePath) || File.Exists(backupPath))
        {
            throw new InvalidOperationException(
                "The initialize phase requires a new data directory without prior smoke artifacts.");
        }

        string sqliteVersion;
        MigrationApplicationResult migrationResult;

        using (var connection = OpenConnection(databasePath))
        {
            sqliteVersion = connection.ServerVersion;
            migrationResult = new SqliteMigrationRunner().Apply(connection, [SyntheticMigration]);
            ExecuteNonQuery(
                connection,
                "INSERT INTO publication_smoke_probe (id, value) VALUES (2, $value);",
                ExpectedPersistedValue);
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

        return CreateSuccessfulReport(
            options,
            databasePath,
            backupPath,
            sqliteVersion,
            migrationResult,
            progressionVerification);
    }

    private static PublicationSmokeReport VerifyUpdate(
        string databasePath,
        string backupPath,
        PublicationSmokeOptions options,
        ProgressionVerificationResult progressionVerification)
    {
        if (!File.Exists(databasePath) || !File.Exists(backupPath))
        {
            throw new FileNotFoundException(
                "The verify-update phase requires the database and backup created by initialize.");
        }

        using var connection = OpenConnection(databasePath);
        var migrationResult = new SqliteMigrationRunner().Apply(connection, [SyntheticMigration]);
        EnsureExpectedDatabaseState(connection);

        return CreateSuccessfulReport(
            options,
            databasePath,
            backupPath,
            connection.ServerVersion,
            migrationResult,
            progressionVerification);
    }

    private static PublicationSmokeReport CreateSuccessfulReport(
        PublicationSmokeOptions options,
        string databasePath,
        string backupPath,
        string sqliteVersion,
        MigrationApplicationResult migrationResult,
        ProgressionVerificationResult progressionVerification) => new(
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
            AppliedMigrationCount: migrationResult.AppliedCount,
            AlreadyAppliedMigrationCount: migrationResult.AlreadyAppliedCount,
            ErrorType: null,
            ErrorMessage: null);

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
        allocations[statIds[0]] = 1;

        var result = new CalculateStatDistributionUseCase(catalog).Execute(
            budget,
            allocations);
        if (result.SpentPoints != 1 ||
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
            RemainingPoints: result.RemainingPoints);
    }

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
        long RemainingPoints);

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
