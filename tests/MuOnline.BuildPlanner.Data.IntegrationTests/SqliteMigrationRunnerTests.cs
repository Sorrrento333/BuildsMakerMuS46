using Microsoft.Data.Sqlite;
using Xunit;

namespace MuOnline.BuildPlanner.Data.IntegrationTests;

public sealed class SqliteMigrationRunnerTests
{
    private static readonly DateTimeOffset AppliedAt = new(2026, 7, 18, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void NewDatabaseAppliesMigrationAndRecordsLedger()
    {
        using var database = new TemporarySqliteDatabase();
        using var connection = database.OpenConnection();
        var migration = CreateSyntheticMigration();
        var runner = CreateRunner();

        var result = runner.Apply(connection, [migration]);

        Assert.Equal(new MigrationApplicationResult(1, 0, 1), result);
        Assert.Equal(1L, ExecuteScalar<long>(connection, "SELECT COUNT(*) FROM synthetic_probe;"));
        Assert.Equal(1L, ExecuteScalar<long>(connection, "SELECT COUNT(*) FROM schema_migrations;"));
        Assert.Equal(migration.Checksum, ExecuteScalar<string>(connection, "SELECT sha256 FROM schema_migrations;"));
        Assert.Equal(AppliedAt.ToString("O"), ExecuteScalar<string>(connection, "SELECT applied_utc FROM schema_migrations;"));
    }

    [Fact]
    public void ReopenedDatabaseRetainsMigratedData()
    {
        using var database = new TemporarySqliteDatabase();
        var runner = CreateRunner();
        var catalog = new[] { CreateSyntheticMigration() };

        using (var connection = database.OpenConnection())
        {
            runner.Apply(connection, catalog);
            ExecuteNonQuery(connection, "INSERT INTO synthetic_probe (value) VALUES ('persisted');");
        }

        using var reopenedConnection = database.OpenConnection();

        Assert.Equal("persisted", ExecuteScalar<string>(
            reopenedConnection,
            "SELECT value FROM synthetic_probe WHERE id = 2;"));
        Assert.Equal(1L, ExecuteScalar<long>(reopenedConnection, "SELECT COUNT(*) FROM schema_migrations;"));
    }

    [Fact]
    public void ReapplyingSameCatalogDoesNotDuplicateMigration()
    {
        using var database = new TemporarySqliteDatabase();
        using var connection = database.OpenConnection();
        var runner = CreateRunner();
        var catalog = new[] { CreateSyntheticMigration() };

        runner.Apply(connection, catalog);
        var secondResult = runner.Apply(connection, catalog);

        Assert.Equal(new MigrationApplicationResult(0, 1, 1), secondResult);
        Assert.Equal(1L, ExecuteScalar<long>(connection, "SELECT COUNT(*) FROM schema_migrations;"));
        Assert.Equal(1L, ExecuteScalar<long>(connection, "SELECT COUNT(*) FROM synthetic_probe;"));
    }

    [Fact]
    public void ChangedAppliedMigrationIsRejected()
    {
        using var database = new TemporarySqliteDatabase();
        using var connection = database.OpenConnection();
        var runner = CreateRunner();
        runner.Apply(connection, [CreateSyntheticMigration()]);
        var changedMigration = new SqliteMigration(
            1,
            "create_synthetic_probe",
            "ALTER TABLE synthetic_probe ADD COLUMN changed INTEGER;");

        var exception = Assert.Throws<MigrationIntegrityException>(
            () => runner.Apply(connection, [changedMigration]));

        Assert.Contains("no longer matches", exception.Message, StringComparison.Ordinal);
        Assert.Equal(0L, ExecuteScalar<long>(
            connection,
            "SELECT COUNT(*) FROM pragma_table_info('synthetic_probe') WHERE name = 'changed';"));
    }

    [Fact]
    public void FailedMigrationRollsBackSchemaAndLedgerEntry()
    {
        using var database = new TemporarySqliteDatabase();
        using var connection = database.OpenConnection();
        var runner = CreateRunner();
        var failingMigration = new SqliteMigration(
            1,
            "create_then_fail",
            """
            CREATE TABLE rollback_probe (id INTEGER NOT NULL PRIMARY KEY);
            INSERT INTO missing_table (id) VALUES (1);
            """);

        Assert.Throws<SqliteException>(() => runner.Apply(connection, [failingMigration]));

        Assert.Equal(0L, ExecuteScalar<long>(
            connection,
            "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'rollback_probe';"));
        Assert.Equal(0L, ExecuteScalar<long>(connection, "SELECT COUNT(*) FROM schema_migrations;"));
    }

    private static SqliteMigrationRunner CreateRunner() => new(new FixedTimeProvider(AppliedAt));

    private static SqliteMigration CreateSyntheticMigration() => new(
        1,
        "create_synthetic_probe",
        """
        CREATE TABLE synthetic_probe (
            id INTEGER NOT NULL PRIMARY KEY,
            value TEXT NOT NULL
        );
        INSERT INTO synthetic_probe (id, value) VALUES (1, 'seed');
        """);

    private static T ExecuteScalar<T>(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return (T)command.ExecuteScalar()!;
    }

    private static void ExecuteNonQuery(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private sealed class TemporarySqliteDatabase : IDisposable
    {
        private readonly string path = Path.Combine(
            Path.GetTempPath(),
            $"mu-build-planner-data-tests-{Guid.NewGuid():N}.sqlite");

        public SqliteConnection OpenConnection()
        {
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = path,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Pooling = false,
            }.ToString();
            var connection = new SqliteConnection(connectionString);
            connection.Open();
            return connection;
        }

        public void Dispose()
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
