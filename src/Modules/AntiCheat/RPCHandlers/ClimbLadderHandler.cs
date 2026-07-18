using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class ClimbLadderHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.ClimbLadder;
}
