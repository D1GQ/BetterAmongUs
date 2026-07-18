using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers.Deprecated;

[RegisterRPCHandler]
internal sealed class SetVisorHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SetVisor_Deprecated;
}
