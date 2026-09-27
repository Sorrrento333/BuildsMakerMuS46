namespace MuOnline.BuildPlanner.Application.Items;

public static class ItemOptionBonusCalculator
{
    private const long StrengthPerOptionLevel = 5;
    private const string StrengthStatId = "strength";

    public static IReadOnlyDictionary<string, long> ApplyToRequiredStats(
        IReadOnlyDictionary<string, long> baseRequiredStats,
        int optionLevel)
    {
        ArgumentNullException.ThrowIfNull(baseRequiredStats);
        if (optionLevel < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(optionLevel),
                "Jewel of Life option level cannot be negative.");
        }

        if (optionLevel == 0 ||
            !baseRequiredStats.TryGetValue(StrengthStatId, out var baseStrength))
        {
            return baseRequiredStats;
        }

        var effective = new Dictionary<string, long>(
            baseRequiredStats,
            StringComparer.Ordinal)
        {
            [StrengthStatId] = checked(
                baseStrength + StrengthPerOptionLevel * optionLevel),
        };
        return effective;
    }
}
