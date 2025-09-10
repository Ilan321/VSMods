using System.Reflection;

namespace MetalRecycling.Helpers;

public static class ReflectionHelpers
{
    public static T GetPrivateField<T>(object instance, string name) =>
        (T) instance
            ?.GetType()
            ?.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(instance);
}