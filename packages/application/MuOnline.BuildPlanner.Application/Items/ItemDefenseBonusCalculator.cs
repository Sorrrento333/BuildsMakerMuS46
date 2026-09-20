namespace MuOnline.BuildPlanner.Application.Items;

public static class ItemDefenseBonusCalculator
{
    private const int PercentagePerLevel = 5;

    public static long? Calculate(long? baseDefense, int itemLevel)
    {
        if (baseDefense is null)
        {
            return null;
        }

        if (itemLevel < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(itemLevel),
                "Item level cannot be negative.");
        }

        var scalePercent = checked(100 + PercentagePerLevel * itemLevel);
        var rawDefense = checked((decimal)baseDefense.Value * scalePercent) / 100m;
        return (long)rawDefense;
    }
}