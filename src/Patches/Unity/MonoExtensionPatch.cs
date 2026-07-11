using BetterAmongUs.Interfaces;
using BetterAmongUs.Modules;
using HarmonyLib;
using UnityEngine;

namespace BetterAmongUs.Patches.Unity;

[HarmonyPatch]
internal static class MonoExtensionPatch
{
    private static bool _patched = false;
    private static readonly HashSet<Type> PatchedTypes = [];

    internal static void Patch(Harmony harmony)
    {
        if (_patched) return;
        _patched = true;

        try
        {
            // Find all types that implement IMonoExtension
            var extensionTypes = FindAllExtensionTypes();

            foreach (var extType in extensionTypes)
            {
                // Get the target MonoBehaviour type from IAutoMonoExtension<T>
                var targetType = GetTargetMonoBehaviourType(extType);
                if (targetType == null) continue;

                // Only patch each target type once
                if (!PatchedTypes.Add(targetType)) continue;

                // Patch the constructor of the target type
                var constructor = AccessTools.Constructor(targetType, [typeof(IntPtr)]);
                if (constructor != null)
                {
                    harmony.Patch(constructor,
                        postfix: new HarmonyMethod(typeof(MonoExtensionPatch),
                            nameof(MonoBehaviour_Constructor_Postfix)));
                }

                // Patch OnDestroy method if it exists on the extension type
                var destroyMethod = AccessTools.Method(extType, nameof(IMonoExtension.OnDestroy));
                if (destroyMethod != null)
                {
                    harmony.Patch(destroyMethod,
                        postfix: new HarmonyMethod(typeof(MonoExtensionPatch),
                            nameof(IMonoExtension_OnDestroy_Postfix)));
                }
            }
        }
        catch (Exception ex)
        {
            _patched = false;
            Logger_.Error($"Failed to patch MonoExtension methods: {ex.Message}");
            throw;
        }
    }

    internal static void Unpatch(Harmony harmony)
    {
        if (!_patched) return;

        try
        {
            var extensionTypes = FindAllExtensionTypes();

            foreach (var extType in extensionTypes)
            {
                var targetType = GetTargetMonoBehaviourType(extType);
                if (targetType == null) continue;

                // Unpatch constructor
                var constructor = AccessTools.Constructor(targetType, [typeof(IntPtr)]);
                if (constructor != null)
                {
                    harmony.Unpatch(constructor, HarmonyPatchType.Postfix, harmony.Id);
                }

                // Unpatch OnDestroy
                var destroyMethod = AccessTools.Method(extType, nameof(IMonoExtension.OnDestroy));
                if (destroyMethod != null)
                {
                    harmony.Unpatch(destroyMethod, HarmonyPatchType.Postfix, harmony.Id);
                }
            }
        }
        catch (Exception ex)
        {
            Logger_.Error($"Failed to unpatch MonoExtension methods: {ex.Message}");
            throw;
        }
    }

    private static Type? GetTargetMonoBehaviourType(Type extensionType)
    {
        var genericInterface = extensionType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType &&
                                i.GetGenericTypeDefinition() == typeof(IAutoMonoExtension<>));

        return genericInterface?.GetGenericArguments()[0];
    }

    private static List<Type> FindAllExtensionTypes()
    {
        var types = new List<Type>();
        var assembly = ModInfo.Assembly;

        foreach (var type in assembly.GetTypes())
        {
            if (type.IsInterface || type.IsAbstract)
                continue;

            if (!typeof(IMonoExtension).IsAssignableFrom(type))
                continue;

            if (type.IsGenericTypeDefinition)
                continue;

            types.Add(type);
        }

        return types;
    }

    private static void MonoBehaviour_Constructor_Postfix(MonoBehaviour __instance)
    {
        try
        {
            if (__instance == null) return;
            IMonoExtension.TryAddAutoExtension(__instance);
        }
        catch (Exception ex)
        {
            Logger_.Error($"Error in MonoExtension constructor patch: {ex.Message}");
        }
    }

    private static void IMonoExtension_OnDestroy_Postfix(IMonoExtension __instance)
    {
        IMonoExtension.TryRemoveExtension(__instance);
    }
}