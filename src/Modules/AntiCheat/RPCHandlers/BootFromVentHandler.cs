using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class BootFromVentHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.BootFromVent;
}
