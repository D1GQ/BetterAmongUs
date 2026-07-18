using BetterAmongUs.Attributes;
using BetterAmongUs.Utilities;
using BetterAmongUs.Managers;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class SendQuickChatHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SendQuickChat;

    internal override void HandleAntiCheat(PlayerControl? sender, MessageReader reader)
    {
        if (sender.IsAlive() && GameState.IsInGamePlay && !GameState.IsMeeting && !GameState.IsExilling || reader.BytesRemaining == 0)
        {
            if (BetterNotificationManager.NotifyCheat(sender, GetFormatActionText(), forceBan: reader.BytesRemaining == 0))
            {
                string issue = GetQuickChatIssue(sender, reader);
                LogRpcInfo($"Invalid quick chat attempt: {issue}");
            }
        }
    }

    private static string GetQuickChatIssue(PlayerControl sender, MessageReader reader)
    {
        if (sender.IsAlive() && GameState.IsInGamePlay && !GameState.IsMeeting && !GameState.IsExilling)
            return "Alive player using quick chat during gameplay";
        if (reader.BytesRemaining == 0)
            return "Empty quick chat message";

        return "Unknown quick chat issue";
    }
}
