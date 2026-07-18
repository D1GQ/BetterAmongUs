using AmongUs.GameOptions;
using BetterAmongUs.Attributes;
using BetterAmongUs.Utilities;
using BetterAmongUs.Managers;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class ProtectPlayerHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.ProtectPlayer;

    internal override void HandleAntiCheat(PlayerControl? sender, MessageReader reader)
    {
        if (!sender.Is(RoleTypes.GuardianAngel))
        {
            if (BetterNotificationManager.NotifyCheat(sender, GetFormatActionText()))
            {
                LogRpcInfo($"Non-GuardianAngel attempted ProtectPlayer RPC");
            }
        }
    }
}
