using BetterAmongUs.Attributes;
using BetterAmongUs.Managers;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class PetHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.Pet;
    internal override void HandleAntiCheat(PlayerControl? sender, MessageReader reader)
    {
        if (sender == null)
            return;

        if (sender.CurrentOutfit == null)
            return;

        if (sender.CurrentOutfit.PetId == PetData.EmptyId)
        {
            if (BetterNotificationManager.NotifyCheat(sender, GetFormatActionText()))
            {
                LogRpcInfo($"Player with empty pet attempted Pet RPC");
            }
        }
    }
}
