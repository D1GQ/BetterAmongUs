using AmongUs.InnerNet.GameDataMessages;
using BetterAmongUs.Interfaces;
using System.Reflection;

namespace BetterAmongUs.Attributes;

/// <summary>
/// Provides a base attribute class that extends <see cref="AutoRegisterAttribute{T}"/> with identifier-based lookup capabilities.
/// </summary>
/// <typeparam name="AT">The self attribute type.</typeparam>
/// <typeparam name="T">The base type or interface that attributed classes must implement.</typeparam>
/// <typeparam name="Id">The type used as a key for instance lookup.</typeparam>
/// <param name="identifier">The specific identifier value to associate with this attribute.</param>
[AttributeUsage(AttributeTargets.Class)]
internal abstract class AutoRegisterLookupAttribute<AT, T, Id>(Id identifier) : AutoRegisterAttribute<T> where AT : AutoRegisterAttribute<T> where T : class where Id : notnull
{
    /// <summary>
    /// The identifier value associated with instances registered by this attribute.
    /// </summary>
    private readonly Id _identifier = identifier;

    /// <summary>
    /// Static dictionary that maps identifier values to their corresponding instances for fast lookup.
    /// </summary>
    private static readonly Dictionary<Id, T> InstanceLookup = [];

    /// <summary>
    /// Static dictionary that maps instances values to their corresponding attribute for fast lookup.
    /// </summary>
    private static readonly Dictionary<T, AT> AttributeLookup = [];

    /// <summary>
    /// Gets the identifier associated with this attribute instance for lookup purposes.
    /// </summary>
    /// <returns>The value that serves as the unique identifier for this registered instance.</returns>
    internal override object GetIdentifier()
    {
        return _identifier;
    }

    /// <summary>
    /// Retrieves an instance from the lookup dictionary by its associated identifier.
    /// </summary>
    /// <param name="identifier">The identifier value to look up.</param>
    /// <returns>The instance associated with the specified identifier, or null if not found.</returns>
    internal static T? GetInstanceFromLookup(Id identifier)
    {
        if (InstanceLookup.TryGetValue(identifier, out var instanceLookup))
        {
            return instanceLookup;
        }

        return null;
    }

    /// <summary>
    /// Attempts to retrieve an instance from the lookup dictionary by its associated identifier.
    /// </summary>
    /// <param name="identifier">The identifier value to look up.</param>
    /// <param name="instance">When this method returns, contains the instance associated with the specified identifier, or <see langword="null"/> if the identifier is not found.</param>
    /// <returns>
    /// <see langword="true"/> if the lookup dictionary contains an instance with the specified identifier; otherwise, <see langword="false"/>.
    /// </returns>
    internal static bool TryGetInstanceFromLookup(Id identifier, out T instance)
    {
        if (InstanceLookup.TryGetValue(identifier, out var instanceLookup))
        {
            instance = instanceLookup;
            return true;
        }

        instance = null!;
        return false;
    }

    /// <summary>
    /// Retrieves an attribute from the lookup dictionary by its associated instance.
    /// </summary>
    /// <param name="instance">The instance value to look up.</param>
    /// <returns>The attribute associated with the specified instance, or null if not found.</returns>
    internal static AT? GetAttributeFromLookup(T instance)
    {
        if (AttributeLookup.TryGetValue(instance, out var attributeLookup))
        {
            return attributeLookup;
        }

        return null;
    }

    /// <summary>
    /// Attempts to retrieve an attribute from the lookup dictionary by its associated instance.
    /// </summary>
    /// <param name="instance">The instance value to look up.</param>
    /// <param name="attribute">When this method returns, contains the attribute associated with the specified instance, or <see langword="null"/> if the instance is not found.</param>
    /// <returns>
    /// <see langword="true"/> if the lookup dictionary contains an attribute with the specified instance; otherwise, <see langword="false"/>.
    /// </returns>
    internal static bool TryGetAttributeFromLookup(T instance, out AT attribute)
    {
        if (AttributeLookup.TryGetValue(instance, out var attributeLookup))
        {
            attribute = attributeLookup;
            return true;
        }

        attribute = null!;
        return false;
    }

    /// <inheritdoc/>
    protected override void RegisterInstances()
    {
        var attributeType = GetType();

        var attributedTypes = BAUPlugin.ModInfo.Assembly.GetTypes().Where(t => !t.IsAbstract && !t.IsInterface).ToArray();

        foreach (var type in attributedTypes)
        {
            if (typeof(T).IsAssignableFrom(type))
            {
                var attributes = type.GetCustomAttributes(attributeType, false);

                foreach (var attribute in attributes)
                {
                    if (attribute is AT autoRegisterAttribute)
                    {
                        var constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, Type.EmptyTypes, null);

                        Id identifierValue = (Id)autoRegisterAttribute.GetIdentifier()!;
                        if (constructor != null && constructor.Invoke(null) is T instance)
                        {
                            _instances.Add(instance);
                            InstanceLookup[identifierValue] = instance;
                            AttributeLookup[instance] = autoRegisterAttribute;
                        }
                    }
                }
            }
        }
    }
}

[AttributeUsage(AttributeTargets.Class)]
internal sealed class RegisterRpcHandler(object @enum) : AutoRegisterLookupAttribute<RegisterRpcHandler, IRpcHandler, Enum>((Enum)@enum);

[AttributeUsage(AttributeTargets.Class)]
internal sealed class RegisterGameDataHandler(GameDataTypes gameDataType) : AutoRegisterLookupAttribute<RegisterGameDataHandler, IGameDataHandler, GameDataTypes>(gameDataType);