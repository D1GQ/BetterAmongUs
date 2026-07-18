using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class PlayAnimationHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.PlayAnimation;
}
