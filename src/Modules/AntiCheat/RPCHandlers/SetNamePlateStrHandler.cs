using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class SetNamePlateStrHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SetNamePlateStr;
}
