using AmongUs.InnerNet.GameDataMessages;
using BetterAmongUs.Managers;
using BetterAmongUs.Patches.Gameplay.UI;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat;

internal sealed class DeserializeNetObjectHandler : RPCHandler
{
    internal override byte GameDataTag => (byte)GameDataTypes.DataFlag;

    internal override void HandleGameData(MessageReader reader)
    {
        uint netId = reader.ReadPackedUInt32();
        var innerNetObject = innerNetClient.FindObjectByNetId<InnerNetObject>(netId);
        if (innerNetObject == null)
            return;

        if (innerNetObject.TryCast<CustomNetworkTransform>() && (GameState.IsMeeting && MeetingHudPatch.timeOpen > 5))
        {
            var player = innerNetObject.Cast<CustomNetworkTransform>()?.myPlayer;
            if (player == null)
                return;

            if (BetterNotificationManager.NotifyCheat(player, "Attempting to move in meeting", forceBan: true))
            {
                LogRpcInfo($"Player attempted to move during meeting", player);
            }
        }
    }
}