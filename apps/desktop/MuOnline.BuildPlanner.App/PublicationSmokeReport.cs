namespace MuOnline.BuildPlanner.App;

internal sealed record PublicationSmokeReport(
    bool Success,
    string Phase,
    string? SqliteVersion,
    string? IntegrityResult,
    string? PersistedValue,
    string? DataDirectory,
    string? DatabasePath,
    string? BackupPath,
    string? BinaryDirectory,
    bool DataOutsideBinaryDirectory,
    int AppliedMigrationCount,
    int AlreadyAppliedMigrationCount,
    string? ErrorType,
    string? ErrorMessage)
{
    public static PublicationSmokeReport Failed(Exception exception) => new(
        Success: false,
        Phase: "failed",
        SqliteVersion: null,
        IntegrityResult: null,
        PersistedValue: null,
        DataDirectory: null,
        DatabasePath: null,
        BackupPath: null,
        BinaryDirectory: AppContext.BaseDirectory,
        DataOutsideBinaryDirectory: false,
        AppliedMigrationCount: 0,
        AlreadyAppliedMigrationCount: 0,
        ErrorType: exception.GetType().FullName,
        ErrorMessage: exception.Message);
}
