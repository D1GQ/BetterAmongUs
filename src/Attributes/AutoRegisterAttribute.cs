using BetterAmongUs.Commands;
using BetterAmongUs.Modules.AntiCheat;
using System.Reflection;
using System.Runtime.Serialization;

namespace BetterAmongUs.Attributes;

/// <summary>
/// Base attribute class for automatically discovering and registering instances of specific types through reflection.
/// </summary>
internal abstract class AutoRegisterAttribute : Attribute
{
    /// <summary>
    /// Scans the entire assembly and registers all instances of classes marked with <see cref="AutoRegisterAttribute"/> subclasses.
    /// </summary>
    internal static void Initialize()
    {
        var types = ModInfo.Assembly.GetTypes();

        foreach (var type in types)
        {
            if (type.IsAbstract || !type.IsSealed)
                continue;

            if (!typeof(AutoRegisterAttribute).IsAssignableFrom(type))
                continue;

            var tempAttribute = (AutoRegisterAttribute)FormatterServices.GetUninitializedObject(type);
            tempAttribute.Register();
        }
    }

    /// <summary>
    /// When implemented in a derived class, registers individual instances.
    /// </summary>
    protected abstract void Register();
}

/// <summary>
/// Generic attribute for automatically registering static instances of a specified base type or interface.
/// </summary>
/// <typeparam name="T">The base type or interface that attributed classes must implement.</typeparam>
[AttributeUsage(AttributeTargets.Class)]
internal abstract class AutoRegisterAttribute<T> : AutoRegisterAttribute where T : class
{
    /// <summary>
    /// The collection of all discovered and registered instances of type <typeparamref name="T"/>.
    /// </summary>
    protected static readonly List<T> _instances = [];

    /// <summary>
    /// Gets a read-only collection of all registered instances of type <typeparamref name="T"/>.
    /// </summary>
    internal static IReadOnlyList<T> Instances => _instances.AsReadOnly();

    /// <summary>
    /// Retrieves a specific instance by its concrete type.
    /// </summary>
    /// <typeparam name="J">The concrete type of the instance to retrieve. Must inherit from or implement <typeparamref name="T"/>.</typeparam>
    /// <returns>The instance of type <typeparamref name="J"/> if found, otherwise null.</returns>
    internal static J? GetInstance<J>() where J : T => (J?)_instances.FirstOrDefault(instance => instance.GetType() == typeof(J));

    /// <inheritdoc/>
    protected override void Register()
    {
        var types = ModInfo.Assembly.GetTypes();

        foreach (var type in types)
        {
            if (type.GetCustomAttribute(GetType()) == null)
                continue;

            if (type.IsAbstract || type.IsInterface)
                continue;

            if (typeof(T).IsAssignableFrom(type))
            {
                var constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, Type.EmptyTypes, null);

                if (constructor != null)
                {
                    if (constructor.Invoke(null) is T instance)
                    {
                        _instances.Add(instance);
                    }
                }
            }
        }
    }
}

// Class instances
internal sealed class RegisterCommandAttribute : AutoRegisterAttribute<BaseCommand>
{
}

internal sealed class RegisterRPCHandlerAttribute : AutoRegisterAttribute<RPCHandler>
{
}