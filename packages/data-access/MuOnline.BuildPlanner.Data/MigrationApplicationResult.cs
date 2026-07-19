namespace MuOnline.BuildPlanner.Data;

public sealed record MigrationApplicationResult(
    int AppliedCount,
    int AlreadyAppliedCount,
    long CurrentVersion);
