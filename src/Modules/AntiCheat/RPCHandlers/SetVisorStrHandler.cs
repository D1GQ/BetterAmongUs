using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class SetVisorStrHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SetVisorStr;
}
