using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers.Deprecated;

[RegisterRPCHandler]
internal sealed class SetNamePlateHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SetNamePlate_Deprecated;
}
