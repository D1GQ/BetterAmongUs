using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class CloseMeetingHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.CloseMeeting;
}
