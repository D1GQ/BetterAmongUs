using AmongUs.GameOptions;
using BetterAmongUs.Utilities;
using HarmonyLib;

namespace BetterAmongUs.Patches.Gameplay.Options;

[HarmonyPatch]
internal static class LogicOptionsPatch
{
    [HarmonyPatch(typeof(LogicOptionsNormal), nameof(LogicOptionsNormal.GetAnonymousVotes))]
    [HarmonyPostfix]
    private static void LogicOptionsNormal_Update_Postfix(ref bool __result)
    {
        if (PlayerControl.LocalPlayer == null)
            return;

        // Show anonymous votes when dead and not Guardian Angel
        if (!PlayerControl.LocalPlayer.IsAlive() && !PlayerControl.LocalPlayer.Is(RoleTypes.GuardianAngel))
        {
            __result = false;
        }
    }
}
