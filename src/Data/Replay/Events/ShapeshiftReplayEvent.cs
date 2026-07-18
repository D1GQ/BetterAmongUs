using AmongUs.GameOptions;
using BetterAmongUs.Interfaces;
using BetterAmongUs.Utilities;
using System.Text.Json.Serialization;

namespace BetterAmongUs.Data.Replay.Events;

[Serializable]
internal sealed class ShapeshiftReplayEvent : IReplayEvent<ShapeshiftReplayEvent.ShapeshiftReplayData, ShapeshiftReplayEvent.ShapeshiftReplayArgs>
{
    [JsonPropertyName("id")]
    public string Id => "player_shapeshift";

    [JsonPropertyName("eventData")]
    public ShapeshiftReplayData? EventData { get; set; }

    public void Play()
    {
        if (EventData == null)
            return;

        var player = Utils.PlayerFromPlayerId(EventData.PlayerId);
        if (player == null)
            return;

        var target = Utils.PlayerFromPlayerId(EventData.TargetId);
        if (target == null)
            return;

        if (player.Data.RoleType != RoleTypes.Shapeshifter)
            return;

        player.Shapeshift(target, EventData.Animate);
    }

    public void Undo()
    {
    }

    public void Record(ShapeshiftReplayArgs args)
    {
        EventData = new ShapeshiftReplayData(args.Player.PlayerId, args.Target.PlayerId, args.Animate);
    }

    internal record ShapeshiftReplayData(int PlayerId, int TargetId, bool Animate) : IReplayEvent.Data;

    internal record ShapeshiftReplayArgs(PlayerControl Player, PlayerControl Target, bool Animate) : IReplayEvent.Args;
}