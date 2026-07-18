using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class SetHatStrHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SetHatStr;
}
