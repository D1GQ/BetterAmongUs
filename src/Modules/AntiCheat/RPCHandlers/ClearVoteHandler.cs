using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class ClearVoteHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.ClearVote;
}
