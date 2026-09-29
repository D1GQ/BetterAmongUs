using BetterAmongUs.Attributes;
using BetterAmongUs.Enums;
using BetterAmongUs.Generated;
using BetterAmongUs.Utilities;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat.Handlers.RpcHandlers;

[RegisterRpcHandler(RpcCalls.SendQuickChat)]
internal sealed class SendQuickChatRpcHandler : IRpcHandler<PlayerControl>
{
    public AntiCheatFlags CheckRpc(PlayerControl innerNetObject, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
        if (innerNetObject.IsAlive() && GameState.IsInGamePlay && !GameState.IsMeeting && !GameState.IsExilling
            || reader.BytesRemaining == 0)
        {
            /*
            if (BetterNotificationManager.NotifyCheat(sender, GetFormatActionText()))
            {
                string issue = GetChatIssue(sender);
                LogRpcInfo($"Invalid chat attempt: {issue}");
            }
            */

            return AntiCheatFlags.High;
        }

        return AntiCheatFlags.None;
    }

    public void HandleRpc(PlayerControl innerNetObject, MessageReader reader)
    {
    }
}
