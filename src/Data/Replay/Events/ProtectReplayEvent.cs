using AmongUs.GameOptions;
using BetterAmongUs.Interfaces;
using BetterAmongUs.Utilities;
using System.Text.Json.Serialization;

namespace BetterAmongUs.Data.Replay.Events;

[Serializable]
internal sealed class ProtectReplayEvent : IReplayEvent<ProtectReplayEvent.ProtectReplayData, ProtectReplayEvent.ProtectReplayArgs>
{
    [JsonPropertyName("id")]
    public string Id => "protect_player";

    [JsonPropertyName("eventData")]
    public ProtectReplayData? EventData { get; set; }

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

        if (player.Data.RoleType != RoleTypes.GuardianAngel)
            return;

        player.ProtectPlayer(target, player.Data.DefaultOutfit.ColorId);
    }

    public void Undo()
    {
    }

    public void Record(ProtectReplayArgs args)
    {
        EventData = new ProtectReplayData(args.Player.PlayerId, args.Target.PlayerId);
    }

    internal record ProtectReplayData(int PlayerId, int TargetId) : IReplayEvent.Data;

    internal record ProtectReplayArgs(PlayerControl Player, PlayerControl Target) : IReplayEvent.Args;
}