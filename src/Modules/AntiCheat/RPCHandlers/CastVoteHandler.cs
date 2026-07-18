using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class CastVoteHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.CastVote;
}
