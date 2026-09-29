using AmongUs.InnerNet.GameDataMessages;
using BetterAmongUs.Attributes;
using BetterAmongUs.Interfaces;
using BetterAmongUs.MonoScripts.Extended;
using BetterAmongUs.Patches.Gameplay.UI.Settings;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat.Handlers.GameDataHandlers;

[RegisterGameDataHandler(GameDataTypes.RpcFlag)]
internal sealed class RpcDataHandler : IGameDataHandler
{
    public void Handle(MessageReader reader)
    {
        NetId netId = reader.ReadPackedUInt32();
        RpcCalls rpcCall = (RpcCalls)reader.ReadByte();

        if (AmongUsClient.Instance.allObjects.AllObjectsFast.TryGetValue(netId, out var innerNetObject))
        {
            if (innerNetObject is PlayerControl player)
            {
                HandleRpcFromPlayer(player, rpcCall, reader);
            }
            else if (innerNetObject is PlayerPhysics physics)
            {
                HandleRpcFromPlayer(physics.myPlayer, rpcCall, reader);
            }
            else if (innerNetObject is CustomNetworkTransform networkTransform)
            {
                HandleRpcFromPlayer(networkTransform.myPlayer, rpcCall, reader);
            }
        }
    }

    private static void HandleRpcFromPlayer(PlayerControl player, RpcCalls rpcCall, MessageReader reader)
    {
        if (player.ExtendedData() != null && BetterGameSettings.RpcRateLimiting.GetBool())
        {
            player.ExtendedData().AntiCheatInfo.RPCSentPS++;
        }
    }
}
