using System.Reflection;

namespace VersionChecker;

internal static class ReflectionUtils
{
    internal static T? GetField<T>(object o, string fieldName)
        where T : class
    {
        var type = o.GetType();

        var pi = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        return pi.GetValue(o) as T;
    }
}
