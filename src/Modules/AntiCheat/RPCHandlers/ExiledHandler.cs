using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class ExiledHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.Exiled;
}
