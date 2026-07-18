using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class UseZiplineHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.UseZipline;
}
