using BetterAmongUs.Modules;
using BetterAmongUs.Patches.Gameplay.UI.Settings;
using BetterAmongUs.Utilities;
using HarmonyLib;

namespace BetterAmongUs.Patches.Gameplay.Anticheat;

[HarmonyPatch]
internal static class CheckPlayerLevelPatch
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    [HarmonyPostfix]
    private static void PlayerControl_FixedUpdate_Postfix(PlayerControl __instance)
    {
        if (__instance.Data == null)
            return;

        if (GameState.IsHost && GameState.IsLobby)
        {
            // Kick players below minimum level
            if (!__instance.IsLocalPlayer() && BetterGameSettings.KickLevel.GetBool() && __instance.Data.PlayerLevel < BetterGameSettings.KickLevelBelow.GetInt())
            {
                var minPlayers = BetterGameSettings.KickLevelBelowMinimumPlayers.GetInt();
                if (minPlayers > 1 && minPlayers > BAUPlugin.AllPlayerControls.Count)
                    return;
                
                __instance.TryKick(setReasonInfo: $" is level {__instance.Data.PlayerLevel}, level must be equal or above {BetterGameSettings.KickLevelBelow.GetInt()} to join");
            }
        }
    }
}