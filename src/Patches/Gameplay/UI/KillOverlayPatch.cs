using BetterAmongUs.Utilities;
using HarmonyLib;

namespace BetterAmongUs.Patches.Gameplay.UI;

[HarmonyPatch(typeof(KillOverlay))]
internal static class KillOverlayPatch
{
    [HarmonyPatch(nameof(KillOverlay.ShowKillAnimation))]
    [HarmonyPatch([typeof(OverlayKillAnimation), typeof(NetworkedPlayerInfo), typeof(NetworkedPlayerInfo)])]
    [HarmonyPrefix]
    private static bool KillOverlay_ShowKillAnimation_Prefix()
    {
        // Don't show kill animation if local player is dead
        if (!PlayerControl.LocalPlayer.IsAlive())
        {
            return false;
        }

        return true;
    }
}