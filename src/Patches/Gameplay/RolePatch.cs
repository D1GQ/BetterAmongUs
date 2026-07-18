using BetterAmongUs.MonoScripts.Extended;
using HarmonyLib;

namespace BetterAmongUs.Patches.Gameplay;

[HarmonyPatch]
internal static class RolePatch
{
    [HarmonyPatch(typeof(NoisemakerRole), nameof(NoisemakerRole.OnDeath))]
    [HarmonyPrefix]
    private static bool NoisemakerRole_NotifyOfDeath_Prefix(NoisemakerRole __instance)
    {
        // Prevent duplicate noisemaker notifications
        if (__instance.Player.ExtendedData().RoleInfo.HasNoisemakerNotify)
        {
            return false;
        }

        // Mark that notification has been sent
        __instance.Player.ExtendedData().RoleInfo.HasNoisemakerNotify = true;

        return true;
    }
}