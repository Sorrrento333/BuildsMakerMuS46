namespace MuOnline.BuildPlanner.Data;

public sealed class MigrationIntegrityException : InvalidOperationException
{
    public MigrationIntegrityException(string message)
        : base(message)
    {
    }
}
