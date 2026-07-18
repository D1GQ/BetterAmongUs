using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers.Deprecated;

[RegisterRPCHandler]
internal sealed class SetSkinHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SetSkin_Deprecated;
}
