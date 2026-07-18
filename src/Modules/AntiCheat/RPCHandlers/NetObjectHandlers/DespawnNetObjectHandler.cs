using AmongUs.InnerNet.GameDataMessages;
using BetterAmongUs.Managers;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers.NetObjectHandlers;

internal sealed class DespawnNetObjectHandler : RPCHandler
{
    internal override byte GameDataTag => (byte)GameDataTypes.DespawnFlag;

    internal override void HandleGameData(MessageReader reader)
    {
        // if (!GameState.IsHost) return;

        uint netId = reader.ReadPackedUInt32();
        var innerNetObject = innerNetClient.FindObjectByNetId<InnerNetObject>(netId);
        if (innerNetObject is PlayerControl player)
        {
            BetterNotificationManager.NotifyCheat(player, "Attempting to despawn player", forceBan: true);
        }
    }
}