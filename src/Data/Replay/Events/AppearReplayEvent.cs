using AmongUs.GameOptions;
using BetterAmongUs.Interfaces;
using BetterAmongUs.Utilities;
using System.Text.Json.Serialization;

namespace BetterAmongUs.Data.Replay.Events;

[Serializable]
internal sealed class AppearReplayEvent : IReplayEvent<AppearReplayEvent.AppearReplayData, AppearReplayEvent.AppearReplayArgs>
{
    [JsonPropertyName("id")]
    public string Id => "player_appear";

    [JsonPropertyName("eventData")]
    public AppearReplayData? EventData { get; set; }

    public void Play()
    {
        if (EventData == null)
            return;

        var player = Utils.PlayerFromPlayerId(EventData.PlayerId);
        if (player == null)
            return;

        if (player.Data.RoleType != RoleTypes.Phantom)
            return;

        player.SetRoleInvisibility(false, EventData.Animate, true);
    }

    public void Undo()
    {
    }

    public void Record(AppearReplayArgs args)
    {
        EventData = new AppearReplayData(args.Player.PlayerId, args.Animate);
    }

    internal record AppearReplayData(int PlayerId, bool Animate) : IReplayEvent.Data;

    internal record AppearReplayArgs(PlayerControl Player, bool Animate) : IReplayEvent.Args;
}