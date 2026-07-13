using HarmonyLib;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
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
    /// Static dictionary mapping MonoBehaviour instances to their active IMonoExtension instances.
    /// </summary>
    private static readonly ConditionalWeakTable<MonoBehaviour, List<IMonoExtension>> MonoToMonoExtensionLookup = [];

    /// <summary>
    /// Determines whether a MonoBehaviour has an extension of the specified type attached to it.
    /// </summary>
    /// <typeparam name="T">The type of IMonoExtension to check for.</typeparam>
    /// <param name="monoBehaviour">The MonoBehaviour to check for the extension.</param>
    /// <returns>true if an extension of type T exists on the MonoBehaviour; otherwise, false.</returns>
    internal static bool HasExtension<T>(MonoBehaviour monoBehaviour) where T : IMonoExtension
    {
        if (monoBehaviour == null)
            return false;

        if (MonoToMonoExtensionLookup.TryGetValue(monoBehaviour, out var extensions))
        {
            foreach (var extension in extensions)
            {
                if (extension is T)
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
/// Provides functionality to patch MonoBehaviour methods and apply extension patches.
/// </summary>
internal interface IMonoExtensionPatcher
{
    /// <summary>
    /// Gets the target patch information.
    /// </summary>
    TargetPatch Target { get; }

    /// <summary>
    /// Adds an extension patch to the specified MonoBehaviour.
    /// </summary>
    /// <param name="monoBehaviour">The MonoBehaviour to patch.</param>
    void AddExtensionPatch(MonoBehaviour monoBehaviour);

    /// <summary>
    /// Represents a target method to be patched.
    /// </summary>
    /// <param name="Type">The type containing the method.</param>
    /// <param name="MethodName">The name of the method to patch.</param>
    internal sealed record TargetPatch(Type Type, string MethodName);

    /// <summary>
    /// Patches all IMonoExtensionPatcher implementations found in the mod assembly.
    /// </summary>
    internal static void PatchAll()
    {
        var assembly = ModInfo.Assembly;
        var patcherTypes = assembly.GetTypes()
            .Where(t => typeof(IMonoExtensionPatcher).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .ToList();

        // Group patchers by their target method to avoid duplicate patches
        var patchersByTarget = patcherTypes
            .Select(t => (Type: t, Instance: (IMonoExtensionPatcher)FormatterServices.GetUninitializedObject(t)))
            .GroupBy(x => x.Instance.Target)
            .ToList();

        foreach (var group in patchersByTarget)
        {
            var target = group.Key;
            var patcherTypesForTarget = group.Select(x => x.Type).ToList();

            var originalMethod = target.Type.GetMethod(target.MethodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (originalMethod == null)
                continue;

            _monoBehaviourToPatcherMap[target.Type] = patcherTypesForTarget;

            var postfixMethod = typeof(IMonoExtensionPatcher).GetMethod(nameof(Postfix),
                BindingFlags.Static | BindingFlags.NonPublic);

            BAUPlugin.Harmony.Patch(originalMethod, postfix: new HarmonyMethod(postfixMethod));
        }
    }

    /// <summary>
    /// Patches a specific IMonoExtensionPatcher implementation.
    /// </summary>
    /// <param name="harmony">The Harmony instance to use for patching.</param>
    /// <param name="monoExtensionPatcherType">The patcher type to apply.</param>
    private static void Patch(Harmony harmony, Type monoExtensionPatcherType)
    {
        var instance = (IMonoExtensionPatcher)FormatterServices.GetUninitializedObject(monoExtensionPatcherType);
        var target = instance.Target;

        var originalMethod = target.Type.GetMethod(target.MethodName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (originalMethod == null)
            return;

        if (!_monoBehaviourToPatcherMap.TryGetValue(target.Type, out var patcherTypes))
        {
            patcherTypes = [];
            _monoBehaviourToPatcherMap[target.Type] = patcherTypes;
        }

        if (!patcherTypes.Contains(monoExtensionPatcherType))
        {
            patcherTypes.Add(monoExtensionPatcherType);
        }

        var postfixMethod = typeof(IMonoExtensionPatcher).GetMethod(nameof(Postfix),
            BindingFlags.Static | BindingFlags.NonPublic);

        harmony.Patch(originalMethod, postfix: new HarmonyMethod(postfixMethod));
    }

    /// <summary>
    /// Postfix method that applies extension patches to MonoBehaviour instances.
    /// </summary>
    /// <param name="__instance">The MonoBehaviour instance being patched.</param>
    private static void Postfix(MonoBehaviour __instance)
    {
        var type = __instance.GetType();

        // Try exact match first
        if (!_monoBehaviourToPatcherMap.TryGetValue(type, out var patcherTypes))
        {
            // Check for assignable types (handles inheritance)
            foreach (var kvp in _monoBehaviourToPatcherMap)
            {
                if (kvp.Key.IsAssignableFrom(type))
                {
                    patcherTypes = kvp.Value;
                    _monoBehaviourToPatcherMap[type] = patcherTypes;
                    break;
                }
            }

            if (patcherTypes == null)
                return;
        }

        foreach (var patcherType in patcherTypes)
        {
            var patcher = GetUninitializedMonoExtensionPatcher(patcherType, __instance);
            patcher.AddExtensionPatch(__instance);
        }
    }

    /// <summary>
    /// Cache for uninitialized patcher instances.
    /// </summary>
    private static readonly Dictionary<Type, IMonoExtensionPatcher> _uninitializedMonoExtensionPatcherLookup = [];

    /// <summary>
    /// Maps MonoBehaviour types to their corresponding patcher types.
    /// </summary>
    private static readonly Dictionary<Type, List<Type>> _monoBehaviourToPatcherMap = [];

    /// <summary>
    /// Gets or creates an uninitialized patcher instance for the specified type.
    /// </summary>
    /// <param name="monoExtensionPatcherType">The patcher type.</param>
    /// <param name="monoBehaviour">The MonoBehaviour instance.</param>
    /// <returns>An uninitialized patcher instance.</returns>
    private static IMonoExtensionPatcher GetUninitializedMonoExtensionPatcher(Type monoExtensionPatcherType, MonoBehaviour monoBehaviour)
    {
        if (!_uninitializedMonoExtensionPatcherLookup.TryGetValue(monoExtensionPatcherType, out var uninitialized))
        {
            uninitialized = (IMonoExtensionPatcher)FormatterServices.GetUninitializedObject(monoExtensionPatcherType);
            _uninitializedMonoExtensionPatcherLookup[monoExtensionPatcherType] = uninitialized;
        }

        return uninitialized;
    }
}

/// <summary>
/// Provides generic functionality to patch MonoBehaviour methods and apply extension patches.
/// </summary>
/// <typeparam name="T">The specific MonoBehaviour type to patch.</typeparam>
internal interface IMonoExtensionPatcher<T> : IMonoExtensionPatcher where T : MonoBehaviour
{
    /// <summary>
    /// Adds an extension patch to the specified MonoBehaviour.
    /// </summary>
    /// <param name="monoBehaviour">The MonoBehaviour to patch.</param>
    void IMonoExtensionPatcher.AddExtensionPatch(MonoBehaviour monoBehaviour)
    {
        AddExtensionPatch((T)monoBehaviour);
    }

    /// <summary>
    /// Adds an extension patch to the specified MonoBehaviour of type T.
    /// </summary>
    /// <param name="monoBehaviour">The MonoBehaviour instance to patch.</param>
    void AddExtensionPatch(T monoBehaviour);
}