using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers.Deprecated;

[RegisterRPCHandler]
internal sealed class SetInfectedHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SetInfected;
}
