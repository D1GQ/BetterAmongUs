using BetterAmongUs.Attributes;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class CheckZiplineHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.CheckZipline;

    internal override bool HandleAntiCheatCancel(PlayerControl? sender, MessageReader reader)
    {
        if (!GameState.IsHost)
        {
            return false;
        }

        return true;
    }
}
