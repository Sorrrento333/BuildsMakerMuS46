using System.Diagnostics;
using Microsoft.Data.Sqlite;
using Xunit;

namespace MuOnline.BuildPlanner.Data.IntegrationTests;

public sealed class SqliteWriteContentionPolicyTests
{
    [Fact]
    public void ZeroCommandTimeoutIsRejectedBecauseProviderTreatsItAsUnbounded()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new SqliteWriteContentionOptions(
                commandTimeoutSeconds: 0,
                maximumRetryCount: 0,
                retryDelay: TimeSpan.Zero));
    }

    [Fact]
    public void ExhaustedTimeoutThrowsTypedContentionErrorWithinBoundedTime()
    {
        using var database = new TemporarySqliteDatabase();
        using var firstWriter = database.OpenConnection();
        using var secondWriter = database.OpenConnection();
        CreateSyntheticTable(firstWriter);
        using var blockingTransaction = firstWriter.BeginTransaction(deferred: false);
        InsertSyntheticRow(firstWriter, blockingTransaction, 1, "blocking-writer");
        var originalTimeout = secondWriter.DefaultTimeout;
        var policy = CreatePolicy(maximumRetryCount: 0);
        var stopwatch = Stopwatch.StartNew();

        var exception = Assert.Throws<SqliteWriteContentionException>(
            () => policy.Execute(secondWriter, InsertPolicyRow));

        stopwatch.Stop();
        Assert.Equal(1, exception.AttemptCount);
        Assert.Equal(1, exception.CommandTimeoutSeconds);
        Assert.Equal(0, exception.MaximumRetryCount);
        Assert.Equal(5, ((SqliteException)exception.InnerException!).SqliteErrorCode);
        Assert.InRange(stopwatch.Elapsed, TimeSpan.FromMilliseconds(750), TimeSpan.FromSeconds(3));
        Assert.Equal(originalTimeout, secondWriter.DefaultTimeout);
        Assert.Equal(0, CountRows(secondWriter, 2));
    }

    [Fact]
    public void RetryAfterLockReleaseCommitsOperationExactlyOnce()
    {
        using var database = new TemporarySqliteDatabase();
        using var firstWriter = database.OpenConnection();
        using var secondWriter = database.OpenConnection();
        CreateSyntheticTable(firstWriter);
        using var blockingTransaction = firstWriter.BeginTransaction(deferred: false);
        InsertSyntheticRow(firstWriter, blockingTransaction, 1, "blocking-writer");
        var operationCount = 0;
        var retryNotificationCount = 0;
        var policy = CreatePolicy(maximumRetryCount: 1);

        var result = policy.Execute(
            secondWriter,
            (connection, transaction) =>
            {
                operationCount++;
                InsertSyntheticRow(connection, transaction, 2, "policy-writer");
            },
            _ =>
            {
                retryNotificationCount++;
                blockingTransaction.Commit();
            });

        Assert.Equal(new SqliteWriteExecutionResult(2), result);
        Assert.Equal(1, retryNotificationCount);
        Assert.Equal(1, operationCount);
        Assert.Equal(1, CountRows(secondWriter, 1));
        Assert.Equal(1, CountRows(secondWriter, 2));
    }

    [Fact]
    public void NonContentionSqliteErrorIsNotRetriedAndRollsBackTransaction()
    {
        using var database = new TemporarySqliteDatabase();
        using var connection = database.OpenConnection();
        CreateSyntheticTable(connection);
        var operationCount = 0;
        var retryNotificationCount = 0;
        var policy = CreatePolicy(maximumRetryCount: 2);

        var exception = Assert.Throws<SqliteException>(() => policy.Execute(
            connection,
            (writeConnection, transaction) =>
            {
                operationCount++;
                InsertSyntheticRow(writeConnection, transaction, 2, "rolled-back");

                using var failingCommand = writeConnection.CreateCommand();
                failingCommand.Transaction = transaction;
                failingCommand.CommandText = "INSERT INTO missing_table (id) VALUES (1);";
                failingCommand.ExecuteNonQuery();
            },
            _ => retryNotificationCount++));

        Assert.Equal(1, exception.SqliteErrorCode);
        Assert.Equal(1, operationCount);
        Assert.Equal(0, retryNotificationCount);
        Assert.Equal(0, CountRows(connection, 2));
    }

    private static SqliteWriteContentionPolicy CreatePolicy(int maximumRetryCount) =>
        new(new SqliteWriteContentionOptions(
            commandTimeoutSeconds: 1,
            maximumRetryCount,
            retryDelay: TimeSpan.Zero));

    private static void CreateSyntheticTable(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE synthetic_write_probe (
                id INTEGER NOT NULL PRIMARY KEY,
                value TEXT NOT NULL
            );
            """;
        command.ExecuteNonQuery();
    }

    private static void InsertPolicyRow(
        SqliteConnection connection,
        SqliteTransaction transaction) =>
        InsertSyntheticRow(connection, transaction, 2, "policy-writer");

    private static void InsertSyntheticRow(
        SqliteConnection connection,
        SqliteTransaction transaction,
        long id,
        string value)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO synthetic_write_probe (id, value)
            VALUES ($id, $value);
            """;
        command.Parameters.AddWithValue("$id", id);
        command.Parameters.AddWithValue("$value", value);
        command.ExecuteNonQuery();
    }

    private static long CountRows(SqliteConnection connection, long id)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM synthetic_write_probe WHERE id = $id;";
        command.Parameters.AddWithValue("$id", id);
        return (long)command.ExecuteScalar()!;
    }

    private sealed class TemporarySqliteDatabase : IDisposable
    {
        private readonly string path = Path.Combine(
            Path.GetTempPath(),
            $"mu-build-planner-contention-tests-{Guid.NewGuid():N}.sqlite");

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
}
