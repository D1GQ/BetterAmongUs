using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime;
using System.Collections;
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
    /// Static dictionary mapping base MonoBehaviour types to their extension pairs.
    /// </summary>
    private static readonly Dictionary<Type, List<ExtensionPair>> _extensionsByBaseType = [];

    /// <summary>
    /// Represents a pairing between a base MonoBehavior and its extension.
    /// </summary>
    private struct ExtensionPair
    {
        /// <summary>
        /// The base MonoBehavior.
        /// </summary>
        internal MonoBehaviour? Base;

        /// <summary>
        /// The extension attached to the base.
        /// </summary>
        internal IMonoExtension? Extension;
    }

    /// <summary>
    /// Removes all entries from the lookup dictionary where the base MonoBehaviour or extension has been destroyed.
    /// </summary>
    private static void CleanupLookups()
    {
        foreach (var kvp in _extensionsByBaseType.ToArray())
        {
            var extensions = kvp.Value;

            for (int i = extensions.Count - 1; i >= 0; i--)
            {
                var pair = extensions[i];

                bool baseIsDead = pair.Base == null || pair.Base.IsDestroyedOrNull();
                bool extensionIsDead = pair.Extension == null ||
                                       (pair.Extension as MonoBehaviour)?.IsDestroyedOrNull() != false;

                if (baseIsDead || extensionIsDead)
                {
                    if (pair.Extension != null && !extensionIsDead)
                    {
                        pair.Extension.OnDestroy();
                    }

                    extensions.RemoveAt(i);
                }
            }

            if (extensions.Count == 0)
            {
                _extensionsByBaseType.Remove(kvp.Key);
            }
        }
    }

    /// <summary>
    /// Determines whether a MonoBehaviour has an extension of the specified type attached to it.
    /// </summary>
    /// <typeparam name="T">The type of IMonoExtension to check for.</typeparam>
    /// <param name="monoBehaviour">The MonoBehaviour to check for the extension.</param>
    /// <returns>true if an extension of type T exists on the MonoBehaviour; otherwise, false.</returns>
    internal static bool HasExtension<T>(MonoBehaviour monoBehaviour) where T : IMonoExtension
    {
        if (monoBehaviour == null || monoBehaviour.IsDestroyedOrNull())
            return false;

        CleanupLookups();

        if (_extensionsByBaseType.TryGetValue(monoBehaviour.GetType(), out var extensions))
        {
            foreach (var pair in extensions)
            {
                if (pair.Base == monoBehaviour && pair.Extension is T)
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets an existing extension of the specified type attached to a MonoBehaviour.
    /// </summary>
    /// <typeparam name="T">The type of IMonoExtension to retrieve.</typeparam>
    /// <param name="monoBehaviour">The MonoBehaviour to get the extension from.</param>
    /// <returns>The extension instance if found, otherwise default(T).</returns>
    internal static T? GetExtension<T>(MonoBehaviour monoBehaviour) where T : IMonoExtension
    {
        if (monoBehaviour == null || monoBehaviour.IsDestroyedOrNull())
            return default;

        CleanupLookups();

        if (_extensionsByBaseType.TryGetValue(monoBehaviour.GetType(), out var extensions))
        {
            foreach (var pair in extensions)
            {
                if (pair.Base == monoBehaviour && pair.Extension is T typedExtension)
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
        if (monoBehaviour == null || monoBehaviour.IsDestroyedOrNull())
            return null;

        CleanupLookups();
        T? existingComponent = monoBehaviour.GetComponent<T>();
        if (existingComponent != null)
        {
            var baseType = monoBehaviour.GetType();
            if (_extensionsByBaseType.TryGetValue(baseType, out var extensions))
            {
                bool isRegistered = extensions.Any(pair => pair.Base == monoBehaviour && pair.Extension == existingComponent);

                if (!isRegistered)
                {
                    extensions.Add(new ExtensionPair
                    {
                        Base = monoBehaviour,
                        Extension = existingComponent
                    });
                    existingComponent.BaseMono = monoBehaviour;
                    existingComponent.OnExtensionAwake(monoBehaviour);
                }
            }
            else
            {
                var newExtensions = new List<ExtensionPair>
                {
                    new() {
                        Base = monoBehaviour,
                        Extension = existingComponent
                    }
                };
                _extensionsByBaseType[baseType] = newExtensions;
                existingComponent.BaseMono = monoBehaviour;
                existingComponent.OnExtensionAwake(monoBehaviour);
            }

            return existingComponent;
        }

        var existingExtensions = monoBehaviour.GetComponentsInChildren(Il2CppType.From(typeof(T)), true);
        if (existingExtensions.Length > 0)
        {
            var found = existingExtensions.FirstOrDefault() as T;
            if (found != null)
                return found;
        }

        T? monoExtension = monoBehaviour.gameObject.AddComponent<T>();
        if (monoExtension != null)
        {
            var baseType = monoBehaviour.GetType();

            if (!_extensionsByBaseType.TryGetValue(baseType, out var extensions))
            {
                extensions = [];
                _extensionsByBaseType[baseType] = extensions;
            }

            extensions.Add(new ExtensionPair
            {
                Base = monoBehaviour,
                Extension = monoExtension
            });

            monoExtension.BaseMono = monoBehaviour;
            monoExtension.OnExtensionAwake(monoBehaviour);
            return monoExtension;
        }

        return null;
    }

    /// <summary>
    /// Tries to remove an extension from its attached MonoBehaviour.
    /// </summary>
    /// <param name="monoExtension">The extension instance to remove.</param>
    internal static void TryRemoveExtension(IMonoExtension monoExtension)
    {
        if (monoExtension == null)
            return;

        CleanupLookups();

        if (monoExtension.BaseMono != null)
        {
            var baseType = monoExtension.BaseMono.GetType();

            if (_extensionsByBaseType.TryGetValue(baseType, out var extensions))
            {
                for (int i = extensions.Count - 1; i >= 0; i--)
                {
                    if (extensions[i].Extension == monoExtension)
                    {
                        extensions.RemoveAt(i);
                        break;
                    }
                }

                if (extensions.Count == 0)
                {
                    _extensionsByBaseType.Remove(baseType);
                }
            }
        }
    }

    /// <summary>
    /// Runs a callback when a MonoBehavior extension becomes available.
    /// </summary>
    /// <typeparam name="T">The type of extension to wait for.</typeparam>
    /// <param name="mono">The base MonoBehavior.</param>
    /// <param name="getExtension">Function to retrieve the extension.</param>
    /// <param name="callback">Callback to execute when extension is available.</param>
    internal static void RunWhenNotNull<T>(MonoBehaviour mono, Func<T?> getExtension, Action<T> callback) where T : class, IMonoExtension
    {
        mono.StartCoroutine(CoWaitForExtension(getExtension, callback));
    }

    /// <summary>
    /// Coroutine that waits for an extension to become available.
    /// </summary>
    private static IEnumerator CoWaitForExtension<T>(Func<T?> getExtension, Action<T> callback) where T : class, IMonoExtension
    {
        T? extension;
        while ((extension = getExtension()) == null)
        {
            yield return null;
        }
        callback(extension);
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