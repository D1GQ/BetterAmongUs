using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers.Deprecated;

[RegisterRPCHandler]
internal sealed class SetPetHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SetPet_Deprecated;
}
