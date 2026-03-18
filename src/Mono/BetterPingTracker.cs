using BetterAmongUs.Data.Config;
using BetterAmongUs.Helpers;
using BetterAmongUs.Modules;
using System.Text;
using TMPro;
using UnityEngine;
using static BetterAmongUs.Patches.Gameplay.LobbyPatch;

namespace BetterAmongUs.Mono;

/// <summary>
/// Provides enhanced ping tracking and display functionality with additional information.
/// Extends the default Among Us ping tracker with custom features.
/// </summary>
internal sealed class BetterPingTracker : MonoBehaviour
{
    /// <summary>
    /// Gets the singleton instance of the BetterPingTracker.
    /// </summary>
    /// <value>The current instance, or null if not initialized.</value>
    internal static BetterPingTracker? Instance { get; private set; }

    private AspectPosition? aspectPosition;
    private TextMeshPro? text;

    /// <summary>
    /// Initializes the BetterPingTracker with the required UI components.
    /// </summary>
    /// <param name="pingText">The TextMeshPro component for displaying ping information.</param>
    /// <param name="pingAspectPosition">The AspectPosition component for positioning the display.</param>
    internal void SetUp(TextMeshPro pingText, AspectPosition pingAspectPosition)
    {
        if (Instance != null)
            return;

        if (pingText == null || pingAspectPosition == null)
        {
            Logger_.Error("BetterPingTracker.SetUp() called with null parameters!");
            return;
        }

        Instance = this;
        text = pingText;
        aspectPosition = pingAspectPosition;
    }

    /// <summary>
    /// Updates the ping tracker display every frame.
    /// </summary>
    private void Update()
    {
        if (aspectPosition == null || text == null)
            return;

        // Update position and appearance
        aspectPosition.DistanceFromEdge = new Vector3(4f, 0.1f, -5);
        aspectPosition.Alignment = AspectPosition.EdgeAlignments.RightTop;
        text.outlineWidth = 0.3f;

        StringBuilder sb = new();

        // Check AmongUsClient.Instance
        if (AmongUsClient.Instance != null && !GameState.IsFreePlay)
        {
            string pingColor = Utils.Color32ToHex(Utils.LerpColor([Color.green, Color.yellow, new Color(1f, 0.5f, 0f), Color.red], (25, 250), AmongUsClient.Instance.Ping));
            sb.AppendFormat("{0}: <b>{1}</b>\n", Translator.GetString("Ping").ToUpper(), $"<{pingColor}>{AmongUsClient.Instance.Ping}</color>");
        }

        if (GameState.IsLobby && GameState.IsHost && GameState.IsVanillaServer && !GameState.IsLocalGame)
        {
            string timeColor = Utils.Color32ToHex(Utils.LerpColor([Color.green, Color.yellow, new Color(1f, 0.5f, 0f), Color.red], (0, 300), lobbyTimer, true));
            sb.AppendFormat("{0}: <b>{1}</b>\n", Translator.GetString("Timer").ToUpper(), $"<{timeColor}>{lobbyTimerDisplay}</color>");
        }

        sb.Append($"<color=#00dbdb><size=75%>BetterAmongUs {BAUPlugin.GetVersionText(true)}</size></color>\n");
        sb.Append($"<color=#8A8A8A>{ModInfo.GITHUB}</color>\n".Size(52f));

        if (BAUConfigs.ShowFPS.Value)
        {
            float FPSNum = 1.0f / Time.deltaTime;
            sb.AppendFormat("<color=#0dff00><size=75%>FPS: <b>{0}</b></size></color>\n", (int)FPSNum);
        }

        // Add Host Info if not in lobby
        if (GameState.IsInGamePlay && !GameState.IsFreePlay && AmongUsClient.Instance != null && !GameState.IsMeeting)
        {
            var hostInfo = AmongUsClient.Instance.GetHost();
            if (hostInfo != null && hostInfo.Character != null)
            {
                sb.AppendFormat("<size=75%>{0}: {1}</size>\n", Translator.GetString("Host"), hostInfo.Character.GetPlayerNameAndColor());
            }
        }

        text?.SetText(sb.ToString());
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}