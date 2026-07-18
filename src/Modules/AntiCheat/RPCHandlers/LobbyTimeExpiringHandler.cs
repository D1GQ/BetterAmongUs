using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class LobbyTimeExpiringHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.LobbyTimeExpiring;
}
