using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class StartMeetingHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.StartMeeting;
}
