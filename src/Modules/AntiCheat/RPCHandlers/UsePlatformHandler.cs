using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class UsePlatformHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.UsePlatform;
}
