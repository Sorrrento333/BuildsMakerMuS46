using System.Data;
using System.Diagnostics;
using Microsoft.Data.Sqlite;

namespace MuOnline.BuildPlanner.Data;

public sealed class SqliteWriteContentionPolicy
{
    private const int SqliteBusy = 5;
    private const int SqliteLocked = 6;

    private readonly SqliteWriteContentionOptions options;

    public SqliteWriteContentionPolicy(SqliteWriteContentionOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        this.options = options;
    }

    public SqliteWriteExecutionResult Execute(
        SqliteConnection connection,
        Action<SqliteConnection, SqliteTransaction> operation,
        Action<int>? retrying = null)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(operation);

        if (connection.State != ConnectionState.Open)
        {
            throw new InvalidOperationException("The SQLite connection must be open.");
        }

        var originalTimeout = connection.DefaultTimeout;
        connection.DefaultTimeout = options.CommandTimeoutSeconds;

        try
        {
            var maximumAttemptCount = checked(options.MaximumRetryCount + 1);
            for (var attempt = 1; attempt <= maximumAttemptCount; attempt++)
            {
                try
                {
                    using var transaction = connection.BeginTransaction(deferred: false);
                    operation(connection, transaction);
                    transaction.Commit();
                    return new SqliteWriteExecutionResult(attempt);
                }
                catch (SqliteException exception) when (IsContention(exception))
                {
                    if (attempt == maximumAttemptCount)
                    {
                        throw new SqliteWriteContentionException(attempt, options, exception);
                    }

                    retrying?.Invoke(attempt);
                    if (options.RetryDelay > TimeSpan.Zero)
                    {
                        Thread.Sleep(options.RetryDelay);
                    }
                }
            }
        }
        finally
        {
            connection.DefaultTimeout = originalTimeout;
        }

        throw new UnreachableException();
    }

    private static bool IsContention(SqliteException exception) =>
        exception.SqliteErrorCode is SqliteBusy or SqliteLocked;
}
