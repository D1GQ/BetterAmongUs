using BetterAmongUs.Data.Config;
using BetterAmongUs.Interfaces;
using BetterAmongUs.Modules;
using BetterAmongUs.Modules.OptionItems;
using BetterAmongUs.Modules.Support;
using BetterAmongUs.MonoScripts.Extended;
using BetterAmongUs.Utilities;
using HarmonyLib;
using UnityEngine;

namespace BetterAmongUs.Patches.Gameplay;

[HarmonyPatch]
internal static class LobbyPatch
{
    [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
    [HarmonyPostfix]
    private static void LobbyBehaviour_Start_Postfix()
    {
        // Reset player selection options when lobby starts
        OptionPlayerItem.ResetAllValues();
    }

    private static Transform? GameStartObj;

    // Disable annoying lobby music if setting is enabled
    [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Update))]
    [HarmonyPostfix]
    private static void LobbyBehaviour_Update_Postfix()
    {
        if (BAUConfigs.DisableLobbyTheme.Value)
            SoundManager.instance.StopSound(LobbyBehaviour.Instance.MapTheme);

        // Adjust GameStartManager position for better UI layout
        if (GameStartObj == null)
        {
            if (HudManager.InstanceExists)
            {
                GameStartObj = HudManager.Instance.transform.Find("GameStartManager");
            }
        }
        else
        {
            GameStartObj.SetLocalY(-2.8f);
        }
    }

    [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.RpcExtendLobbyTimer))]
    [HarmonyPostfix]
    private static void LobbyBehaviour_RpcExtendLobbyTimer_Postfix()
    {
        lobbyTimer += 30f; // Add 30 seconds to lobby timer
    }

    // Apply UI colors to settings pane buttons
    [HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.Awake))]
    [HarmonyPostfix]
    private static void LobbyViewSettingsPane_Awake_Postfix(LobbyViewSettingsPane __instance)
    {
        __instance.backButton.gameObject.SetUIColors("Icon");
        __instance.taskTabButton.gameObject.SetUIColors("Icon");
        __instance.rolesTabButton.gameObject.SetUIColors("Icon");
    }

    internal static float lobbyTimer = 600f; // 10 minute default
    internal static string lobbyTimerDisplay = "";

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    [HarmonyPostfix]
    private static void GameStartManager_Start_Postfix(GameStartManager __instance)
    {
        lobbyTimer = 600f; // Reset timer

        var extendedPassiveButton = IMonoExtension.AddExtension<ExtendedPassiveButton>(__instance.StartButton);
        extendedPassiveButton.OnHoldOrShiftClick += () =>
        {
            if (__instance.startState == GameStartManager.StartingStates.NotStarting)
            {
                __instance.startState = GameStartManager.StartingStates.Countdown;
                __instance.FinallyBegin();
            }
        };

        // Apply UI colors to buttons
        __instance.StartButton.gameObject.SetUIColors("Icon");
        __instance.EditButton.gameObject.SetUIColors("Icon");
        __instance.ClientViewButton.gameObject.SetUIColors("Icon");
        __instance.HostViewButton.gameObject.SetUIColors("Icon");

        // Move start buttons to host panel if ping tracker not disabled
        if (!BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_BetterPingTracker))
        {
            __instance.StartButton.transform.SetParent(__instance.HostInfoPanel?.transform);
            __instance.StartButtonClient.transform.SetParent(__instance.HostInfoPanel?.transform);
        }
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    [HarmonyPrefix]
    private static void GameStartManager_Update_Prefix(GameStartManager __instance)
    {
        // Update lobby timer countdown
        lobbyTimer = Mathf.Max(0f, lobbyTimer -= Time.deltaTime);
        int minutes = (int)lobbyTimer / 60;
        int seconds = (int)lobbyTimer % 60;
        lobbyTimerDisplay = $"{minutes:00}:{seconds:00}";

        __instance.MinPlayers = 1; // Allow starting with just 1 player
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    [HarmonyPostfix]
    private static void GameStartManager_Update_Postfix(GameStartManager __instance)
    {
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_CancelStartingGame))
            return;

        // Hide start button for non-hosts
        if (!GameState.IsHost)
        {
            __instance.StartButton.gameObject.SetActive(false);
            return;
        }

        // Custom start button behavior for hosts
        __instance.GameStartTextParent.SetActive(false);
        __instance.StartButton.gameObject.SetActive(true);

        if (__instance.startState == GameStartManager.StartingStates.Countdown)
        {
            // Show cancel button with countdown
            __instance.StartButton.buttonText.text = string.Format("{0}: {1}", Translator.GetString(StringNames.Cancel), (int)__instance.countDownTimer + 1);
        }
        else
        {
            // Show normal start button
            __instance.StartButton.buttonText.text = Translator.GetString(StringNames.StartLabel);
        }
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
    [HarmonyPrefix]
    private static bool GameStartManager_BeginGame_Prefix(GameStartManager __instance)
    {
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_CancelStartingGame)) return true;

        // If countdown is active, clicking cancels the start
        if (__instance.startState == GameStartManager.StartingStates.Countdown)
        {
            SoundManager.instance.StopSound(__instance.gameStartSound);
            __instance.ResetStartState();
            return false;
        }

        return true;
    }

    // Log game start info
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.FinallyBegin))]
    [HarmonyPrefix]
    private static void GameStartManager_FinallyBegin_Prefix(/*GameStartManager __instance*/)
    {
        Logger_.LogHeader($"Game Has Started - {Enum.GetName(typeof(MapNames), GameState.GetActiveMapId)}/{GameState.GetActiveMapId}", "GamePlayManager");
    }
}