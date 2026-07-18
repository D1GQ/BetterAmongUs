using BetterAmongUs.Interfaces;
using BetterAmongUs.Utilities;
using System.Text.Json.Serialization;

namespace BetterAmongUs.Data.Replay.Events;

[Serializable]
internal sealed class ChatEvent : IReplayEvent<ChatEvent.ChatReplayData, ChatEvent.ChatReplayArgs>
{
    [JsonPropertyName("id")]
    public string Id => "chat";

    [JsonPropertyName("eventData")]
    public ChatReplayData? EventData { get; set; }

    public void Play()
    {
        if (EventData == null)
            return;

        var player = Utils.PlayerFromPlayerId(EventData.PlayerId);
        if (player == null)
            return;

        if (HudManager.InstanceExists)
        {
            HudManager.Instance.Chat.AddChat(player, EventData.Message, false);
        }
    }

    public void Undo()
    {
    }

    public void Record(ChatReplayArgs args)
    {
        EventData = new ChatReplayData(args.Player.PlayerId, args.Message);
    }

    internal record ChatReplayData(int PlayerId, string Message) : IReplayEvent.Data;

    internal record ChatReplayArgs(PlayerControl Player, string Message) : IReplayEvent.Args;
}