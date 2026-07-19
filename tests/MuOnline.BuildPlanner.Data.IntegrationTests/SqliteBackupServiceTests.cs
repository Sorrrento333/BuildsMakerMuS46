using Microsoft.Data.Sqlite;
using Xunit;

namespace MuOnline.BuildPlanner.Data.IntegrationTests;

public sealed class SqliteBackupServiceTests
{
    private static readonly SqliteMigration SyntheticMigration = new(
        1,
        "create_synthetic_backup_probe",
        """
        CREATE TABLE synthetic_backup_probe (
            id INTEGER NOT NULL PRIMARY KEY,
            value TEXT NOT NULL
        );
        INSERT INTO synthetic_backup_probe (id, value) VALUES (1, 'seed');
        """);

    [Fact]
    public void CreateVerifiedBackupCopiesMigratedSchemaLedgerAndCommittedData()
    {
        using var workingDatabase = new TemporarySqliteDatabase();
        using var backupDatabase = new TemporarySqliteDatabase(createFile: false);
        using var connection = workingDatabase.OpenConnection();
        new SqliteMigrationRunner().Apply(connection, [SyntheticMigration]);
        ExecuteNonQuery(
            connection,
            "INSERT INTO synthetic_backup_probe (id, value) VALUES (2, 'committed');");

        SqliteBackupService.CreateVerifiedBackup(connection, backupDatabase.Path);

        using var backupConnection = backupDatabase.OpenConnection(SqliteOpenMode.ReadOnly);
        Assert.Equal("ok", ExecuteScalar<string>(backupConnection, "PRAGMA integrity_check;"));
        Assert.Equal(1L, ExecuteScalar<long>(backupConnection, "SELECT COUNT(*) FROM schema_migrations;"));
        Assert.Equal("committed", ExecuteScalar<string>(
            backupConnection,
            "SELECT value FROM synthetic_backup_probe WHERE id = 2;"));
    }

    [Fact]
    public void RestoreVerifiedBackupRecoversSchemaLedgerAndData()
    {
        using var workingDatabase = new TemporarySqliteDatabase();
        using var backupDatabase = new TemporarySqliteDatabase(createFile: false);
        using var connection = workingDatabase.OpenConnection();
        new SqliteMigrationRunner().Apply(connection, [SyntheticMigration]);
        ExecuteNonQuery(
            connection,
            "INSERT INTO synthetic_backup_probe (id, value) VALUES (2, 'recover-me');");
        SqliteBackupService.CreateVerifiedBackup(connection, backupDatabase.Path);
        ExecuteNonQuery(connection, "DROP TABLE synthetic_backup_probe;");
        ExecuteNonQuery(connection, "DELETE FROM schema_migrations;");

        SqliteBackupService.RestoreVerifiedBackup(backupDatabase.Path, connection);

        Assert.Equal("ok", ExecuteScalar<string>(connection, "PRAGMA integrity_check;"));
        Assert.Equal(1L, ExecuteScalar<long>(connection, "SELECT COUNT(*) FROM schema_migrations;"));
        Assert.Equal("recover-me", ExecuteScalar<string>(
            connection,
            "SELECT value FROM synthetic_backup_probe WHERE id = 2;"));
    }

    [Fact]
    public void FailedCandidateVerificationDoesNotOverwriteExistingValidBackup()
    {
        using var workingDatabase = new TemporarySqliteDatabase();
        using var backupDatabase = new TemporarySqliteDatabase(createFile: false);
        using (var connection = workingDatabase.OpenConnection())
        {
            new SqliteMigrationRunner().Apply(connection, [SyntheticMigration]);
            ExecuteNonQuery(
                connection,
                "INSERT INTO synthetic_backup_probe (id, value) VALUES (2, 'valid-backup');");
            SqliteBackupService.CreateVerifiedBackup(connection, backupDatabase.Path);
        }

        using (var connection = workingDatabase.OpenConnection())
        {
            ExecuteNonQuery(connection, "PRAGMA writable_schema = ON;");
            ExecuteNonQuery(
                connection,
                "UPDATE sqlite_schema SET rootpage = 2147483647 WHERE name = 'synthetic_backup_probe';");
            ExecuteNonQuery(connection, "PRAGMA writable_schema = OFF;");

            Assert.Throws<SqliteBackupIntegrityException>(
                () => SqliteBackupService.CreateVerifiedBackup(connection, backupDatabase.Path));
        }

        using var backupConnection = backupDatabase.OpenConnection(SqliteOpenMode.ReadOnly);
        Assert.Equal("ok", ExecuteScalar<string>(backupConnection, "PRAGMA integrity_check;"));
        Assert.Equal("valid-backup", ExecuteScalar<string>(
            backupConnection,
            "SELECT value FROM synthetic_backup_probe WHERE id = 2;"));
    }

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
        public TemporarySqliteDatabase(bool createFile = true)
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"mu-build-planner-backup-tests-{Guid.NewGuid():N}.sqlite");

            if (createFile)
            {
                using var connection = OpenConnection();
            }
        }

        public string Path { get; }

        public SqliteConnection OpenConnection(SqliteOpenMode mode = SqliteOpenMode.ReadWriteCreate)
        {
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = Path,
                Mode = mode,
                Pooling = false,
            }.ToString();
            var connection = new SqliteConnection(connectionString);
            connection.Open();
            return connection;
        }

        public void Dispose()
        {
            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
        }
    }
}
