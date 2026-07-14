using BepInEx.Unity.IL2CPP.Utils;
using BetterAmongUs.Interfaces;
using BetterAmongUs.MonoScripts.Extended;
using HarmonyLib;
using System.Collections;

namespace BetterAmongUs.Patches.Client;

[HarmonyPatch]
internal static class MonoExtensionPatch
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Awake))]
    [HarmonyPostfix]
    private static void PlayerControl_Awake_Postfix(PlayerControl __instance)
    {
        IMonoExtension.AddExtension<ExtendedPlayerControl>(__instance);
        __instance.StartCoroutine(CoAddExtensionPatch(__instance));
    }

    private static IEnumerator CoAddExtensionPatch(PlayerControl playerControl)
    {
        while (playerControl.Data == null)
        {
            yield return null;
        }

        IMonoExtension.AddExtension<ExtendedPlayerInfo>(playerControl.Data);
    }
}
