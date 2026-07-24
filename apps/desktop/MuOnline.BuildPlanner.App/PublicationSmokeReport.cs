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
    string? RulesetId,
    string? RulesetSnapshotPath,
    int ApprovedProgressionCaseCount,
    int RejectedProgressionCaseCount,
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
        RulesetId: null,
        RulesetSnapshotPath: null,
        ApprovedProgressionCaseCount: 0,
        RejectedProgressionCaseCount: 0,
        AppliedMigrationCount: 0,
        AlreadyAppliedMigrationCount: 0,
        ErrorType: exception.GetType().FullName,
        ErrorMessage: exception.Message);
}
