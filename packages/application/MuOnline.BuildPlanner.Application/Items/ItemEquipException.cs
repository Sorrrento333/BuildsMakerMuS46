namespace MuOnline.BuildPlanner.Application.Items;

public static class ItemEquipErrorCodes
{
    public const string ItemNotFound = "item-equip-not-found";
    public const string ClassNotAllowed = "item-equip-class-not-allowed";
    public const string RequirementsNotMet = "item-equip-requirements-not-met";
    public const string LevelOutOfRange = "item-equip-level-out-of-range";
}

public sealed class ItemEquipException : Exception
{
    public ItemEquipException(string code, string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
    }

    public ItemEquipException(string code, string message, Exception innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
    }

    public string Code { get; }
}
