using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class SetStartCounterHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SetStartCounter;
}
