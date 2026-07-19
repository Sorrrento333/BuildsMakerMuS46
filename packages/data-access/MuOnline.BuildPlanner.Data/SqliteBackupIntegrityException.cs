namespace MuOnline.BuildPlanner.Data;

public sealed class SqliteBackupIntegrityException : InvalidOperationException
{
    public SqliteBackupIntegrityException(string message)
        : base(message)
    {
    }

    public SqliteBackupIntegrityException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
