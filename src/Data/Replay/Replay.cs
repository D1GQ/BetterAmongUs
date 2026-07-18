using BetterAmongUs.Interfaces;
using System.Text.Json.Serialization;

namespace BetterAmongUs.Data.Replay;

[Serializable]
internal class Replay
{
    [JsonPropertyName("mapId")]
    public int MapId;

    [JsonPropertyName("playerData")]
    public List<PlayerReplayData> PlayerData = [];

    [JsonPropertyName("events")]
    public Dictionary<float, List<IReplayEvent>> Events = [];

    internal void Load()
    {
        AmongUsClient.Instance.TutorialMapId = MapId;
        UnityEngine.Object.FindFirstObjectByType<FreeplayPopover>().hostGameButton.OnClick();
        CreatePlayers();
    }

    private void CreatePlayers()
    {
        // TODO: Implement player creation logic
    }

    internal void UpdateMovement(float timeStamp)
    {
        // TODO: Implement movement update logic
    }

    internal void UpdateEvents(float timeStamp)
    {
        // TODO: Implement event update logic
    }
}