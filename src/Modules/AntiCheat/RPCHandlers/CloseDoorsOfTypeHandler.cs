using BetterAmongUs.Attributes;
using BetterAmongUs.Utilities;
using BetterAmongUs.Managers;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class CloseDoorsOfTypeHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.CloseDoorsOfType;
    internal override void HandleAntiCheat(PlayerControl? sender, MessageReader reader)
    {
        if (!sender.IsImpostorTeam())
        {
            if (BetterNotificationManager.NotifyCheat(sender, GetFormatActionText()))
            {
                LogRpcInfo($"Non-impostor attempted CloseDoorsOfType RPC");
            }
        }
    }
}
