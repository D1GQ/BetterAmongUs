using BetterAmongUs.Modules.Support;
using HarmonyLib;

namespace BetterAmongUs.Patches.Gameplay.Managers;

[HarmonyPatch]
internal static class LoadingBarManagerPatch
{
    [HarmonyPatch(typeof(LoadingBarManager), nameof(LoadingBarManager.SetLoadingPercent))]
    [HarmonyPrefix]
    private static bool LoadingBarManager_SetLoadingPercent_Prefix()
    {
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_CustomLoadingBar))
        {
            return true;
        }

        return false;
    }

    [HarmonyPatch(typeof(LoadingBarManager), nameof(LoadingBarManager.ToggleLoadingBar))]
    [HarmonyPrefix]
    private static bool LoadingBarManager_ToggleLoadingBart_Prefix()
    {
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_CustomLoadingBar))
        {
            return true;
        }

        return false;
    }
}
