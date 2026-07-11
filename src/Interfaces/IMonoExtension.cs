using BetterAmongUs.Modules;
using Il2CppInterop.Runtime;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace BetterAmongUs.Interfaces;

/// <summary>
/// Interface for MonoBehavior extensions.
/// Provides a standardized way to extend MonoBehaviour functionality
/// </summary>
internal interface IMonoExtension
{
    /// <summary>
    /// Gets or sets the base MonoBehavior this extension is attached to.
    /// </summary>
    /// <value>The base MonoBehaviour instance this extension extends.</value>
    MonoBehaviour? BaseMono { get; set; }

    /// <summary>
    /// Called when the extension is awakened. Initializes the extension with its base MonoBehaviour.
    /// </summary>
    /// <param name="baseMono">The base MonoBehaviour this extension is attached to.</param>
    void OnExtensionAwake(MonoBehaviour baseMono);

    /// <summary>
    /// Called when the extension is being destroyed.
    /// </summary>
    void OnDestroy();

    /// <summary>
    /// Static dictionary mapping MonoBehaviour types to their auto-extension types.
    /// </summary>
    private static readonly Dictionary<Type, Type> AutoExtensionTypeLookup = [];

    /// <summary>
    /// Static dictionary mapping MonoBehaviour instances to their active IMonoExtension instances.
    /// </summary>
    private static readonly ConditionalWeakTable<MonoBehaviour, List<IMonoExtension>> MonoToMonoExtensionLookup = [];

    /// <summary>
    /// Registers all auto-extension types from the assembly.
    /// </summary>
    internal static void RegisterAll()
    {
        var assembly = ModInfo.Assembly;

        foreach (var type in assembly.GetTypes())
        {
            if (type.IsInterface || type.IsAbstract)
                continue;

            if (!typeof(IMonoExtension).IsAssignableFrom(type))
                continue;

            if (type.IsGenericTypeDefinition)
                continue;

            var genericInterface = type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType &&
                                    i.GetGenericTypeDefinition() == typeof(IAutoMonoExtension<>));

            if (genericInterface == null)
                continue;

            var monoType = genericInterface.GetGenericArguments()[0];

            if (!monoType.IsSubclassOf(typeof(MonoBehaviour)))
                continue;

            AutoExtensionTypeLookup[monoType] = type;
        }
    }

    /// <summary>
    /// Gets an existing extension of the specified type attached to a MonoBehaviour.
    /// </summary>
    /// <typeparam name="T">The type of IMonoExtension to retrieve.</typeparam>
    /// <param name="monoBehaviour">The MonoBehaviour to get the extension from.</param>
    /// <returns>The extension instance if found, otherwise default(T).</returns>
    internal static T? GetExtension<T>(MonoBehaviour monoBehaviour) where T : IMonoExtension
    {
        if (monoBehaviour == null)
            return default;

        if (MonoToMonoExtensionLookup.TryGetValue(monoBehaviour, out var extensions))
        {
            foreach (var extension in extensions)
            {
                if (extension is T typedExtension)
                    return typedExtension;
            }
        }

        return default;
    }

    /// <summary>
    /// Adds a new extension component of the specified type to the MonoBehaviour's GameObject.
    /// </summary>
    /// <typeparam name="T">The type of MonoBehaviour and IMonoExtension to add.</typeparam>
    /// <param name="monoBehaviour">The MonoBehaviour to attach the extension to.</param>
    /// <returns>The newly created extension instance, or null if creation failed.</returns>
    internal static T? AddExtension<T>(MonoBehaviour monoBehaviour) where T : MonoBehaviour, IMonoExtension
    {
        if (monoBehaviour == null)
            return null;

        // Check if extension already exists
        var existing = GetExtension<T>(monoBehaviour);
        if (existing != null)
            return existing;

        T? monoExtension = monoBehaviour.gameObject.AddComponent<T>();
        if (monoExtension != null)
        {
            // Get or create the extension list for this MonoBehaviour
            if (!MonoToMonoExtensionLookup.TryGetValue(monoBehaviour, out var extensions))
            {
                extensions = [];
                MonoToMonoExtensionLookup.Add(monoBehaviour, extensions);
            }

            extensions.Add(monoExtension);
            monoExtension.BaseMono = monoBehaviour;
            monoExtension.OnExtensionAwake(monoBehaviour);
            return monoExtension;
        }

        return null;
    }

    /// <summary>
    /// Attempts to add an auto-extension to a MonoBehaviour if one is registered.
    /// </summary>
    /// <param name="monoBehaviour">The MonoBehaviour to add an auto-extension to.</param>
    internal static void TryAddAutoExtension(MonoBehaviour monoBehaviour)
    {
        if (monoBehaviour == null)
            return;

        var monoType = monoBehaviour.GetType();

        // Check if this type has an auto-extension registered
        if (!AutoExtensionTypeLookup.TryGetValue(monoType, out var extensionType))
            return;

        // Check if extension already exists for this MonoBehaviour instance
        if (MonoToMonoExtensionLookup.TryGetValue(monoBehaviour, out var existingExtensions))
        {
            foreach (var ext in existingExtensions)
            {
                if (ext.GetType() == extensionType)
                    return;
            }
        }

        try
        {
            IMonoExtension monoExtension = (IMonoExtension)monoBehaviour.gameObject.AddComponent(Il2CppType.From(extensionType));
            if (monoExtension != null)
            {
                // Get or create the extension list for this MonoBehaviour
                if (!MonoToMonoExtensionLookup.TryGetValue(monoBehaviour, out var extensions))
                {
                    extensions = [];
                    MonoToMonoExtensionLookup.Add(monoBehaviour, extensions);
                }

                extensions.Add(monoExtension);
                monoExtension.BaseMono = monoBehaviour;
                monoExtension.OnExtensionAwake(monoBehaviour);
            }
        }
        catch (Exception ex)
        {
            Logger_.Error($"Failed to add auto-extension {extensionType.Name} to {monoType.Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// Tries to removes an extension from its attached MonoBehaviour.
    /// </summary>
    /// <param name="monoExtension">The extension instance to remove.</param>
    internal static void TryRemoveExtension(IMonoExtension monoExtension)
    {
        if (monoExtension == null)
            return;

        if (monoExtension.BaseMono != null)
        {
            if (MonoToMonoExtensionLookup.TryGetValue(monoExtension.BaseMono, out var extensions))
            {
                extensions.Remove(monoExtension);
            }
        }
    }
}

/// <summary>
/// Generic interface for MonoBehavior extensions with specific base type.
/// </summary>
/// <typeparam name="T">The type of MonoBehavior this extension attaches to. Must inherit from MonoBehaviour.</typeparam>
internal interface IMonoExtension<T> : IMonoExtension where T : MonoBehaviour
{
    /// <summary>
    /// Gets or sets the base MonoBehavior of type T.
    /// </summary>
    /// <value>The strongly-typed base MonoBehaviour instance.</value>
    new T? BaseMono { get; set; }

    /// <summary>
    /// Explicit interface implementation for non-generic BaseMono.
    /// Casts the base MonoBehaviour to type T.
    /// </summary>
    MonoBehaviour? IMonoExtension.BaseMono
    {
        get => BaseMono;
        set => BaseMono = value as T;
    }

    /// <summary>
    /// Called when the extension is awakened and attached to its base MonoBehaviour.
    /// </summary>
    /// <param name="baseMono">The base MonoBehaviour instance of type T.</param>
    void OnExtensionAwake(T baseMono);

    /// <summary>
    /// Explicit implementation of IMonoExtension.OnExtensionAwake that calls
    /// the strongly-typed OnExtensionAwake method with the correct type.
    /// </summary>
    /// <param name="baseMono">The base MonoBehaviour to cast to type T.</param>
    void IMonoExtension.OnExtensionAwake(MonoBehaviour baseMono)
    {
        OnExtensionAwake((T)baseMono);
    }
}

/// <summary>
/// Marker interface for auto-registering extensions.
/// When a type implements this interface with a specific MonoBehaviour type T, it will be automatically registered and attached to matching MonoBehaviour instances.
/// </summary>
/// <typeparam name="T">The type of MonoBehaviour this extension automatically attaches to.</typeparam>
internal interface IAutoMonoExtension<T> : IMonoExtension<T> where T : MonoBehaviour;