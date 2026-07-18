using BetterAmongUs.Modules.Support;
using HarmonyLib;
using UnityEngine;

namespace BetterAmongUs.Patches.Gameplay.UI.Chat;

[HarmonyPatch]
internal static class ChatNotificationPatch
{
    [HarmonyPatch(typeof(ChatNotification), nameof(ChatNotification.SetUp))]
    [HarmonyPostfix]
    private static void ChatNotification_SetUp_Postfix(ChatNotification __instance)
    {
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_BetterPingTracker))
            return;

        // Reposition to top left 
        __instance.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(-2.8f, 0.3f, -40f);
        __instance.transform.localScale = new Vector3(0.45f, 0.42f, 1f);
    }
}
