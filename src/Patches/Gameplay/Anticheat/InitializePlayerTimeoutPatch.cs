using BepInEx.Unity.IL2CPP.Utils.Collections;
using BetterAmongUs.Generated;
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
        __result = CoClientInitialize(__instance, __result).WrapToIl2Cpp();
    }

    private static IEnumerator CoClientInitialize(PlayerControl player, Il2CppSystem.Collections.IEnumerator original)
    {
        player.Visible = false;
        bool exit = false;
        yield return player.AssertWithTimeout((Func<bool>)(() => GameData.Instance != null && player.Data != null && !player.Data.IsIncomplete), (Action)(() =>
        {
            if (GameState.IsHost)
            {
                player.Kick(true, TranslationStrings.AntiCheat_Reason_Initialize.LocalizedString, true, forceBan: true);
                exit = true;
            }
            else
            {
                if (GameData.Instance != null && player.Data != null)
                {
                    var outfit = player.Data.DefaultOutfit;
                    outfit.PlayerName = TranslationStrings.Player_Loading.LocalizedString;
                    outfit.HatId = HatData.EmptyId;
                    outfit.VisorId = VisorData.EmptyId;
                    outfit.SkinId = SkinData.EmptyId;
                    outfit.PetId = PetData.EmptyId;
                    outfit.NamePlateId = NamePlateData.EmptyId;
                    outfit.ColorId = 18;
                }
                else
                {
                    exit = true;
                }
            }
        }), 25f);

        if (exit)
        {
            yield break;
        }

        yield return original;
    }
}
