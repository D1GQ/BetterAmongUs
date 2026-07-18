using BetterAmongUs.Data;
using BetterAmongUs.Data.Config;
using BetterAmongUs.Generated;
using BetterAmongUs.Modules;
using BetterAmongUs.Modules.Support;
using BetterAmongUs.MonoScripts.Extended;
using BetterAmongUs.Structs;
using BetterAmongUs.Utilities;
using BetterAmongUs.Utilities.Extension;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Patches.Gameplay.UI.Chat;

[HarmonyPatch]
internal static class ChatPatch
{
    internal static List<string> ChatHistory = [];
    internal static int CurrentHistorySelection = -1;

    internal const string COMMAND_POSTFIX_ID = "<size=0%>IsCommand</size>";

    /// <summary>
    /// Removes all chat bubbles from the chat.
    /// </summary>
    internal static void ClearChat()
    {
        if (!HudManager.InstanceExists)
            return;

        // Clear all chat bubbles
        HudManager.Instance.Chat.chatBubblePool.ReclaimAll();
    }

    /// <summary>
    /// Removes all player chat bubbles from the chat, excluding command bubbles.
    /// </summary>
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

    /// <summary>
    /// Contains cached information about censored chat bubbles, keyed by the associated chat bubble instance.
    /// </summary>
    private static readonly Dictionary<ChatBubble, (int index, NetworkedPlayerInfo info, string name, string msg)> _censoredChatBubbleInfo = [];

    /// <summary>
    /// Censors all active player chat bubbles by anonymizing player information and obscuring chat text.
    /// </summary>
    internal static void CensorPlayerChats()
    {
        if (!HudManager.InstanceExists)
            return;

        UncensorPlayerChats();

        foreach (var obj in HudManager.Instance.Chat.chatBubblePool.activeChildren.ToArray())
        {
            var chatBubble = obj.GetComponent<ChatBubble>();
            if (chatBubble != null)
            {
                if (_censoredChatBubbleInfo.ContainsKey(chatBubble)) continue;
                if (chatBubble.NameText.text.EndsWith(COMMAND_POSTFIX_ID)) continue;

                _censoredChatBubbleInfo[chatBubble] = (chatBubble.PoolIndex, chatBubble.playerInfo, chatBubble.NameText.text, chatBubble.TextArea.text);

                // Anonymize player info and obscure chat text
                chatBubble.playerInfo = null;
                chatBubble.NameText.SetText("???".ToColor(Color.white));
                chatBubble.TextArea.SetText(new string('*', chatBubble.TextArea.text.Length).ToColor(Color.white));
                chatBubble.Player.ResetCosmetics();
                chatBubble.Player.SetBodyColor(15);
                chatBubble.ColorBlindName.SetText(string.Empty);
            }
        }

        HudManager.Instance.Chat.AlignAllBubbles();
    }

    /// <summary>
    /// Restores all previously censored player chat bubbles to display their original, uncensored content in the chat
    /// UI.
    /// </summary>
    internal static void UncensorPlayerChats()
    {
        if (!HudManager.InstanceExists)
            return;

        foreach (var (chatBubble, (index, info, name, msg)) in _censoredChatBubbleInfo)
        {
            if (chatBubble == null) continue;

            // Ensure the chat bubble matches the cached pool index before restoring
            if (index != chatBubble.PoolIndex) continue;

            // Restore original player info and chat text
            chatBubble.playerInfo = info;
            chatBubble.NameText.SetText(name);
            chatBubble.TextArea.SetText(msg);
            chatBubble.Player.UpdateFromPlayerData(chatBubble.playerInfo, PlayerOutfitType.Default, PlayerMaterial.MaskType.ScrollingUI, false);
            chatBubble.AlignChildren();
        }

        _censoredChatBubbleInfo.Clear();
        HudManager.Instance.Chat.AlignAllBubbles();
    }

    /// <summary>
    /// Removes all command related chat bubbles from the chat, preserving player chat bubbles.
    /// </summary>
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
    private static void ChatController_SetChatBubbleName_Postfix(ChatBubble bubble, NetworkedPlayerInfo playerInfo, bool isDead, bool didVote)
    {
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_ChatNameOverride))
            return;

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

        SplitStringBuilder ssbTag = new(100, '-');

        string hashPuid = Utils.GetHashPuid(sourcePlayer);
        string friendCode = playerInfo.FriendCode;
        string playerName = playerInfo.ExtendedData()?.RealName ?? "???";

        // In lobby, show player tags instead of roles
        if (GameState.IsLobby && !GameState.IsFreePlay)
        {
            var betterData = sourcePlayer.ExtendedData();
            if (betterData == null)
                return;

            // Show BAU user tag
            if (sourcePlayer.IsLocalPlayer() || betterData.IsBetterUser)
            {
                ssbTag.AppendFormat("<color=#0dff00>{1}{0}</color>", TranslationStrings.Player_BetterUser.LocalizedString, betterData.IsVerifiedBetterUser || sourcePlayer.IsLocalPlayer() ? "✓ " : "");
            }

            // Show mod-specific tags based on player data
            if (BetterDataManager.Files.BetterDataFile.TryGetCheatInfo(sourcePlayer.Data, out var info))
            {
                ssbTag.Append(info.title.ToColor(info.hexColor));
            }
        }

        ssbTag.Append(sourcePlayer.GetRoleInfo(false).Size(75f));

        string infoText = ssbTag.ToString().Size(75f);

        // Position tags before local player name, after other players' names
        if (sourcePlayer.IsLocalPlayer())
            playerName = infoText + " " + playerName;
        else
            playerName += " " + infoText;

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
        __instance.textArea.characterLimit = ModInfo.Constants.MAX_CHAT_TEXT;
        __instance.charCountText.text = "0/" + ModInfo.Constants.MAX_CHAT_TEXT;
    }

    [HarmonyPatch(typeof(FreeChatInputField), nameof(FreeChatInputField.UpdateCharCount))]
    [HarmonyPostfix]
    private static void FreeChatInputField_UpdateCharCount_Postfix(FreeChatInputField __instance)
    {
        // Update character counter with color coding
        int length = __instance.textArea.text.Length;
        __instance.charCountText.text = string.Format("{0}/" + ModInfo.Constants.MAX_CHAT_TEXT, length);
        __instance.charCountText.color = GetCharColor(length);
    }

    private static Color GetCharColor(int length)
    {
        // Color gradient: green -> yellow -> red as text length increases
        Color[] colorGradient = [Color.green, Color.yellow, Color.red];
        (float min, float max) lerpRange = (0f, ModInfo.Constants.MAX_CHAT_TEXT - 1);
        return colorGradient.LerpColor(lerpRange, length);
    }
}
