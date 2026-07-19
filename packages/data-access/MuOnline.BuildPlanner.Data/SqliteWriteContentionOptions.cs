namespace MuOnline.BuildPlanner.Data;

public sealed class SqliteWriteContentionOptions
{
    private static readonly TimeSpan MaximumRetryDelay = TimeSpan.FromMilliseconds(int.MaxValue);

    public SqliteWriteContentionOptions(
        int commandTimeoutSeconds,
        int maximumRetryCount,
        TimeSpan retryDelay)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(commandTimeoutSeconds);
        ArgumentOutOfRangeException.ThrowIfNegative(maximumRetryCount);
        if (maximumRetryCount == int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumRetryCount),
                maximumRetryCount,
                "Maximum retry count must leave room for the initial attempt.");
        }

        if (retryDelay < TimeSpan.Zero || retryDelay > MaximumRetryDelay)
        {
            throw new ArgumentOutOfRangeException(
                nameof(retryDelay),
                retryDelay,
                $"Retry delay must be between zero and {MaximumRetryDelay}.");
        }

        CommandTimeoutSeconds = commandTimeoutSeconds;
        MaximumRetryCount = maximumRetryCount;
        RetryDelay = retryDelay;
    }

    public int CommandTimeoutSeconds { get; }

    public int MaximumRetryCount { get; }

    public TimeSpan RetryDelay { get; }
}
