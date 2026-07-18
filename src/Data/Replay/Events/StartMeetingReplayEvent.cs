using BetterAmongUs.Interfaces;
using System.Text.Json.Serialization;

namespace BetterAmongUs.Data.Replay.Events;

[Serializable]
internal sealed class StartMeetingReplayEvent : IReplayEvent<StartMeetingReplayEvent.StartMeetingReplayData, StartMeetingReplayEvent.StartMeetingReplayArgs>
{
    [JsonPropertyName("id")]
    public string Id => "start_meeting";

    [JsonPropertyName("eventData")]
    public StartMeetingReplayData? EventData { get; set; }

    public void Play()
    {
        if (EventData == null)
            return;
    }

    public void Undo()
    {
    }

    public void Record(StartMeetingReplayArgs args)
    {
        EventData = new StartMeetingReplayData(args.Player.PlayerId, args.Target?.PlayerId ?? -1);
    }

    internal record StartMeetingReplayData(int PlayerId, int TargetId) : IReplayEvent.Data;

    internal record StartMeetingReplayArgs(PlayerControl Player, PlayerControl? Target) : IReplayEvent.Args;
}