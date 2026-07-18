using BepInEx;
using BepInEx.Unity.IL2CPP;
using System.ComponentModel;
using System.Reflection;

namespace BetterAmongUs.Modules.Support;

/// <summary>
/// Base class for reflection-based event systems that allow plugins to interact with BetterAmongUs.
/// Provides infrastructure for discovering and invoking static methods decorated with specific
/// <see cref="CategoryAttribute"/> flags across all loaded plugins.
/// </summary>
internal abstract class BAUSupportEvent(string attributeFlag)
{
    /// <summary>
    /// Gets the expected parameter types for the event handler method.
    /// </summary>
    protected abstract Type[] ArgTypes { get; }

    /// <summary>
    /// The category attribute flag used to identify target methods.
    /// </summary>
    protected string _attributeFlag = attributeFlag;

    /// <summary>
    /// Cache of discovered methods per plugin to avoid repeated reflection lookups.
    /// </summary>
    protected readonly Dictionary<PluginInfo, MethodInfo?> _methods = [];

    /// <summary>
    /// Invokes the event handler on all loaded plugins and collects their return values.
    /// </summary>
    /// <typeparam name="T">The expected return type of the event handler.</typeparam>
    /// <param name="args">The arguments to pass to the event handler method.</param>
    /// <returns>
    /// An array containing the return values from all plugins that have a matching event handler.
    /// Returns <c>default(T)</c> for plugins without a handler or if the handler returns null.
    /// </returns>
    protected T?[] InvokeMethodAll<T>(params object[] args)
    {
        List<T?> list = [];
        foreach (var pluginInfo in IL2CPPChainloader.Instance.Plugins.Values)
        {
            if (pluginInfo == null)
                continue;

            list.Add(InvokeMethod<T>(pluginInfo, args));
        }

        return [.. list];
    }

    /// <summary>
    /// Invokes the event handler on a specific plugin.
    /// </summary>
    /// <typeparam name="T">The expected return type of the event handler.</typeparam>
    /// <param name="pluginInfo">The plugin to invoke the handler on.</param>
    /// <param name="args">The arguments to pass to the event handler method.</param>
    /// <returns>
    /// The return value from the event handler, or <c>default(T)</c> if no handler is found
    /// or an exception occurs during invocation.
    /// </returns>
    protected T? InvokeMethod<T>(PluginInfo pluginInfo, params object[] args)
    {
        if (pluginInfo == null || pluginInfo.Instance == null)
            return default;

        if (!_methods.TryGetValue(pluginInfo, out var method))
        {
            method = FindMethod(pluginInfo);
            _methods[pluginInfo] = method;
        }

        if (method == null)
            return default;

        try
        {
            return (T?)method.Invoke(null, args);
        }
        catch (Exception ex)
        {
            Logger_.Error($"Invoking event '{_attributeFlag}' for {pluginInfo.GetType().Name}: {ex}");
            return default;
        }
    }

    /// <summary>
    /// Finds a static method in the plugin's assembly that matches the configured attribute flag and parameter types.
    /// </summary>
    /// <param name="pluginInfo">The plugin to search for matching methods.</param>
    /// <returns>
    /// A <see cref="MethodInfo"/> representing the matching method, or <c>null</c> if no matching method is found.
    /// </returns>
    private MethodInfo? FindMethod(PluginInfo pluginInfo)
    {
        var assembly = pluginInfo.Instance.GetType().Assembly;
        var expectedArgTypes = ArgTypes;

        foreach (var type in assembly.GetTypes())
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                var attr = method.GetCustomAttribute<CategoryAttribute>();
                if (attr == null || attr.Category != _attributeFlag)
                    continue;

                var parameters = method.GetParameters();
                if (parameters.Length != expectedArgTypes.Length)
                    continue;

                bool match = true;
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].ParameterType != expectedArgTypes[i])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    return method;
                }
            }
        }

        return null;
    }
}


/// <summary>
/// Represents an event with no parameters that plugins can handle.
/// </summary>
/// <typeparam name="ReturnT">The return type of the event handler.</typeparam>
internal class BAUSupportEvent<ReturnT>(string attributeFlag) : BAUSupportEvent(attributeFlag)
{
    /// <inheritdoc/>
    protected override Type[] ArgTypes => [];

    /// <summary>
    /// Invokes the event handler on a specific plugin.
    /// </summary>
    /// <param name="pluginInfo">The plugin to invoke the handler on.</param>
    /// <returns>The return value from the event handler, or <c>default(ReturnT)</c> if not found.</returns>
    internal ReturnT? Invoke(PluginInfo pluginInfo)
    {
        return InvokeMethod<ReturnT>(pluginInfo);
    }

    /// <summary>
    /// Invokes the event handler on all loaded plugins and collects their return values.
    /// </summary>
    /// <returns>An array containing the return values from all plugins that have a matching event handler.</returns>
    internal ReturnT?[] InvokeAll()
    {
        return InvokeMethodAll<ReturnT>();
    }
}

/// <summary>
/// Represents an event with one parameter that plugins can handle.
/// </summary>
/// <typeparam name="ReturnT">The return type of the event handler.</typeparam>
/// <typeparam name="Arg1">The type of the first parameter.</typeparam>
internal sealed class BAUSupportEvent<ReturnT, Arg1>(string attributeFlag) : BAUSupportEvent(attributeFlag)
{
    /// <inheritdoc/>
    protected override Type[] ArgTypes => [typeof(Arg1)];

    /// <summary>
    /// Invokes the event handler on a specific plugin.
    /// </summary>
    /// <param name="pluginInfo">The plugin to invoke the handler on.</param>
    /// <param name="arg1">The first argument to pass to the event handler.</param>
    /// <returns>The return value from the event handler, or <c>default(ReturnT)</c> if not found.</returns>
    internal ReturnT? Invoke(PluginInfo pluginInfo, Arg1 arg1)
    {
        return InvokeMethod<ReturnT>(pluginInfo, arg1);
    }

    /// <summary>
    /// Invokes the event handler on all loaded plugins and collects their return values.
    /// </summary>
    /// <param name="arg1">The first argument to pass to all event handlers.</param>
    /// <returns>An array containing the return values from all plugins that have a matching event handler.</returns>
    internal ReturnT?[] InvokeAll(Arg1 arg1)
    {
        return InvokeMethodAll<ReturnT>(arg1);
    }
}

/// <summary>
/// Represents an event with two parameters that plugins can handle.
/// </summary>
/// <typeparam name="ReturnT">The return type of the event handler.</typeparam>
/// <typeparam name="Arg1">The type of the first parameter.</typeparam>
/// <typeparam name="Arg2">The type of the second parameter.</typeparam>
internal sealed class BAUSupportEvent<ReturnT, Arg1, Arg2>(string attributeFlag) : BAUSupportEvent(attributeFlag)
{
    /// <inheritdoc/>
    protected override Type[] ArgTypes => [typeof(Arg1), typeof(Arg2)];

    /// <summary>
    /// Invokes the event handler on a specific plugin.
    /// </summary>
    /// <param name="pluginInfo">The plugin to invoke the handler on.</param>
    /// <param name="arg1">The first argument to pass to the event handler.</param>
    /// <param name="arg2">The second argument to pass to the event handler.</param>
    /// <returns>The return value from the event handler, or <c>default(ReturnT)</c> if not found.</returns>
    internal ReturnT? Invoke(PluginInfo pluginInfo, Arg1 arg1, Arg2 arg2)
    {
        return InvokeMethod<ReturnT>(pluginInfo, arg1, arg2);
    }

    /// <summary>
    /// Invokes the event handler on all loaded plugins and collects their return values.
    /// </summary>
    /// <param name="arg1">The first argument to pass to all event handlers.</param>
    /// <param name="arg2">The second argument to pass to all event handlers.</param>
    /// <returns>An array containing the return values from all plugins that have a matching event handler.</returns>
    internal ReturnT?[] InvokeAll(Arg1 arg1, Arg2 arg2)
    {
        return InvokeMethodAll<ReturnT>(arg1, arg2);
    }
}