using System.Reflection;

namespace KittySaver.Domain.Tests.Unit;

public static class SharedHelper
{
    public static void SetBackingField(object sut, string propertyName, object? objectToSet)
    {
        FieldInfo? backingField = sut.GetType().GetField($"<{propertyName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        if (backingField == null)
        {
            backingField = sut.GetType().BaseType?.GetField($"<{propertyName}>k__BackingField", 
                BindingFlags.Instance | BindingFlags.NonPublic);
        }
        backingField?.SetValue(sut, objectToSet);
    }
    public static void SetBackingFieldDirectly(object sut, string backingFieldName, object? objectToSet)
    {
        FieldInfo? backingField = sut.GetType()
            .GetField(backingFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        backingField?.SetValue(sut, objectToSet);
    }
}