using BetterAmongUs.Interfaces;
using BetterAmongUs.Modules;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace BetterAmongUs.Patches.Hooks;

internal static class MonoExtensionHook
{
    private static bool _patched = false;
    private static readonly HashSet<Type> PatchedTypes = [];
    private static readonly List<IDetour> _detours = [];

    internal static void Install()
    {
        if (_patched) return;
        _patched = true;

        try
        {
            var extensionTypes = FindAllExtensionTypes();

            foreach (var extType in extensionTypes)
            {
                var targetType = GetTargetMonoBehaviourType(extType);
                if (targetType == null) continue;

                if (!PatchedTypes.Add(targetType)) continue;

                // Hook the IntPtr constructor
                var constructor = AccessTools.Constructor(targetType, [typeof(IntPtr)]);
                if (constructor != null)
                {
                    var hook = new MonoMod.RuntimeDetour.Hook(
                        constructor,
                        new Action<Action<MonoBehaviour, IntPtr>, MonoBehaviour, IntPtr>(MonoBehaviour_Constructor_Hook)
                    );
                    _detours.Add(hook);
                }

                // Hook OnDestroy
                var destroyMethod = AccessTools.Method(extType, nameof(IMonoExtension.OnDestroy));
                if (destroyMethod != null)
                {
                    var hook = new MonoMod.RuntimeDetour.Hook(
                        destroyMethod,
                        new Action<Action<IMonoExtension>, IMonoExtension>(IMonoExtension_OnDestroy_Hook)
                    );
                    _detours.Add(hook);
                }
            }
        }
        catch (Exception ex)
        {
            _patched = false;
            Logger_.Error($"Failed to hook MonoExtension methods: {ex.Message}");
            throw;
        }
    }

    internal static void Uninstall()
    {
        if (!_patched) return;

        try
        {
            foreach (var detour in _detours)
            {
                detour.Dispose();
            }
            _detours.Clear();
            _patched = false;
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

    private static void MonoBehaviour_Constructor_Hook(Action<MonoBehaviour, IntPtr> orig, MonoBehaviour instance, IntPtr ptr)
    {
        orig(instance, ptr);

        try
        {
            IMonoExtension.TryAddAutoExtension(instance);
        }
        catch (Exception ex)
        {
            Logger_.Error($"Error in constructor hook: {ex.Message}");
        }
    }

    private static void IMonoExtension_OnDestroy_Hook(Action<IMonoExtension> orig, IMonoExtension instance)
    {
        orig(instance);

        try
        {
            IMonoExtension.TryRemoveExtension(instance);
        }
        catch (Exception ex)
        {
            Logger_.Error($"Error in OnDestroy hook: {ex.Message}");
        }
    }
}