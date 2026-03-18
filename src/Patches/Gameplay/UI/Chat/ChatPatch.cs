using AmongUs.GameOptions;
using BetterAmongUs.Data;
using BetterAmongUs.Data.Config;
using BetterAmongUs.Helpers;
using BetterAmongUs.Modules;
using BetterAmongUs.Mono;
using HarmonyLib;
using System.Text;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Patches.Gameplay.UI.Chat;

[HarmonyPatch]
internal static class ChatPatch
{
    internal static List<string> ChatHistory = [];
    internal static int CurrentHistorySelection = -1;

    internal const string COMMAND_POSTFIX_ID = "<size=0%>IsCommand</size>";

    internal static void ClearChat()
    {
        if (!HudManager.InstanceExists)
            return;

        // Clear all chat bubbles
        HudManager.Instance.Chat.chatBubblePool.ReclaimAll();
    }

    internal static void ClearPlayerChats()
    {
        if (!HudManager.InstanceExists)
            return;

        // Clear only player chat bubbles (keep command bubbles)
        foreach (var obj in HudManager.Instance.Chat.chatBubblePool.activeChildren.ToArray())
        {
            var chatBubble = obj.GetComponent<ChatBubble>();
            if (chatBubble != null)
            {
                if (chatBubble.NameText.text.EndsWith(COMMAND_POSTFIX_ID)) continue;
                HudManager.Instance.Chat.chatBubblePool.Reclaim(chatBubble);
            }
        }
        HudManager.Instance.Chat.AlignAllBubbles();
    }

    internal static void ClearCommands()
    {
        if (!HudManager.InstanceExists)
            return;

        // Clear only command chat bubbles (keep player chat)
        foreach (var obj in HudManager.Instance.Chat.chatBubblePool.activeChildren.ToArray())
        {
            var chatBubble = obj.GetComponent<ChatBubble>();
            if (chatBubble != null)
            {
                if (!chatBubble.NameText.text.EndsWith(COMMAND_POSTFIX_ID)) continue;
                HudManager.Instance.Chat.chatBubblePool.Reclaim(chatBubble);
            }
        }
        HudManager.Instance.Chat.AlignAllBubbles();
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Toggle))]
    [HarmonyPostfix]
    private static void ChatController_Toggle_Postfix()
    {
        // Apply chat theme when chat is opened/closed
        SetChatTheme();
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static void ChatController_Update_Prefix(ChatController __instance)
    {
        // Apply dark/light theme to chat input field
        if (BAUConfigs.ChatDarkMode.Value)
        {
            // Free chat color
            __instance.freeChatField.background.color = new Color32(40, 40, 40, byte.MaxValue);
            __instance.freeChatField.textArea.compoText.Color(Color.white);
            __instance.freeChatField.textArea.outputText.color = Color.white;
        }
        else
        {
            // Free chat color
            __instance.freeChatField.background.color = new Color32(255, 255, 255, byte.MaxValue);
            __instance.freeChatField.textArea.compoText.Color(Color.black);
            __instance.freeChatField.textArea.outputText.color = Color.black;
        }
        // Ctrl+x to cut text to clipboard
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.X))
        {
            ClipboardHelper.PutClipboardString(__instance.freeChatField.textArea.text);
            __instance.freeChatField.textArea.SetText("");
        }
        // Up arrow for chat history navigation
        if (Input.GetKeyDown(KeyCode.UpArrow) && ChatHistory.Any())
        {
            CurrentHistorySelection = Mathf.Clamp(--CurrentHistorySelection, 0, ChatHistory.Count - 1);
            __instance.freeChatField.textArea.SetText(ChatHistory[CurrentHistorySelection]);
        }
        // Down arrow for chat history navigation
        if (Input.GetKeyDown(KeyCode.DownArrow) && ChatHistory.Any())
        {
            CurrentHistorySelection++;
            if (CurrentHistorySelection < ChatHistory.Count)
                __instance.freeChatField.textArea.SetText(ChatHistory[CurrentHistorySelection]);
            else __instance.freeChatField.textArea.SetText("");
        }
    }
    // Log chat messages to console
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
    [HarmonyPostfix]
    private static void ChatController_AddChat_Postfix(ChatController __instance, PlayerControl sourcePlayer, string chatText)
    {
        // Log chat publicly if player is alive, privately if dead
        if (sourcePlayer.IsAlive() || !PlayerControl.LocalPlayer.IsAlive())
        {
            Logger_.Log($"{sourcePlayer.Data.PlayerName} -> {chatText}", "ChatLog");
        }
        else
        {
            Logger_.LogPrivate($"{sourcePlayer.Data.PlayerName} -> {chatText}", "ChatLog");
        }
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SetChatBubbleName))]
    [HarmonyPostfix]
    private static void ChatController_SetChatBubbleName_Postfix(ChatController __instance, ChatBubble bubble, NetworkedPlayerInfo playerInfo, bool isDead, bool didVote)
    {
        if (playerInfo == null)
            return;

        if (didVote)
            return;

        if (bubble == null)
            return;

        if (bubble.NameText == null)
            return;

        var sourcePlayer = playerInfo.Object;
        if (sourcePlayer == null)
            return;

        var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null)
            return;

        StringBuilder sbTag = new();
        StringBuilder sbInfo = new();

        string hashPuid = Utils.GetHashPuid(sourcePlayer);
        string friendCode = playerInfo.FriendCode;
        string playerName = playerInfo.BetterData()?.RealName ?? "???";

        // Format role display with team color
        string Role = $"<size=75%><color={sourcePlayer.GetTeamHexColor()}>{sourcePlayer.GetRoleName()}</color></size>+++";

        // In lobby, show player tags instead of roles
        if (GameState.IsLobby && !GameState.IsFreePlay)
        {
            Role = "";

            var betterData = sourcePlayer.BetterData();
            if (betterData == null)
                return;

            // Show BAU user tag
            if (sourcePlayer.IsLocalPlayer() || betterData.IsBetterUser)
            {
                sbTag.AppendFormat("<color=#0dff00>{1}{0}</color>+++", Translator.GetString("Player.BetterUser"), betterData.IsVerifiedBetterUser || sourcePlayer.IsLocalPlayer() ? "✓ " : "");
            }

            // Show mod-specific tags based on player data
            if (BetterDataManager.BetterDataFile.SickoData.Any(info => info.CheckPlayerData(sourcePlayer.Data)))
                sbTag.Append($"<color=#00f583>{Translator.GetString("Player.SickoUser")}</color>+++");
            else if (BetterDataManager.BetterDataFile.AUMData.Any(info => info.CheckPlayerData(sourcePlayer.Data)))
                sbTag.Append($"<color=#4f0000>{Translator.GetString("Player.AUMUser")}</color>+++");
            else if (BetterDataManager.BetterDataFile.KNData.Any(info => info.CheckPlayerData(sourcePlayer.Data)))
                sbTag.Append($"<color=#8731e7>{Translator.GetString("Player.KNUser")}</color>+++");
            else if (BetterDataManager.BetterDataFile.CheatData.Any(info => info.CheckPlayerData(sourcePlayer.Data)))
                sbTag.Append($"<color=#fc0000>{Translator.GetString("Player.KnownCheater")}</color>+++");
        }

        // Hide roles from alive players (unless same team)
        if (!sourcePlayer.IsImpostorTeammate())
        {
            if (localPlayer.IsAlive() && !sourcePlayer.IsLocalPlayer())
                Role = "";
        }

        // Show role for dead players or if local player is Guardian Angel
        if ((localPlayer.Is(RoleTypes.GuardianAngel) && !sourcePlayer.IsAlive()) || !localPlayer.Is(RoleTypes.GuardianAngel))
        {
            sbTag.Append(Role);
        }

        // Format tags with separators
        sbInfo.Append("<size=75%>");
        var parts = sbTag.ToString().Split("+++");

        for (int i = 0; i < parts.Length; i++)
        {
            if (string.IsNullOrEmpty(parts[i])) continue;

            sbInfo.Append(parts[i]);

            if (i != parts.Length - 2)
                sbInfo.Append(" - ");
        }

        sbInfo.Append("</size>");

        // Position tags before local player name, after other players' names
        if (sourcePlayer.IsLocalPlayer())
            playerName = sbInfo + " " + playerName;
        else
            playerName += " " + sbInfo;

        bubble.NameText.SetText(playerName);
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.GetPooledBubble))]
    [HarmonyPostfix]
    private static void ChatController_GetPooledBubble_Postfix(ChatController __instance, ChatBubble __result)
    {
        SetChatPoolTheme(__result);
    }

    internal static void SetChatTheme()
    {
        var chat = HudManager.Instance.Chat;

        if (BAUConfigs.ChatDarkMode.Value)
        {
            // Quick chat color
            chat.quickChatField.background.color = new Color32(40, 40, 40, byte.MaxValue);
            chat.quickChatField.text.color = Color.white;
            // Icons
            chat.quickChatButton.transform.Find("QuickChatIcon").GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 1f);
            chat.openKeyboardButton.transform.Find("OpenKeyboardIcon").GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 1f);
        }
        else
        {
            // Quick chat color
            chat.quickChatField.background.color = new Color32(255, 255, 255, byte.MaxValue);
            chat.quickChatField.text.color = Color.black;
            // Icons
            chat.quickChatButton.transform.Find("QuickChatIcon").GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
            chat.openKeyboardButton.transform.Find("OpenKeyboardIcon").GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        }
        // Apply theme to all existing chat bubbles
        foreach (var item in HudManager.Instance.Chat.chatBubblePool.activeChildren.SelectIl2Cpp(c => c.GetComponent<ChatBubble>()))
        {
            SetChatPoolTheme(item);
        }
    }
    // Apply theme to individual chat bubble
    internal static ChatBubble SetChatPoolTheme(ChatBubble asChatBubble)
    {
        var chatBubble = asChatBubble;

        if (BAUConfigs.ChatDarkMode.Value)
        {
            chatBubble.transform.Find("ChatText (TMP)").GetComponentInChildren<TextMeshPro>(true).color = Color.white;
            chatBubble.transform.Find("Background").GetComponentInChildren<SpriteRenderer>(true).color = new Color(0.15f, 0.15f, 0.15f, 1f);

            var mark = chatBubble.transform.Find("PoolablePlayer/xMark");
            if (mark != null && mark.GetComponentInChildren<SpriteRenderer>(true).enabled)
            {
                chatBubble.transform.Find("Background").GetComponentInChildren<SpriteRenderer>(true).color = new Color(0.15f, 0.15f, 0.15f, 0.5f);
            }
        }
        else
        {
            chatBubble.transform.Find("ChatText (TMP)").GetComponentInChildren<TextMeshPro>(true).color = Color.black;
            chatBubble.transform.Find("Background").GetComponentInChildren<SpriteRenderer>(true).color = Color.white;

            var mark = chatBubble.transform.Find("PoolablePlayer/xMark");
            if (mark != null && mark.GetComponentInChildren<SpriteRenderer>(true).enabled)
            {
                chatBubble.transform.Find("Background").GetComponentInChildren<SpriteRenderer>(true).color = new Color(1f, 1f, 1f, 0.5f);
            }
        }

        return chatBubble;
    }

    [HarmonyPatch(typeof(FreeChatInputField), nameof(FreeChatInputField.Awake))]
    [HarmonyPostfix]
    private static void FreeChatInputField_Awake_Postfix(FreeChatInputField __instance)
    {
        // Enable extended character support for chat
        __instance.textArea.allowAllCharacters = true;
        __instance.textArea.AllowSymbols = true;
        __instance.textArea.AllowPaste = true;
        __instance.textArea.AllowEmail = true;
        __instance.textArea.characterLimit = 118;
        __instance.charCountText.text = "0/118";
    }

    [HarmonyPatch(typeof(FreeChatInputField), nameof(FreeChatInputField.UpdateCharCount))]
    [HarmonyPostfix]
    private static void FreeChatInputField_UpdateCharCount_Postfix(FreeChatInputField __instance)
    {
        // Update character counter with color coding
        int length = __instance.textArea.text.Length;
        __instance.charCountText.text = string.Format("{0}/118", length);
        __instance.charCountText.color = GetCharColor(length);
    }

    private static Color GetCharColor(int length)
    {
        // Color gradient: green -> yellow -> red as text length increases
        Color[] colorGradient = [Color.green, Color.yellow, Color.red];
        (float min, float max) lerpRange = (0f, 117f);
        return colorGradient.LerpColor(lerpRange, length);
    }
}
