using AmongUs.InnerNet.GameDataMessages;
using BetterAmongUs.Attributes;
using BetterAmongUs.Interfaces;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat.Handlers.GameDataHandlers;

[RegisterGameDataHandler(GameDataTypes.DespawnFlag)]
internal sealed class DespawnDataHandler : IGameDataHandler
{
    public void Handle(MessageReader reader)
    {
        NetId netId = reader.ReadPackedUInt32();
        if (AmongUsClient.Instance.allObjects.AllObjectsFast.TryGetValue(netId, out var innerNetObject))
        {
            if (innerNetObject is PlayerControl player)
            {
                // BetterNotificationManager.NotifyCheat(player, "Attempting to despawn player", forceBan: true);
            }
        }
    }
}
