using BepInEx.Unity.IL2CPP.Utils;
using BetterAmongUs.Data.Config;
using BetterAmongUs.Generated;
using BetterAmongUs.Managers;
using BetterAmongUs.Modules;
using BetterAmongUs.Utilities;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace BetterAmongUs.Patches.Gameplay.Managers;

[HarmonyPatch]
internal static class HudManagerPatch
{
    internal static string WelcomeMessage => $"<b><color=#00b530><size=125%><align=\"center\">{TranslationStrings.WelcomeMsg_WelcomeToBAU.Format(TranslationStrings.BetterAmongUs)}\n{BAUPlugin.GetVersionText()}</size>\n" +
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

            if (!BAUConfigs.ChatInGameplay.Value)
            {
                // Vanilla chat behavior: only show chat when dead or during meetings
                if (!PlayerControl.LocalPlayer.IsAlive())
                {
                    __instance.Chat.gameObject.SetActive(true);
                }
                else if (GameState.IsInGamePlay && !(GameState.IsMeeting || GameState.IsExilling))
                {
                    __instance.Chat.gameObject.SetActive(false);
                }
            }
            else
            {
                // BAU chat behavior: always show chat when enabled
                if (__instance.Chat.gameObject.active == false)
                {
                    __instance.Chat.gameObject.SetActive(true);
                }
            }
        }
    }
}