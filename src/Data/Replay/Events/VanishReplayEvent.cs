using AmongUs.GameOptions;
using BetterAmongUs.Interfaces;
using BetterAmongUs.Utilities;
using System.Text.Json.Serialization;

namespace BetterAmongUs.Data.Replay.Events;

[Serializable]
internal sealed class VanishReplayEvent : IReplayEvent<VanishReplayEvent.VanishReplayData, VanishReplayEvent.VanishReplayArgs>
{
    [JsonPropertyName("id")]
    public string Id => "player_vanish";

    [JsonPropertyName("eventData")]
    public VanishReplayData? EventData { get; set; }

    public void Play()
    {
        if (EventData == null)
            return;

        var player = Utils.PlayerFromPlayerId(EventData.PlayerId);
        if (player == null)
            return;

        if (player.Data.RoleType != RoleTypes.Phantom)
            return;

        player.SetRoleInvisibility(true, true, true);
    }

    public void Undo()
    {
    }

    public void Record(VanishReplayArgs args)
    {
        EventData = new VanishReplayData(args.Player.PlayerId);
    }

    internal record VanishReplayData(int PlayerId) : IReplayEvent.Data;

    internal record VanishReplayArgs(PlayerControl Player) : IReplayEvent.Args;
}