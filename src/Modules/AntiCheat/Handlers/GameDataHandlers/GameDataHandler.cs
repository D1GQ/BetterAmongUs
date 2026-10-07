using AmongUs.InnerNet.GameDataMessages;
using BetterAmongUs.Attributes;
using BetterAmongUs.Interfaces;
using BetterAmongUs.Managers;
using BetterAmongUs.Patches.Gameplay.UI;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat.Handlers.GameDataHandlers;

[RegisterGameDataHandler(GameDataTypes.DataFlag)]
internal sealed class GameDataHandler : IGameDataHandler
{
    public void Handle(MessageReader reader)
    {
        NetId netId = reader.ReadPackedUInt32();
        if (AmongUsClient.Instance.allObjects.AllObjectsFast.TryGetValue(netId, out var innerNetObject))
        {
            if (innerNetObject.TryCast<CustomNetworkTransform>() && (GameState.IsMeeting && MeetingHudPatch.timeOpen > 5))
            {
                var player = innerNetObject.Cast<CustomNetworkTransform>()?.myPlayer;
                if (player == null)
                    return;

                if (BetterNotificationManager.NotifyCheat(player, "Attempting to move in meeting", forceBan: true))
                {
                    // LogRpcInfo($"Player attempted to move during meeting", player);
                }
            }
        }
    }
}
