using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class SetColorHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SetColor;
}
