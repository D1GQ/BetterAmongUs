using AmongUs.GameOptions;
using BetterAmongUs.Attributes;
using BetterAmongUs.Utilities;
using BetterAmongUs.Managers;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class ExitVentHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.ExitVent;

    internal override void HandleAntiCheat(PlayerControl? sender, MessageReader reader)
    {
        if (!sender.IsImpostorTeam() && !sender.Is(RoleTypes.Engineer))
        {
            if (BetterNotificationManager.NotifyCheat(sender, GetFormatActionText()))
            {
                LogRpcInfo($"Non-impostor and non-engineer attempted ExitVent RPC");
            }
        }
    }
}
