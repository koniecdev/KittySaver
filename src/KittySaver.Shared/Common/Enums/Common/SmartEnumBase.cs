using Ardalis.SmartEnum;

namespace KittySaver.Shared.Common.Enums.Common;


public abstract class SmartEnumBase<TEnum>(string name, int value) : SmartEnum<TEnum, int>(name, value)
    where TEnum : SmartEnum<TEnum, int>
{
    public static TEnum FromNameOrValue(string nameOrValue, bool ignoreCase = false)
    {
        return int.TryParse(nameOrValue, out int value) ? FromValue(value) : FromName(nameOrValue, ignoreCase);
    }
}