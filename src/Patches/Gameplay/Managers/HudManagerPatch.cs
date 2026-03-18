using BetterAmongUs.Data.Config;
using BetterAmongUs.Helpers;
using BetterAmongUs.Managers;
using BetterAmongUs.Modules;
using HarmonyLib;
using UnityEngine;

namespace BetterAmongUs.Patches.Gameplay.Managers;

[HarmonyPatch]
internal static class HudManagerPatch
{
    internal static string WelcomeMessage = $"<b><color=#00b530><size=125%><align=\"center\">{string.Format(Translator.GetString("WelcomeMsg.WelcomeToBAU"), Translator.GetString("BetterAmongUs"))}\n{BAUPlugin.GetVersionText()}</size>\n" +
        $"{Translator.GetString("WelcomeMsg.ThanksForDownloading")}</align></color></b>\n<size=120%> </size>\n" +
        string.Format(Translator.GetString("WelcomeMsg.BAUDescription1"), Translator.GetString("bau"), Translator.GetString("BetterOption.AntiCheat"));

    private static bool HasBeenWelcomed = false;

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    [HarmonyPostfix]
    private static void HudManager_Start_Postfix(HudManager __instance)
    {
        // Create custom BAU notification system if it doesn't exist
        BetterNotificationManager.Init();

        // Show welcome message after 1 second delay (only once per session)
        LateTask.Schedule(() =>
        {
            if (!HasBeenWelcomed && GameState.IsInGame && GameState.IsLobby && !GameState.IsFreePlay)
            {
                // Show notification with welcome text
                BetterNotificationManager.Notify($"<b><color=#00751f>{string.Format(Translator.GetString("WelcomeMsg.WelcomeToBAU"), Translator.GetString("BetterAmongUs"))}!</color></b>", 8f);

                // Send detailed welcome message to private chat
                Utils.AddChatPrivate(WelcomeMessage, overrideName: " ");
                HasBeenWelcomed = true;
            }
        }, 1f, shouldLog: false);
    }

    private static GameObject? gameStart;

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPostfix]
    private static void HudManager_Update_Postfix(HudManager __instance)
    {
        // Adjust GameStartManager position for better UI layout
        if (gameStart == null)
        {
            gameStart = GameObject.Find("GameStartManager");
        }
        if (gameStart != null)
        {
            gameStart.transform.SetLocalY(-2.8f);
        }

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