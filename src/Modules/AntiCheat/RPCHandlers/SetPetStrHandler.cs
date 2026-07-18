using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class SetPetStrHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SetPetStr;
}
