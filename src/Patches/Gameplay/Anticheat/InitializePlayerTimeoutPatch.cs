using BepInEx.Unity.IL2CPP.Utils.Collections;
using BetterAmongUs.Generated;
using BetterAmongUs.Data.Config;
using BetterAmongUs.Modules.Support;
using BetterAmongUs.Modules;
using BetterAmongUs.Utilities;
using HarmonyLib;
using System.Collections;

namespace BetterAmongUs.Patches.Gameplay.Anticheat;

[HarmonyPatch]
internal static class InitializePlayerTimeoutPatch
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ClientInitialize))]
    [HarmonyPostfix]
    private static void PlayerControl_ClientInitialize_Postfix(PlayerControl __instance, ref Il2CppSystem.Collections.IEnumerator __result)
    {
        if (!BAUConfigs.AntiCheat.Value || BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_Anticheat))
            return;

        __result = CoClientInitialize(__instance, __result).WrapToIl2Cpp();
    }

    private static IEnumerator CoClientInitialize(PlayerControl player, Il2CppSystem.Collections.IEnumerator original)
    {
        player.Visible = false;
        bool exit = false;
        yield return player.AssertWithTimeout((Func<bool>)(() => player == null || (GameData.Instance != null && player.Data != null && !player.Data.IsIncomplete)), (Action)(() =>
        {
            exit = GameState.IsHost && player != null && !player.AmOwner;
            if (player != null && GameState.IsHost && !player.AmOwner)
                player.Kick(false, TranslationStrings.AntiCheat_Reason_Initialize.LocalizedString,
                    bypassDataCheck: true);
        }), 25f);

        if (exit || player == null)
        {
            yield break;
        }

        yield return original;
    }
}
