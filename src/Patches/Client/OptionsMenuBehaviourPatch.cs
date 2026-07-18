using BetterAmongUs.Data;
using BetterAmongUs.Data.Config;
using BetterAmongUs.Data.Json;
using BetterAmongUs.Generated;
using BetterAmongUs.Managers;
using BetterAmongUs.Modules;
using BetterAmongUs.MonoScripts.Extended;
using BetterAmongUs.Patches.Gameplay.UI;
using BetterAmongUs.Patches.Gameplay.UI.Chat;
using BetterAmongUs.Utilities;
using BetterAmongUs.Utilities.Extension;
using HarmonyLib;
using System.Diagnostics;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Patches.Client;

[HarmonyPatch]
internal static class OptionsMenuBehaviourPatch
{
    internal static TabGroup? BetterOptionsTab { get; private set; }

    [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
    [HarmonyPostfix]
    private static void Start_Postfix(OptionsMenuBehaviour __instance)
    {
        // Create custom "Better Options" tab in settings menu
        BetterOptionsTab = CreateTabPage(__instance, TranslationStrings.BetterOption.LocalizedString);

        // Populate the tab with all BAU client options
        SetupAllClientOptions(__instance);

        // Reposition tabs to fit new Better Options tab
        UpdateTabPositions(__instance);
    }

    private static void SetupAllClientOptions(OptionsMenuBehaviour __instance)
    {
        if (__instance.DisableMouseMovement == null)
            return;

        // Clear previous client options to prevent duplicates
        ClientOptionItem.ClientOptions.Clear();

        // Toggle options with config binding
        ClientOptionItem.CreateToggle(TranslationStrings.BetterOption_AntiCheat, BAUConfigs.AntiCheat, 1, __instance);
        ClientOptionItem.CreateToggle(TranslationStrings.BetterOption_SendBetterRpc, BAUConfigs.SendBetterRpc, 1, __instance, SendBetterRpcAction);
        ClientOptionItem.CreateToggle(TranslationStrings.BetterOption_BetterNotifications, BAUConfigs.BetterNotifications, 1, __instance, BetterNotificationManager.ClearNotifications);
        ClientOptionItem.CreateToggle(TranslationStrings.BetterOption_ForceOwnLanguage, BAUConfigs.ForceOwnLanguage, 1, __instance);
        ClientOptionItem.CreateToggle(TranslationStrings.BetterOption_ChatDarkMode, BAUConfigs.ChatDarkMode, 1, __instance, ChatPatch.SetChatTheme);
        ClientOptionItem.CreateToggle(TranslationStrings.BetterOption_ChatInGame, BAUConfigs.ChatInGameplay, 1, __instance);
        ClientOptionItem.CreateToggle(TranslationStrings.BetterOption_LobbyInfo, BAUConfigs.LobbyPlayerInfo, 1, __instance);
        ClientOptionItem.CreateToggle(TranslationStrings.BetterOption_LobbyTheme, BAUConfigs.DisableLobbyTheme, 1, __instance, ToggleLobbyTheme);
        ClientOptionItem.CreateToggle(TranslationStrings.BetterOption_UnlockFPS, BAUConfigs.UnlockFPS, 1, __instance, UpdateFrameRate);
        ClientOptionItem.CreateToggle(TranslationStrings.BetterOption_ShowFPS, BAUConfigs.ShowFPS, 1, __instance);

        ClientOptionItem.CreateToggle(TranslationStrings.BetterOption_VentColorGroups, BAUConfigs.VentColorGroups, 2, __instance, MiniMapBehaviourPatch.ClearMapIcons);
        ClientOptionItem.CreateToggle(TranslationStrings.BetterOption_MinimapIcons, BAUConfigs.MinimapIcons, 2, __instance, MiniMapBehaviourPatch.ClearMapIcons);
        ClientOptionItem.CreateToggle(TranslationStrings.BetterOption_CompressSettingFiles, BAUConfigs.CompressSettingFiles, 2, __instance, ConvertAllSettingFiles);

        // Button options (no toggle)
        if (!ModInfo.Starlight)
        {
            ClientOptionItem.CreateButton(TranslationStrings.BetterOption_SaveData, -1, __instance, OpenSaveData, () =>
            {
                // Only allow opening save data in lobby/main menu, not during gameplay
                bool cannotOpen = GameState.IsInGame && !GameState.IsLobby;
                if (cannotOpen)
                {
                    BetterNotificationManager.Notify($"Cannot open save data while in gameplay!", 2.5f);
                }
                return !cannotOpen;
            });
        }

        ClientOptionItem.CreateButton(TranslationStrings.BetterOption_ToVanilla, -1, __instance, BAUPlugin.Instance.UnloadBAU, () =>
        {
            // Prevent switching to vanilla while in a game
            bool cannotSwitch = GameState.IsInGame;
            if (cannotSwitch)
            {
                BetterNotificationManager.Notify($"Unable to switch to vanilla while in game!", 2.5f);
            }
            return !cannotSwitch;
        });
    }

    private static void SendBetterRpcAction()
    {
        // Resend handshake secret to all other players when option is toggled
        if (!GameState.IsInGame)
            return;

        foreach (var player in BAUPlugin.AllPlayerControls)
        {
            if (player.IsLocalPlayer()) continue;
            player.ExtendedData().HandshakeHandler.ResendSecretToPlayer();
        }
    }

    private static void ToggleLobbyTheme()
    {
        // Play lobby theme music if re-enabled while in lobby
        if (GameState.IsLobby && !BAUConfigs.DisableLobbyTheme.Value)
        {
            SoundManager.instance.CrossFadeSound("MapTheme", LobbyBehaviour.Instance.MapTheme, 0.5f, 1.5f);
        }
    }

    internal static void UpdateFrameRate()
    {
        // Toggle between 60 FPS (default) and 165 FPS
        Application.targetFrameRate = BAUConfigs.UnlockFPS.Value ? 999 : 60;
    }

    private static void OpenSaveData()
    {
        // Open BAU save data folder in file explorer
        if (!File.Exists(BetterDataManager.Files.dataFilePath))
            return;

        Process.Start(new ProcessStartInfo
        {
            FileName = BetterDataManager.Files.dataFilePath,
            UseShellExecute = true,
            Verb = "open"
        });
    }

    private static void ConvertAllSettingFiles()
    {
        for (int i = 0; i < 10; i++)
        {
            if (i == BAUConfigs.SettingsPreset.Value)
            {
                BetterDataManager.Files.BetterGameSettingsFile.Save();
                continue;
            }

            string path = BetterDataManager.GetSettingsFilePathFromPreset(i);
            if (File.Exists(path))
            {
                var settings = new BetterGameSettingsFile
                {
                    OverrideFilePath = path
                };
                settings.Init();
            }
        }
    }

    private static TabGroup CreateTabPage(OptionsMenuBehaviour __instance, string name)
    {
        // Clone last tab as template for new Better Options tab
        var tabPrefab = __instance.Tabs[^1];
        var tab = UnityEngine.Object.Instantiate(tabPrefab, tabPrefab.transform.parent);

        tab.name = $"{name}Button";
        tab.DestroyTextTranslators();
        tab.GetComponentInChildren<TextMeshPro>(true)?.SetText(name);
        tab.gameObject.SetActive(true);

        // Create content container for the new tab
        var content = new GameObject($"{name}Tab");
        content.SetActive(false);
        content.transform.SetParent(tab.Content.transform.parent);
        content.transform.localScale = Vector3.one;
        tab.Content = content;

        // Add new tab to the tabs array
        var tabs = new List<TabGroup>(__instance.Tabs) { tab };
        __instance.Tabs = tabs.ToArray();

        // Set up click handler for the tab button
        var index = __instance.Tabs.Length - 1;
        var button = tab.GetComponent<PassiveButton>();
        button.OnClick = new();
        button.OnClick.AddListener(() =>
        {
            tab.Rollover.SetEnabledColors();
            __instance.OpenTabGroup(index);
        });

        return tab;
    }

    private static void UpdateTabPositions(OptionsMenuBehaviour __instance)
    {
        // Position tabs based on game state (in-game vs main menu)
        Vector3 basePos = new(0f, !GameState.InGame ? 0 : 2.5f, -1f);
        const float buttonSpacing = 0.6f;
        const float buttonWidth = 1.0f;

        // Count only active tabs
        int activeCount = 0;
        foreach (var tabButton in __instance.Tabs)
        {
            if (tabButton.gameObject.activeInHierarchy) activeCount++;
        }

        if (activeCount == 0)
            return;

        // Calculate total width needed for all active tabs
        float totalWidth = (activeCount - 1) * buttonSpacing + activeCount * buttonWidth;
        float startX = basePos.x - (totalWidth / 2f) + (buttonWidth / 2f);

        // Position each active tab evenly spaced
        int activeIndex = 0;
        foreach (var tabButton in __instance.Tabs)
        {
            if (!tabButton.gameObject.activeInHierarchy) continue;

            float xPos = startX + activeIndex * (buttonWidth + buttonSpacing);
            tabButton.transform.localPosition = new Vector3(xPos, basePos.y, basePos.z);
            activeIndex++;
        }
    }
}