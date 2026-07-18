using HarmonyLib;

namespace BetterAmongUs.Patches.Fixes;

// This fixes crashing issue when pressing UI buttons, W Innerslop!!!
// Fix by https://github.com/TouseefX
[HarmonyPatch]
internal static class PassiveButtonFixPatch
{
    [HarmonyPatch(typeof(PassiveButton), nameof(PassiveButton.ReceiveClickDown))]
    [HarmonyPrefix]
    public static bool PassiveButton_ReceiveClickDown_Prefix(PassiveButton __instance)
    {
        if (__instance == null || __instance.Pointer == IntPtr.Zero)
        {
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(PassiveButton), nameof(PassiveButton.ReceiveClickUp))]
    [HarmonyPrefix]
    public static bool PassiveButton_ReceiveClickUp_Prefix(PassiveButton __instance)
    {
        if (__instance == null || __instance.Pointer == IntPtr.Zero)
        {
            return false;
        }

        return true;
    }
}
