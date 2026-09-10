using AmongUs.Data;
using BetterAmongUs.Attributes;
using BetterAmongUs.Data;
using BetterAmongUs.Utilities;
using BetterAmongUs.Managers;
using BetterAmongUs.Patches.Gameplay.UI.Settings;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class SendChatHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SendChat;

    internal override void Handle(PlayerControl? sender, MessageReader reader)
    {
        var text = reader.ReadString();

        if (sender == null || !GameState.IsHost || sender.IsLocalPlayer()) return;

        if (BetterGameSettings.UseBanWordList.GetBool() && (!BetterGameSettings.UseBanWordListOnlyLobby.GetBool() || GameState.IsLobby))
        {
            if (TextFileHandler.CompareChatFilters(BetterDataManager.Files.banWordListFilePath, text))
            {
                sender.Kick(false, $"has been kicked due to\nchat message containing a banned word!");
            }
        }
    }

    internal override void HandleAntiCheat(PlayerControl? sender, MessageReader reader)
    {
        if (sender.IsAlive() && GameState.IsInGamePlay && !GameState.IsMeeting && !GameState.IsExilling)
        {
            if (BetterNotificationManager.NotifyCheat(sender, GetFormatActionText()))
            {
                string issue = GetChatIssue(sender);
                LogRpcInfo($"Invalid chat attempt: {issue}");
            }
        }
    }

    private static string GetChatIssue(PlayerControl sender)
    {
        if (sender.IsAlive() && GameState.IsInGamePlay && !GameState.IsMeeting && !GameState.IsExilling)
            return "Alive player chatting during gameplay";
        if (DataManager.Settings.Multiplayer.ChatMode == InnerNet.QuickChatModes.QuickChatOnly)
            return "Quick chat only mode";

        return "Unknown chat issue";
    }
}
