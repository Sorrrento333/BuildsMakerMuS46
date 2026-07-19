using Microsoft.Data.Sqlite;

namespace MuOnline.BuildPlanner.Data;

public sealed class SqliteWriteContentionException : InvalidOperationException
{
    public SqliteWriteContentionException(
        int attemptCount,
        SqliteWriteContentionOptions options,
        SqliteException innerException)
        : base(
            $"SQLite remained locked after {attemptCount} write attempt(s) " +
            $"with a {options.CommandTimeoutSeconds}-second timeout and " +
            $"{options.MaximumRetryCount} configured retry attempt(s).",
            innerException)
    {
        AttemptCount = attemptCount;
        CommandTimeoutSeconds = options.CommandTimeoutSeconds;
        MaximumRetryCount = options.MaximumRetryCount;
    }

    public int AttemptCount { get; }

    public int CommandTimeoutSeconds { get; }

    public int MaximumRetryCount { get; }
}
