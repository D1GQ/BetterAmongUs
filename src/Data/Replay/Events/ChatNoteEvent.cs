using BetterAmongUs.Interfaces;
using BetterAmongUs.Utilities;
using System.Text.Json.Serialization;

namespace BetterAmongUs.Data.Replay.Events;

[Serializable]
internal sealed class ChatNoteEvent : IReplayEvent<ChatNoteEvent.ChatNoteReplayData, ChatNoteEvent.ChatNoteReplayArgs>
{
    [JsonPropertyName("id")]
    public string Id => "chat_note";

    [JsonPropertyName("eventData")]
    public ChatNoteReplayData? EventData { get; set; }

    public void Play()
    {
        if (EventData == null)
            return;

        var player = Utils.PlayerFromPlayerId(EventData.PlayerId);
        if (player == null)
            return;

        if (HudManager.InstanceExists)
        {
            HudManager.Instance.Chat.AddChatNote(player.Data, (ChatNoteTypes)EventData.NoteType);
        }
    }

    public void Undo()
    {
    }

    public void Record(ChatNoteReplayArgs args)
    {
        EventData = new ChatNoteReplayData(args.Player.PlayerId, (int)args.NoteType);
    }

    internal record ChatNoteReplayData(int PlayerId, int NoteType) : IReplayEvent.Data;

    internal record ChatNoteReplayArgs(PlayerControl Player, ChatNoteTypes NoteType) : IReplayEvent.Args;
}