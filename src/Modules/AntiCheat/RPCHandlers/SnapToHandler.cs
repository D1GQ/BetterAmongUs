using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class SnapToHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SnapTo;
}
