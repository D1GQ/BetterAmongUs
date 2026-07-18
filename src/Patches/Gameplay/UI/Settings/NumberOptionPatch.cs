using BetterAmongUs.Modules.Support;
using HarmonyLib;
using UnityEngine;

namespace BetterAmongUs.Patches.Gameplay.UI.Settings;

[HarmonyPatch]
internal static class NumberOptionPatch
{
    [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Increase))]
    [HarmonyPrefix]
    private static bool NumberOption_Increase_Prefix(NumberOption __instance)
    {
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_AllGameOptions)) return true;

        // Determine multiplier based on modifier keys
        int times = 1;
        if (Input.GetKey(KeyCode.LeftShift))
            times = 5;      // Shift = 5x
        if (Input.GetKey(KeyCode.LeftControl))
            times = 10;     // Control = 10x

        // Increase value with bounds checking
        if (__instance.Value + __instance.Increment * times > __instance.ValidRange.max)
        {
            __instance.Value = __instance.ValidRange.max; // Cap at max
        }
        else
        {
            __instance.Value = __instance.ValidRange.Clamp(__instance.Value + __instance.Increment * times);
        }

        // Update UI and invoke events
        __instance.UpdateValue();
        __instance.OnValueChanged.Invoke(__instance);
        __instance.AdjustButtonsActiveState();

        return false;
    }

    [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Decrease))]
    [HarmonyPrefix]
    private static bool NumberOption_Decrease_Prefix(NumberOption __instance)
    {
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_AllGameOptions)) return true;

        // Determine multiplier based on modifier keys
        int times = 1;
        if (Input.GetKey(KeyCode.LeftShift))
            times = 5;      // Shift = 5x
        if (Input.GetKey(KeyCode.LeftControl))
            times = 10;     // Control = 10x

        // Decrease value with bounds checking
        if (__instance.Value - __instance.Increment * times < __instance.ValidRange.min)
        {
            __instance.Value = __instance.ValidRange.min; // Cap at min
        }
        else
        {
            __instance.Value = __instance.ValidRange.Clamp(__instance.Value - __instance.Increment * times);
        }

        // Update UI and invoke events
        __instance.UpdateValue();
        __instance.OnValueChanged.Invoke(__instance);
        __instance.AdjustButtonsActiveState();

        return false;
    }
}