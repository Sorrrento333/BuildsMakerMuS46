namespace MuOnline.BuildPlanner.Domain.Stats;

public static class StatDistributionErrorCodes
{
    public const string AllocationNegative = "allocation-negative";
    public const string StatNotAvailable = "stat-not-available";
    public const string StatAllocationMissing = "stat-allocation-missing";
    public const string AllocationExceedsEarnedPoints = "allocation-exceeds-earned-points";
    public const string AllocationOverflow = "allocation-overflow";
    public const string BudgetSourceMismatch = "budget-source-mismatch";
    public const string ResetCountNegative = "reset-count-negative";
    public const string PointsPerResetNegative = "points-per-reset-negative";
    public const string ResetPointsOverflow = "reset-points-overflow";
    public const string TotalDistributablePointsOverflow =
        "total-distributable-points-overflow";
}

public sealed class StatDistributionException : Exception
{
    public StatDistributionException(string code, string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
    }

    public string Code { get; }
}
