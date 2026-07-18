using BetterAmongUs.Interfaces;
using BetterAmongUs.Utilities;
using System.Text.Json.Serialization;

namespace BetterAmongUs.Data.Replay.Events;

[Serializable]
internal sealed class UpdateSystemReplayEvent : IReplayEvent<UpdateSystemReplayEvent.UpdateSystemReplayData, UpdateSystemReplayEvent.UpdateSystemReplayArgs>
{
    [JsonPropertyName("id")]
    public string Id => "update_system";

    [JsonPropertyName("eventData")]
    public UpdateSystemReplayData? EventData { get; set; }

    public void Play()
    {
        if (EventData == null)
            return;

        var player = Utils.PlayerFromPlayerId(EventData.PlayerId);
        if (player == null)
            return;

        if (ShipStatus.Instance == null)
            return;

        ShipStatus.Instance.UpdateSystem((SystemTypes)EventData.SystemType, player, EventData.Amount);
    }

    public void Undo()
    {
    }

    public void Record(UpdateSystemReplayArgs args)
    {
        EventData = new UpdateSystemReplayData(checked((byte)args.System), args.Player.PlayerId, args.Amount);
    }

    internal record UpdateSystemReplayData(byte SystemType, int PlayerId, byte Amount) : IReplayEvent.Data;

    internal record UpdateSystemReplayArgs(SystemTypes System, PlayerControl Player, byte Amount) : IReplayEvent.Args;
}