using System.Data;
using System.IO;
using Microsoft.Data.Sqlite;
using MuOnline.BuildPlanner.Data;

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

        var databasePath = Path.Combine(options.DataDirectory, DatabaseFileName);
        var backupPath = Path.Combine(options.DataDirectory, BackupFileName);

        return options.Phase switch
        {
            PublicationSmokePhase.Initialize => Initialize(databasePath, backupPath, options),
            PublicationSmokePhase.VerifyUpdate => VerifyUpdate(databasePath, backupPath, options),
            _ => throw new ArgumentOutOfRangeException(nameof(options)),
        };
    }

    private static PublicationSmokeReport Initialize(
        string databasePath,
        string backupPath,
        PublicationSmokeOptions options)
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
            migrationResult);
    }

    private static PublicationSmokeReport VerifyUpdate(
        string databasePath,
        string backupPath,
        PublicationSmokeOptions options)
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
            migrationResult);
    }

    private static PublicationSmokeReport CreateSuccessfulReport(
        PublicationSmokeOptions options,
        string databasePath,
        string backupPath,
        string sqliteVersion,
        MigrationApplicationResult migrationResult) => new(
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
            AppliedMigrationCount: migrationResult.AppliedCount,
            AlreadyAppliedMigrationCount: migrationResult.AlreadyAppliedCount,
            ErrorType: null,
            ErrorMessage: null);

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
}
