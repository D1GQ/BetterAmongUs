using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class TriggerSporesHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.TriggerSpores;
}
