using System.Data;
using System.Globalization;
using Microsoft.Data.Sqlite;

namespace MuOnline.BuildPlanner.Data;

public sealed class SqliteMigrationRunner
{
    public const string LedgerTableName = "schema_migrations";

    private const string CreateLedgerSql = """
        CREATE TABLE IF NOT EXISTS schema_migrations (
            version INTEGER NOT NULL PRIMARY KEY,
            name TEXT NOT NULL,
            sha256 TEXT NOT NULL,
            applied_utc TEXT NOT NULL
        );
        """;

    private readonly TimeProvider timeProvider;

    public SqliteMigrationRunner(TimeProvider? timeProvider = null)
    {
        this.timeProvider = timeProvider ?? TimeProvider.System;
    }

    public MigrationApplicationResult Apply(
        SqliteConnection connection,
        IEnumerable<SqliteMigration> migrations)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(migrations);

        if (connection.State != ConnectionState.Open)
        {
            throw new InvalidOperationException("The SQLite connection must be open before applying migrations.");
        }

        var catalog = migrations.OrderBy(migration => migration.Version).ToArray();
        ValidateCatalog(catalog);
        EnsureLedger(connection);

        var appliedMigrations = LoadAppliedMigrations(connection);
        EnsureDatabaseMatchesCatalog(catalog, appliedMigrations);

        var appliedCount = 0;
        var alreadyAppliedCount = 0;

        foreach (var migration in catalog)
        {
            if (appliedMigrations.ContainsKey(migration.Version))
            {
                alreadyAppliedCount++;
                continue;
            }

            ApplyMigration(connection, migration);
            appliedCount++;
        }

        var currentVersion = catalog.Length == 0 ? 0 : catalog[^1].Version;
        return new MigrationApplicationResult(appliedCount, alreadyAppliedCount, currentVersion);
    }

    private static void ValidateCatalog(SqliteMigration[] catalog)
    {
        for (var index = 1; index < catalog.Length; index++)
        {
            if (catalog[index - 1].Version == catalog[index].Version)
            {
                throw new MigrationIntegrityException(
                    $"Migration version {catalog[index].Version} appears more than once in the catalog.");
            }
        }

        var duplicateName = catalog
            .GroupBy(migration => migration.Name, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateName is not null)
        {
            throw new MigrationIntegrityException(
                $"Migration name '{duplicateName.Key}' appears more than once in the catalog.");
        }
    }

    private static void EnsureLedger(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = CreateLedgerSql;
        command.ExecuteNonQuery();
    }

    private static Dictionary<long, AppliedMigration> LoadAppliedMigrations(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT version, name, sha256
            FROM schema_migrations
            ORDER BY version;
            """;

        using var reader = command.ExecuteReader();
        var appliedMigrations = new Dictionary<long, AppliedMigration>();

        while (reader.Read())
        {
            var appliedMigration = new AppliedMigration(
                reader.GetInt64(0),
                reader.GetString(1),
                reader.GetString(2));
            appliedMigrations.Add(appliedMigration.Version, appliedMigration);
        }

        return appliedMigrations;
    }

    private static void EnsureDatabaseMatchesCatalog(
        SqliteMigration[] catalog,
        IReadOnlyDictionary<long, AppliedMigration> appliedMigrations)
    {
        var catalogByVersion = catalog.ToDictionary(migration => migration.Version);

        foreach (var appliedMigration in appliedMigrations.Values)
        {
            if (!catalogByVersion.TryGetValue(appliedMigration.Version, out var expectedMigration))
            {
                throw new MigrationIntegrityException(
                    $"Database contains migration version {appliedMigration.Version}, which is absent from the catalog.");
            }

            if (!string.Equals(appliedMigration.Name, expectedMigration.Name, StringComparison.Ordinal) ||
                !string.Equals(appliedMigration.Checksum, expectedMigration.Checksum, StringComparison.Ordinal))
            {
                throw new MigrationIntegrityException(
                    $"Migration version {appliedMigration.Version} no longer matches its recorded name and checksum.");
            }
        }
    }

    private void ApplyMigration(SqliteConnection connection, SqliteMigration migration)
    {
        using var transaction = connection.BeginTransaction();

        using (var migrationCommand = connection.CreateCommand())
        {
            migrationCommand.Transaction = transaction;
            migrationCommand.CommandText = migration.Sql;
            migrationCommand.ExecuteNonQuery();
        }

        using (var ledgerCommand = connection.CreateCommand())
        {
            ledgerCommand.Transaction = transaction;
            ledgerCommand.CommandText = """
                INSERT INTO schema_migrations (version, name, sha256, applied_utc)
                VALUES ($version, $name, $sha256, $appliedUtc);
                """;
            ledgerCommand.Parameters.AddWithValue("$version", migration.Version);
            ledgerCommand.Parameters.AddWithValue("$name", migration.Name);
            ledgerCommand.Parameters.AddWithValue("$sha256", migration.Checksum);
            ledgerCommand.Parameters.AddWithValue(
                "$appliedUtc",
                timeProvider.GetUtcNow().ToString("O", CultureInfo.InvariantCulture));
            ledgerCommand.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    private sealed record AppliedMigration(long Version, string Name, string Checksum);
}
