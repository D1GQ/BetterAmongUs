using BetterAmongUs.Data.Config;
using BetterAmongUs.Helpers;
using BetterAmongUs.Modules;
using BetterAmongUs.Modules.Support;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Patches.Client;

[HarmonyPatch]
internal static class PrivateLobbyPatch
{
    private static GameObject? toggle;
    private static readonly List<PassiveButton>? buttons = [];
    private static TextMeshPro? toggleText;

    [HarmonyPatch(typeof(CreateGameOptions), nameof(CreateGameOptions.Show))]
    [HarmonyPostfix]
    private static void CreateGameOptions_Show_Postfix(CreateGameOptions __instance)
    {
        // Check if other mods have disabled private lobby feature
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_PrivateLobby))
        {
            BAUConfigs.PrivateOnlyLobby.Value = false;
            return;
        }

        // Only create toggle UI once
        if (toggle != null)
            return;

        buttons.Clear();

        // Clone April Fools toggle as template for private lobby toggle
        toggle = UnityEngine.Object.Instantiate(__instance.AprilFoolsToggle, __instance.contentObjects.First().transform.parent);
        if (toggle != null)
        {
            toggle.name = "PrivateOnlyLobby";

            // Get the ON and OFF buttons from the toggle
            buttons.Add(toggle.transform.Find("AprilOn").GetComponent<PassiveButton>());
            buttons.Add(toggle.transform.Find("AprilOff").GetComponent<PassiveButton>());
            if (buttons.Count < 2)
                return;

            toggle.gameObject.SetActive(true);

            // Set up ON button click handler
            var onButton = buttons[0];
            if (onButton != null)
            {
                onButton.gameObject.SetActive(false);
                onButton.OnClick = new();
                onButton.OnClick.AddListener(() => TogglePrivateOnlyLobby(true));
            }

            // Set up OFF button click handler
            var offButton = buttons[1];
            if (offButton != null)
            {
                offButton.gameObject.SetActive(false);
                offButton.OnClick = new();
                offButton.OnClick.AddListener(() => TogglePrivateOnlyLobby(false));
            }

            // Position toggle at top-right of create game screen
            var aspect = toggle.gameObject.AddComponent<AspectPosition>();
            aspect.Alignment = AspectPosition.EdgeAlignments.Top;
            aspect.DistanceFromEdge = new Vector3(0.4f, 1.62f, 0);
            aspect.AdjustPosition();

            // Update toggle text label
            var modeText = toggle.transform.Find("BlackSquare/ModeText");
            if (modeText != null)
            {
                var text = modeText.GetComponent<TextMeshPro>();
                if (text != null)
                {
                    text.DestroyTextTranslators();
                    text.text = "Private Only Lobby";
                    toggleText = text;
                }
            }
        }

        // Initialize toggle state from saved configuration
        TogglePrivateOnlyLobby(BAUConfigs.PrivateOnlyLobby.Value);
    }

    private static void TogglePrivateOnlyLobby(bool modeOn)
    {
        if (buttons.Count < 2)
            return;

        // Show both buttons first
        buttons[0].gameObject.SetActive(true);
        buttons[1].gameObject.SetActive(true);

        // Deselect both buttons
        buttons[0].SelectButton(false);
        buttons[1].SelectButton(false);

        // Select appropriate button based on mode
        if (modeOn)
        {
            buttons[0].SelectButton(true);
        }
        else
        {
            buttons[1].SelectButton(true);
        }

        // Save setting to configuration
        BAUConfigs.PrivateOnlyLobby.Value = modeOn;
    }

    [HarmonyPatch(typeof(LobbyInfoPane), nameof(LobbyInfoPane.Update))]
    [HarmonyPostfix]
    private static void LobbyInfoPane_Update_Postfix(LobbyInfoPane __instance)
    {
        // Enforce private lobby setting if enabled by host
        if (BAUConfigs.PrivateOnlyLobby.Value && !GameState.IsLocalGame && GameState.IsHost)
        {
            // Ensure game stays private
            if (AmongUsClient.Instance.IsGamePublic)
            {
                AmongUsClient.Instance.ChangeGamePublic(false);
            }

            // Disable and visually indicate that public lobby button is locked
            var button = __instance.HostPrivateButton.GetComponent<PassiveButton>();
            if (button != null)
            {
                button.enabled = false;

                // Change inactive sprite to cyan color to indicate locked state
                var inactive = __instance.HostPrivateButton.transform.Find("Inactive");
                if (inactive != null)
                {
                    var sprite = inactive.GetComponent<SpriteRenderer>();
                    if (sprite != null)
                    {
                        sprite.color = new Color(0.35f, 1f, 1f, 1f); // Corrected colour assignment
                    }
                }
            }
        }
    }
}