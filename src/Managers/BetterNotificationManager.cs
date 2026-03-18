using BetterAmongUs.Data;
using BetterAmongUs.Data.Config;
using BetterAmongUs.Helpers;
using BetterAmongUs.Modules;
using BetterAmongUs.Mono;
using BetterAmongUs.Patches.Gameplay.UI.Settings;
using Cpp2IL.Core.Extensions;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Managers;

/// <summary>
/// Manages in-game notifications for BetterAmongUs, including cheat detection alerts and system messages.
/// </summary>
internal static class BetterNotificationManager
{
    private static GameObject? BAUNotificationManagerObj;
    private static TextMeshPro? NameText;
    private static TextMeshPro? TextArea;
    private readonly static Dictionary<string, float> NotifyQueue = [];
    private static float showTime = 0f;
    private static Camera? localCamera;
    private static bool Notifying = false;

    /// <summary>
    /// Initializes the BetterNotificationManager by creating and configuring a cloned instance of the game's chat notification system.
    /// </summary>
    internal static void Init()
    {
        if (BAUNotificationManagerObj == null)
        {
            var ChatNotifications = HudManager.Instance.Chat.chatNotification;
            if (ChatNotifications != null)
            {
                ChatNotifications.timeOnScreen = 1f;
                ChatNotifications.gameObject.SetActive(true);

                // Clone chat notification system for BAU notifications
                GameObject BAUNotification = UnityEngine.Object.Instantiate(ChatNotifications.gameObject);
                BAUNotification.name = "BAUNotification";
                BAUNotification.GetComponent<ChatNotification>().DestroyMono();

                // Remove unnecessary elements from the clone
                GameObject.Find($"{BAUNotification.name}/Sizer/PoolablePlayer").DestroyObj();
                GameObject.Find($"{BAUNotification.name}/Sizer/ColorText").DestroyObj();

                // Position notification at bottom-left corner
                BAUNotification.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(-1.57f, 5.3f, -15f);
                GameObject.Find($"{BAUNotification.name}/Sizer/NameText").transform.localPosition = new Vector3(-3.3192f, -0.0105f);

                // Cache TextMeshPro component for text updates
                NameText = GameObject.Find($"{BAUNotification.name}/Sizer/NameText").GetComponent<TextMeshPro>();
                UnityEngine.Object.DontDestroyOnLoad(BAUNotification);
                BAUNotificationManagerObj = BAUNotification;
                BAUNotification.SetActive(false);

                // Reset original chat notification settings
                ChatNotifications.timeOnScreen = 0f;
                ChatNotifications.gameObject.SetActive(false);

                // Configure text wrapping for multi-line notifications
                TextArea = BAUNotificationManagerObj.transform.Find("Sizer/ChatText (TMP)").GetComponent<TextMeshPro>();
                TextArea.enableWordWrapping = true;
                TextArea.m_firstOverflowCharacterIndex = 0;
                TextArea.overflowMode = TextOverflowModes.Overflow;
            }
        }
    }

    /// <summary>
    /// Cleans up and destroys the notification manager.
    /// </summary>
    internal static void Detach()
    {
        NotifyQueue.Clear();

        if (BAUNotificationManagerObj == null)
            return;

        BAUNotificationManagerObj.DestroyObj();
    }

    /// <summary>
    /// Displays a notification message in-game.
    /// </summary>
    /// <param name="text">The text to display in the notification.</param>
    /// <param name="Time">The duration in seconds to show the notification.</param>
    internal static void Notify(string text, float Time = 5f)
    {
        if (!BAUConfigs.BetterNotifications.Value)
            return;

        if (BAUNotificationManagerObj == null)
            return;

        if (Notifying)
        {
            if (text == TextArea.text)
                return;

            NotifyQueue[text] = Time;
            return;
        }

        showTime = Time;
        BAUNotificationManagerObj.SetActive(true);
        NameText.text = $"<color=#00ff44>{Translator.GetString("SystemNotification")}</color>";
        TextArea.text = text;
        SoundManager.Instance.PlaySound(HudManager.Instance.TaskCompleteSound, false, 1f);
        Notifying = true;
    }

    /// <summary>
    /// Handles cheat detection notifications and actions.
    /// </summary>
    /// <param name="player">The player who was detected cheating.</param>
    /// <param name="reason">The reason for the cheat detection.</param>
    /// <param name="newText">Optional custom text to replace the default detection message.</param>
    /// <param name="kickPlayer">Whether to kick the detected player.</param>
    /// <param name="forceBan">Whether to force a ban regardless of settings.</param>
    /// <returns>True if the cheat detection was handled, false otherwise.</returns>
    internal static bool NotifyCheat(PlayerControl player, string reason, string newText = "", bool kickPlayer = true, bool forceBan = false)
    {
        if (player == null)
            return false;

        if (player.Data == null)
            return false;

        if (player.IsCheater())
            return false;

        if (player.IsLocalPlayer())
        {
            /*
            FileChecker.SetHasUnauthorizedFileOrMod();
            FileChecker.SetWarningMsg("Tampered client detected!");
            Utils.DisconnectSelf("Tampered client detected!");
            Utils.DisconnectAccountFromOnline();
            */
            return false;
        }

        var Reason = reason;
        if (BetterGameSettings.CensorDetectionReason.GetBool())
        {
            Reason = string.Concat('*').Repeat(reason.Length);
        }

        string playerDetected = Translator.GetString("AntiCheat.PlayerDetected");
        string unauthorizedAction = Translator.GetString("AntiCheat.UnauthorizedAction");
        string byAntiCheat = Translator.GetString("AntiCheat.ByAntiCheat");
        string playerDetectedLog = Translator.GetString("AntiCheat.PlayerDetected", useConsoleLanguage: true);
        string unauthorizedActionLog = Translator.GetString("AntiCheat.UnauthorizedAction", useConsoleLanguage: true);

        string text = $"{playerDetected}: <color=#0097b5>{player?.BetterData().RealName}</color> {unauthorizedAction}: <b><color=#fc0000>{Reason}</color></b>";
        string rawText = $"{playerDetectedLog}: <color=#0097b5>{player?.BetterData().RealName}</color> {unauthorizedActionLog}: <b><color=#fc0000>{reason}</color></b>";

        if (newText != "")
        {
            text = $"{playerDetected}: <color=#0097b5>{player?.BetterData().RealName}</color> " + newText + $": <b><color=#fc0000>{Reason}</color></b>";
            rawText = $"{playerDetectedLog}: <color=#0097b5>{player?.BetterData().RealName}</color> " + newText + $": <b><color=#fc0000>{reason}</color></b>";
        }

        if (!BetterDataManager.BetterDataFile.CheatData.Any(info => info.CheckPlayerData(player.Data)))
        {
            BetterDataManager.BetterDataFile.CheatData.Add(new(player?.BetterData().RealName ?? player.Data.PlayerName, player.GetHashPuid(), player.Data.FriendCode, reason));
            BetterDataManager.BetterDataFile.Save();
            Notify(text, Time: 8f);
        }

        Logger_.LogCheat($"{player.cosmetics.nameText.text} Info: {player.Data.PlayerName} - {player.Data.FriendCode} - {player.GetHashPuid()}");
        Logger_.LogCheat(Utils.RemoveHtmlText(rawText));

        if (GameState.IsHost && kickPlayer)
        {
            string kickMessage = string.Format(Translator.GetString("AntiCheat.KickMessage"), byAntiCheat, Reason);
            player.Kick(true, kickMessage, true, false, forceBan);
        }

        return true;
    }

    /// <summary>
    /// Clears all pending and active notifications by resetting the notification queue, display timer, and notification state.
    /// </summary>
    internal static void ClearNotifications()
    {
        NotifyQueue.Clear();
        showTime = 0f;
        Notifying = false;
    }

    /// <summary>
    /// Updates the notification manager each frame.
    /// </summary>
    internal static void Update()
    {
        if (BAUNotificationManagerObj == null)
            return;

        if (TextArea == null)
            return;

        if (!localCamera)
        {
            if (HudManager.InstanceExists)
            {
                localCamera = HudManager.Instance.GetComponentInChildren<Camera>();
            }
            else
            {
                localCamera = Camera.main;
            }
        }

        BAUNotificationManagerObj.transform.position = AspectPosition.ComputeWorldPosition(localCamera, AspectPosition.EdgeAlignments.Bottom, new Vector3(-1.3f, 0.7f, localCamera.nearClipPlane + 0.1f));

        showTime -= Time.deltaTime;
        if (showTime <= 0f && GameState.IsInGame)
        {
            TextArea.text = "";
            BAUNotificationManagerObj.SetActive(false);
            Notifying = false;

            CheckNotifyQueue();
        }

        if (!GameState.IsInGame)
        {
            BAUNotificationManagerObj.SetActive(false);
            showTime = 0f;
        }
    }

    /// <summary>
    /// Checks and processes queued notifications.
    /// </summary>
    private static void CheckNotifyQueue()
    {
        if (NotifyQueue.Any())
        {
            var key = NotifyQueue.Keys.First();
            var value = NotifyQueue[key];
            Notify(key, value);
            NotifyQueue.Remove(key);
        }
    }
}