using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class SyncSettingsHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SyncSettings;
}
