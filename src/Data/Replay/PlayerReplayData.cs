using System.Text.Json.Serialization;
using UnityEngine;

namespace BetterAmongUs.Data.Replay;

[Serializable]
internal class PlayerReplayData
{
    [JsonPropertyName("playerId")]
    public int PlayerId;

    [JsonPropertyName("playerName")]
    public string PlayerName = "";

    [JsonPropertyName("movementData")]
    public List<(float timeStamp, Vector2 pos)> MovementDataBuffer = [];

    [JsonPropertyName("cosmeticData")]
    public (int colorId, string skinId, string visorId, string petId, string namePlateId) CosmeticData = new();

    internal void Set(PlayerControl player)
    {
        PlayerId = player.Data.PlayerId;
        PlayerName = player.Data.PlayerName;
    }

    internal void RecordMovement(Vector2 pos)
    {
        MovementDataBuffer.Add((0, pos));
    }
}