using BetterAmongUs.Interfaces;
using BetterAmongUs.Utilities;
using System.Text.Json.Serialization;

namespace BetterAmongUs.Data.Replay.Events;

[Serializable]
internal sealed class MurderReplayEvent : IReplayEvent<MurderReplayEvent.MurderReplayData, MurderReplayEvent.MurderReplayArgs>
{
    [JsonPropertyName("id")]
    public string Id => "player_murder";

    [JsonPropertyName("eventData")]
    public MurderReplayData? EventData { get; set; }

    public void Play()
    {
        var killer = Utils.PlayerFromPlayerId(EventData.KillerId);
        if (killer == null)
            return;

        var target = Utils.PlayerFromPlayerId(EventData.TargetId);
        if (target == null)
            return;

        killer.MurderPlayer(target, MurderResultFlags.Succeeded);
    }

    public void Undo()
    {
    }

    public void Record(MurderReplayArgs murderReplayArgs)
    {
        EventData = new MurderReplayData(murderReplayArgs.Killer.PlayerId, murderReplayArgs.Target.PlayerId);
    }

    internal record MurderReplayData(int KillerId, int TargetId) : IReplayEvent.Data;

    internal record MurderReplayArgs(PlayerControl Killer, PlayerControl Target) : IReplayEvent.Args;
}
