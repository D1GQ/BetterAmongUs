using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class RejectShapeshiftHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.RejectShapeshift;
}
