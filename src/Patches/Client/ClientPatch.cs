using BepInEx.Unity.IL2CPP.Utils;
using BetterAmongUs.Data.Config;
using BetterAmongUs.Helpers;
using BetterAmongUs.Managers;
using BetterAmongUs.Modules;
using BetterAmongUs.Patches.Gameplay.UI.Chat;
using HarmonyLib;
using InnerNet;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BetterAmongUs.Patches.Client;

[HarmonyPatch]
internal static class ClientPatch
{
    [HarmonyPatch(typeof(AccountTab), nameof(AccountTab.Awake))]
    [HarmonyPostfix]
    private static void AccountTab_Awake_Postfix(AccountTab __instance)
    {
        // Apply custom UI colors to the friends button
        __instance.signInStatusComponent.friendsButton.SetUIColors();
    }

    [HarmonyPatch(typeof(SignInStatusComponent), nameof(SignInStatusComponent.SetOnline))]
    [HarmonyPrefix]
    private static bool SignInStatusComponent_SetOnline_Prefix(SignInStatusComponent __instance)
    {
        // Get supported Among Us versions for BAU
        var varSupportedVersions = ModInfo.SupportedAmongUsVersions;
        Version currentVersion = new(BAUPlugin.AppVersion);
        Version firstSupportedVersion = new(varSupportedVersions.First());
        Version lastSupportedVersion = new(varSupportedVersions.Last());

        // Check if current Among Us version is higher than supported range
        if (currentVersion > firstSupportedVersion)
        {
            var verText = $"<b>{varSupportedVersions.First()}</b>";
            // Format version range if there are multiple supported versions
            if (firstSupportedVersion != lastSupportedVersion)
            {
                verText = $"<b>{varSupportedVersions.Last()}</b> - <b>{varSupportedVersions.First()}</b>";
            }

            // Show warning popup for newer Among Us version
            Utils.ShowPopUp($"<size=200%>-= <color=#ff2200><b>Warning</b></color> =-</size>\n\n" +
                $"<size=125%><color=#0dff00>Better Among Us {BAUPlugin.GetVersionText()}</color>\nsupports <color=#4f92ff>Among Us {verText}</color>,\n" +
                $"<color=#4f92ff>Among Us <b>{BAUPlugin.AppVersion}</b></color> is above the supported versions!\n" +
                $"<color=#ae1700>You may encounter minor to game breaking bugs.</color></size>");
        }
        // Check if current Among Us version is lower than supported range
        else if (currentVersion < lastSupportedVersion)
        {
            var verText = $"<b>{varSupportedVersions.First()}</b>";
            if (firstSupportedVersion != lastSupportedVersion)
            {
                verText = $"<b>{varSupportedVersions.Last()}</b> - <b>{varSupportedVersions.First()}</b>";
            }

            // Show warning popup for older Among Us version
            Utils.ShowPopUp($"<size=200%>-= <color=#ff2200><b>Warning</b></color> =-</size>\n\n" +
                $"<size=125%><color=#0dff00>Better Among Us {BAUPlugin.GetVersionText()}</color>\nsupports <color=#4f92ff>Among Us {verText}</color>,\n" +
                $"<color=#4f92ff>Among Us <b>{BAUPlugin.AppVersion}</b></color> is below the supported versions!\n" +
                $"<color=#ae1700>You may encounter minor to game breaking bugs.</color></size>");
        }

        return true;
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.ExitGame))]
    [HarmonyPostfix]
    private static void AmongUsClient_ExitGame_Postfix([HarmonyArgument(0)] DisconnectReasons reason)
    {
        // Hide custom loading bar when exiting game
        CustomLoadingBarManager.ToggleLoadingBar(false);
        Logger_.Log($"Client has left game for: {Enum.GetName(reason)}", "AmongUsClientPatch");
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    [HarmonyPrefix]
    private static void AmongUsClient_OnGameEnd_Prefix()
    {
        // Preserve all player GameObjects during scene transitions
        foreach (var data in GameData.Instance.AllPlayers)
        {
            UnityEngine.Object.DontDestroyOnLoad(data.gameObject);
        }

        // Move player GameObjects to active scene after a short delay
        LateTask.Schedule(() =>
        {
            foreach (var data in GameData.Instance.AllPlayers)
            {
                SceneManager.MoveGameObjectToScene(data.gameObject, SceneManager.GetActiveScene());
            }
        }, 0.6f, shouldLog: false);
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    [HarmonyPostfix]
    private static void AmongUsClient_CoStartGame_Postfix(AmongUsClient __instance)
    {
        // Clear in-game chat if chat feature is enabled
        if (BAUConfigs.ChatInGameplay.Value)
        {
            ChatPatch.ClearChat();
        }

        // Start custom loading sequence
        __instance.StartCoroutine(CoLoading());
    }

    private static IEnumerator CoLoading()
    {
        // Show custom loading bar
        CustomLoadingBarManager.ToggleLoadingBar(true);

        // Run different loading logic for host vs client
        if (GameState.IsHost)
        {
            yield return CoLoadingHost();
        }
        else
        {
            yield return CoLoadingClient();
        }

        // Mark loading as complete and hide bar after delay
        CustomLoadingBarManager.SetLoadingPercent(100f, "Complete");
        yield return new WaitForSeconds(0.25f);
        CustomLoadingBarManager.ToggleLoadingBar(false);
    }

    private static IEnumerator CoLoadingHost()
    {
        var client = AmongUsClient.Instance.GetClient(AmongUsClient.Instance.ClientId);
        var clients = AmongUsClient.Instance.allClients;

        // Continue loading while there are unassigned roles
        while (BAUPlugin.AllPlayerControls.Count > 0 && BAUPlugin.AllPlayerControls.Any(pc => !pc.roleAssigned))
        {
            // Early exit if game ended during loading
            if (!GameState.IsInGame)
            {
                CustomLoadingBarManager.ToggleLoadingBar(false);
                yield break;
            }

            string loadingText = "Initializing Game";
            float progress = 0f;

            // Progress through different loading stages
            if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
            {
                loadingText = "Starting Game Session";
                progress = 0.1f;
            }
            else if (LobbyBehaviour.Instance)
            {
                loadingText = "Loading";
                progress = 0.2f;
            }
            else if (!ShipStatus.Instance || AmongUsClient.Instance.ShipLoadingAsyncHandle.IsValid())
            {
                bool isShipLoading = AmongUsClient.Instance.ShipLoadingAsyncHandle.IsValid();

                loadingText = isShipLoading ? "Loading Ship Async" : "Spawning Ship";
                progress = isShipLoading ? 0.3f : 0.4f;
            }
            else if (BAUPlugin.AllPlayerControls.Any(player => !player.roleAssigned))
            {
                // Calculate role assignment progress
                int totalPlayers = BAUPlugin.AllPlayerControls.Count;
                int assignedPlayers = BAUPlugin.AllPlayerControls.Count(pc => pc.roleAssigned);
                float assignmentProgress = (float)assignedPlayers / Mathf.Max(1, totalPlayers);

                loadingText = $"Assigning Roles ({assignedPlayers}/{totalPlayers})";
                progress = 0.4f + 0.3f * assignmentProgress;
            }
            else if (!client.IsReady)
            {
                // Wait for other clients to be ready
                int readyClients = clients.CountIl2Cpp(c => c != null && c.Character != null && c.IsReady);
                int totalClients = clients.CountIl2Cpp(c => c != null && c.Character != null);

                loadingText = $"Waiting for Players ({readyClients}/{totalClients})";
                progress = 0.8f + 0.2f * readyClients / Mathf.Max(1, totalClients);
            }

            // Update loading bar with current progress
            int percent = Mathf.RoundToInt(progress * 100f);
            CustomLoadingBarManager.SetLoadingPercent(percent, loadingText);

            yield return null;
        }
    }

    private static IEnumerator CoLoadingClient()
    {
        var client = AmongUsClient.Instance.GetClient(AmongUsClient.Instance.ClientId);
        var clients = AmongUsClient.Instance.allClients;

        // Client loading logic (similar to host but with some differences)
        while (BAUPlugin.AllPlayerControls.Count > 0 && BAUPlugin.AllPlayerControls.Any(pc => !pc.roleAssigned))
        {
            // Switch to host logic if client becomes host mid-loading
            if (GameState.IsHost)
            {
                yield return CoLoadingHost();
                yield break;
            }

            if (!GameState.IsInGame)
            {
                CustomLoadingBarManager.ToggleLoadingBar(false);
                yield break;
            }

            string loadingText = "Initializing Game";
            float progress = 0;

            if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
            {
                loadingText = "Starting Game Session";
                progress = 0.1f;
            }
            else if (LobbyBehaviour.Instance)
            {
                loadingText = "Loading";
                progress = 0.25f;
            }
            else if (!ShipStatus.Instance || AmongUsClient.Instance.ShipLoadingAsyncHandle.IsValid())
            {
                bool isShipLoading = AmongUsClient.Instance.ShipLoadingAsyncHandle.IsValid();

                loadingText = isShipLoading ? "Loading Ship Async" : "Spawning Ship";
                progress = isShipLoading ? 0.35f : 0.4f;
            }
            else if (!client.IsReady)
            {
                loadingText = "Finalizing Connection";
                progress = 0.75f;
            }
            else
            {
                // Wait for other players (including host) to be ready
                int readyClients = clients.CountIl2Cpp(c => c != null && c.Character != null && c.IsReady);
                int totalClients = clients.CountIl2Cpp(c => c != null && c.Character != null);

                loadingText = $"Waiting for Players ({readyClients}/{totalClients})";
                progress = 0.85f + 0.15f * readyClients / Mathf.Max(1, totalClients);
            }

            int percent = Mathf.RoundToInt(progress * 100f);
            CustomLoadingBarManager.SetLoadingPercent(percent, loadingText);

            yield return null;
        }
    }
}