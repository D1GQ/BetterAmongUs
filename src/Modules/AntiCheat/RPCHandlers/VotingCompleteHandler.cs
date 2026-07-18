using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class VotingCompleteHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.VotingComplete;
}
