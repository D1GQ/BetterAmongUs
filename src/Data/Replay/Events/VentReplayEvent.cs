using BetterAmongUs.Interfaces;
using BetterAmongUs.Utilities;
using System.Text.Json.Serialization;

namespace BetterAmongUs.Data.Replay.Events;

[Serializable]
internal sealed class VentReplayEvent : IReplayEvent<VentReplayEvent.VentReplayData, VentReplayEvent.VentReplayArgs>
{
    [JsonPropertyName("id")]
    public string Id => "player_vent";

    [JsonPropertyName("eventData")]
    public VentReplayData? EventData { get; set; }

    public void Play()
    {
        if (EventData == null)
            return;

        var player = Utils.PlayerFromPlayerId(EventData.PlayerId);
        if (player == null)
            return;

        var vent = ShipStatus.Instance.AllVents.FirstOrDefault(v => v.Id == EventData.VentId);
        if (vent == null)
            return;

        if (EventData.Exit)
        {
            vent.ExitVent(player);
        }
        else
        {
            vent.EnterVent(player);
        }
    }

    public void Undo()
    {
    }

    public void Record(VentReplayArgs args)
    {
        EventData = new VentReplayData(args.Player.PlayerId, args.Exit, args.VentId);
    }

    internal record VentReplayData(int PlayerId, bool Exit, int VentId) : IReplayEvent.Data;

    internal record VentReplayArgs(PlayerControl Player, bool Exit, int VentId) : IReplayEvent.Args;
}