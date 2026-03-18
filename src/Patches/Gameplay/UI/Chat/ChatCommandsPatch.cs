using BetterAmongUs.Commands;
using BetterAmongUs.Data.Config;
using BetterAmongUs.Enums;
using BetterAmongUs.Helpers;
using BetterAmongUs.Modules;
using BetterAmongUs.Modules.Support;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Patches.Gameplay.UI.Chat;

[HarmonyPatch]
internal static class ChatCommandsPatch
{
    private static bool _enabled = true;
    internal static string CommandPrefix => BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Force_BAU_Command_Prefix) ? "bau:" : BAUConfigs.CommandPrefix.Value;

    // Execute command when valid command is typed
    private static void HandleCommand()
    {
        if (closestCommand != null && isTypedOut)
        {
            closestCommand.Run();
        }
        else
        {
            Utils.AddChatPrivate("<color=#f50000><size=150%><b>Invalid Command!</b></size></color>");
        }
    }

    // Intercept chat messages to handle commands
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    [HarmonyPrefix]
    private static bool ChatController_SendChat_Prefix(ChatController __instance)
    {
        // Skip if commands are disabled
        if (!_enabled || BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_AllCommands))
        {
            return true;
        }

        // Check chat cooldown
        bool IsOnCooldown = 3f - __instance.timeSinceLastMessage > 0f;

        string text = __instance.freeChatField.textArea.text;

        // If not a command, handle as normal chat (with restrictions)
        if (!text.StartsWith(CommandPrefix) || IsOnCooldown)
        {
            // Prevent chat during gameplay if not in meeting
            if (GameState.IsInGame && !GameState.IsLobby && !GameState.IsFreePlay && !GameState.IsMeeting && !GameState.IsExilling && PlayerControl.LocalPlayer.IsAlive())
                return false;

            // Add to chat history
            if (ChatPatch.ChatHistory.Count == 0 || ChatPatch.ChatHistory[^1] != text) ChatPatch.ChatHistory.Add(text);
            ChatPatch.CurrentHistorySelection = ChatPatch.ChatHistory.Count;
            return true;
        }

        // Check if command can be executed
        if (closestCommand != null && !closestCommand.CanRunCommand(out string _))
        {
            return false;
        }

        // Execute command
        HandleCommand();

        // Add command to chat history
        if (ChatPatch.ChatHistory.Count == 0 || ChatPatch.ChatHistory[^1] != text) ChatPatch.ChatHistory.Add(text);
        ChatPatch.CurrentHistorySelection = ChatPatch.ChatHistory.Count;

        // Reset chat timer if command sets it
        if (closestCommand != null && closestCommand.SetChatTimer == true)
        {
            __instance.timeSinceLastMessage = 0f;
        }

        // Clear chat input
        __instance.freeChatField.Clear();
        __instance.quickChatMenu.Clear();
        __instance.quickChatField.Clear();

        return false;
    }

    // Set up command helper UI elements
    private static TextMeshPro? commandText;
    private static TextMeshPro? commandInfo;

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Toggle))]
    [HarmonyPostfix]
    private static void ChatController_Awake_Postfix(ChatController __instance)
    {
        // Create semi-transparent text for command suggestions
        if (commandText == null)
        {
            var TextArea = __instance.freeChatField.textArea.outputText;
            commandText = UnityEngine.Object.Instantiate(TextArea, TextArea.transform.parent.transform);
            commandText.transform.SetSiblingIndex(TextArea.transform.GetSiblingIndex() + 1);
            commandText.transform.DestroyChildren();
            commandText.name = "CommandArea";
            commandText.GetComponent<TextMeshPro>().color = new Color(1f, 1f, 1f, 0.5f);
        }

        // Create info text for command descriptions and arguments
        if (commandInfo == null)
        {
            var TextArea = __instance.freeChatField.textArea.outputText;
            commandInfo = UnityEngine.Object.Instantiate(TextArea, TextArea.transform.parent.transform);
            commandInfo.transform.SetSiblingIndex(TextArea.transform.GetSiblingIndex() + 1);
            commandInfo.transform.DestroyChildren();
            commandInfo.transform.localPosition = new Vector3(commandInfo.transform.localPosition.x, 0.45f);
            commandInfo.name = "CommandInfoText";
            commandInfo.GetComponent<TextMeshPro>().color = Color.yellow;
            commandInfo.GetComponent<TextMeshPro>().outlineColor = new Color(0f, 0f, 0f, 1f);
            commandInfo.GetComponent<TextMeshPro>().outlineWidth = 0.2f;
            commandInfo.GetComponent<TextMeshPro>().characterWidthAdjustment = 1.5f;
            commandInfo.GetComponent<TextMeshPro>().enableWordWrapping = false;
        }
    }

    private static bool isTypedOut;
    private static string typedCommand = "";
    private static BaseCommand? closestCommand;

    // Update command suggestion UI as user types
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
    [HarmonyPostfix]
    private static void ChatController_Update_Postfix(ChatController __instance)
    {
        if (!_enabled || BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_AllCommands))
        {
            ClearCommandDisplay();
            return;
        }

        string text = __instance.freeChatField.textArea.text;

        if (commandText == null || commandInfo == null)
            return;

        // Check if text starts with command prefix
        if (text.Length > 0 && text.StartsWith(CommandPrefix))
        {
            typedCommand = text.Length > CommandPrefix.Length ? text[CommandPrefix.Length..] : string.Empty;
            string[] typedParts = typedCommand.Split(' ');

            // Find closest matching command
            closestCommand = GetClosestCommand(typedParts[0]);
            bool isSuggestionValid = closestCommand != null
                && (typedParts[0].Equals(closestCommand.Name, StringComparison.OrdinalIgnoreCase) || typedParts.Length == 1);

            if (isSuggestionValid)
            {
                HandleValidSuggestion(__instance, typedParts);
            }
            else
            {
                ClearCommandDisplay();
            }
        }
        else
        {
            ClearCommandDisplay();
        }
    }

    private static void ClearCommandDisplay()
    {
        isTypedOut = false;
        commandText.text = string.Empty;
        commandInfo.text = string.Empty;
    }

    private static void HandleValidSuggestion(ChatController __instance, string[] typedParts)
    {
        isTypedOut = true;

        // Generate suggestion text
        string suggestion = GenerateSuggestion(typedParts);
        string fullSuggestion = CommandPrefix + suggestion;

        // Tab completion
        if (Input.GetKeyDown(KeyCode.Tab) && typedParts.Length >= 1)
        {
            __instance.freeChatField.textArea.SetText(fullSuggestion);
        }

        // Red text if command cannot be run
        if (!closestCommand.CanRunCommand(out string _))
        {
            fullSuggestion = fullSuggestion.ToColor("#FF0300".HexToColor());
        }

        // Update UI elements
        commandText.text = fullSuggestion;
        commandInfo.text = $"{closestCommand.Description}{GenerateArgumentInfo()}{GenerateCanRunInfo()}";
    }

    private static string GenerateSuggestion(string[] typedParts)
    {
        // If only command name typed, return full command name
        if (typedParts.Length == 1)
            return closestCommand.Name;

        // Update command arguments with typed values
        UpdateCommandArguments(typedParts);

        // Get next argument index for autocomplete
        int nextArgumentIndex = typedParts.Length - 2;

        if (nextArgumentIndex >= 0 && nextArgumentIndex < closestCommand.Arguments.Length)
        {
            var argument = closestCommand.Arguments[nextArgumentIndex];
            var closestSuggestion = argument.GetClosestSuggestion();
            // Complete partial argument
            return typedCommand + closestSuggestion[Math.Min(argument.Arg.Length, closestSuggestion.Length)..];
        }

        return string.Empty;
    }

    private static void UpdateCommandArguments(string[] typedParts)
    {
        for (int i = 1; i < typedParts.Length && i <= closestCommand.Arguments.Length; i++)
        {
            if (closestCommand.Arguments[i - 1] != null)
            {
                closestCommand.Arguments[i - 1].Arg = typedParts[i]; // Safe assignment
            }
        }
    }

    private static string GenerateArgumentInfo()
    {
        if (closestCommand.Arguments.Length == 0)
            return string.Empty;

        // Format argument help text
        var argumentInfo = string.Join(" ", closestCommand.Arguments.Select(arg => arg.ArgInfo));
        return $" - <#a6a6a6>{argumentInfo}</color>";
    }

    private static string GenerateCanRunInfo()
    {
        // Show reason if command cannot be run
        if (!closestCommand.CanRunCommand(out string reason))
        {
            return $" - <#FF0300>{reason}</color>";
        }

        return string.Empty;
    }

    internal static BaseCommand? GetClosestCommand(string typedCommand)
    {
        // First try exact match
        var directNormalMatch = BaseCommand.allCommands
            .FirstOrDefault(c => FilterCommand(c, CommandType.Normal) &&
            c.Names.Any(name => string.Equals(name, typedCommand, StringComparison.OrdinalIgnoreCase)));
        if (directNormalMatch != null)
            return directNormalMatch;

        // Then try partial match
        var closestNormalCommand = BaseCommand.allCommands
            .OrderBy(c => c.Name)
            .FirstOrDefault(c => FilterCommand(c, CommandType.Normal) &&
            c.Names.Any(name => name.StartsWith(typedCommand, StringComparison.OrdinalIgnoreCase)));
        if (closestNormalCommand != null)
            return closestNormalCommand;

        return null;
    }

    private static bool FilterCommand(BaseCommand command, CommandType commandType)
    {
        // Check if command is enabled and not disabled by other mods
        return command.Type == commandType && command.ShowCommand() && !BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_Command + command.Name);
    }
}