using BepInEx;
using System.ComponentModel;
using System.Reflection;

namespace BetterAmongUs.Modules.Support;

/// <summary>
/// Provides a mechanism to retrieve values from plugins through reflection-based property/field access.
/// </summary>
/// <typeparam name="T">The type of value to retrieve from the target plugin.</typeparam>
internal sealed class BAUSupportVar<T>(string attributeFlag)
{
    private readonly string _attributeFlag = attributeFlag;
    private readonly Dictionary<PluginInfo, Func<T?>?> _getters = [];

    /// <summary>
    /// Retrieves the value from the specified plugin that matches the configured attribute flag.
    /// </summary>
    /// <param name="pluginInfo">The plugin information for the target plugin.</param>
    /// <returns>
    /// The value of the field or property decorated with the matching <see cref="CategoryAttribute"/>,
    /// or <c>default(T)</c> if no matching member is found or the plugin instance is null.
    /// </returns>
    internal T? GetValue(PluginInfo pluginInfo)
    {
        if (pluginInfo == null || pluginInfo.Instance == null)
            return default;

        if (!_getters.TryGetValue(pluginInfo, out var getter))
        {
            getter = FindGetter(pluginInfo);
            if (getter == null)
            {
                getter = () => default;
            }
            _getters[pluginInfo] = getter;
        }

        return getter();
    }

    /// <summary>
    /// Locates a static field or property in the plugin's assembly that is decorated with the
    /// configured <see cref="CategoryAttribute"/>.
    /// </summary>
    /// <param name="pluginInfo">The plugin information for the target plugin.</param>
    /// <returns>
    /// A function delegate that retrieves the value from the matching member,
    /// or <c>null</c> if no matching member is found.
    /// </returns>
    private Func<T?>? FindGetter(PluginInfo pluginInfo)
    {
        foreach (var type in pluginInfo.Instance.GetType().Assembly.GetTypes())
        {
            // Search for matching static fields
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                var attr = field.GetCustomAttribute<CategoryAttribute>();
                if (attr != null && attr.Category == _attributeFlag)
                {
                    return () => (T?)field.GetValue(null);
                }
            }

            // Search for matching static properties
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                var attr = prop.GetCustomAttribute<CategoryAttribute>();
                if (attr != null && attr.Category == _attributeFlag)
                {
                    return () => (T?)prop.GetValue(null);
                }
            }
        }

        return null;
    }
}