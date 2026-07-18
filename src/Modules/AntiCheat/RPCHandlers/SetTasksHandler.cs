using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class SetTasksHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SetTasks;
}
