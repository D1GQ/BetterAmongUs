using BepInEx.Unity.IL2CPP.Utils;
using BetterAmongUs.Generated;
using BetterAmongUs.Managers;
using BetterAmongUs.Modules;
using BetterAmongUs.Patches.Gameplay.UI.Chat;
using BetterAmongUs.Utilities;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace BetterAmongUs.Patches.Gameplay.Managers;

[HarmonyPatch]
internal static class HudManagerPatch
{
    internal static string WelcomeMessage => $"<b><color=#00b530><size=125%><align=\"center\">{TranslationStrings.WelcomeMsg_WelcomeToBAU.Format(TranslationStrings.BetterAmongUs)}\n{BAUPlugin.ModInfo.VERSION_STRING}</size>\n" +
        $"{TranslationStrings.WelcomeMsg_ThanksForDownloading}</align></color></b>\n<size=120%> </size>\n" +
        TranslationStrings.WelcomeMsg_BAUDescription1.Format(TranslationStrings.BAU, TranslationStrings.BetterOption_AntiCheat);

    private static bool HasBeenWelcomed = false;

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    [HarmonyPostfix]
    private static void HudManager_Start_Postfix(HudManager __instance)
    {
        __instance.StartCoroutine(CoHudManagerStart());
    }

    private static IEnumerator CoHudManagerStart()
    {
        BetterNotificationManager.Init();

        yield return new WaitForSeconds(1f);

        if (!HasBeenWelcomed && GameState.IsInGame && GameState.IsLobby && !GameState.IsFreePlay)
        {
            // Show notification with welcome text
            BetterNotificationManager.Notify($"<b><color=#00751f>{TranslationStrings.WelcomeMsg_WelcomeToBAU.Format(TranslationStrings.BetterAmongUs)}!</color></b>", 8f);

            // Send detailed welcome message to private chat
            Utils.AddChatPrivate(WelcomeMessage, overrideName: " ");
            HasBeenWelcomed = true;
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPostfix]
    private static void HudManager_Update_Postfix(HudManager __instance)
    {
        // Manage in-game chat visibility based on settings and game state
        if (GameState.InGame)
        {
            if (__instance.Chat == null)
                return;

            __instance.Chat.gameObject.SetActive(ChatPatch.IsChatVisible);
        }
    }

    [HarmonyPatch(typeof(MatchInfoHudButton), nameof(MatchInfoHudButton.Update))]
    [HarmonyPrefix]
    private static bool MatchInfoHudButton_Update_Prefix(MatchInfoHudButton __instance)
    {
        if (ChatPatch.IsChatVisible)
        {
            __instance.aspectPosition.DistanceFromEdge = MatchInfoHudButton.adjustedDistanceFromEdge;
        }
        else
        {
            __instance.aspectPosition.DistanceFromEdge = MatchInfoHudButton.defaultDistanceFromEdge;
        }

        return false;
    }
}