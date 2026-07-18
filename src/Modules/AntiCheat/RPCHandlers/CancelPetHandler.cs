using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class CancelPetHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.CancelPet;
}
