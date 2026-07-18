using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class AddVoteHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.AddVote;
}
