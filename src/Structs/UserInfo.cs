using BetterAmongUs.Utilities;
using System.Text.Json.Serialization;

namespace BetterAmongUs.Structs;

/// <summary>
/// Represents user information for player identification and tracking.
/// </summary>
/// <param name="playerName">The player's name.</param>
/// <param name="hashPuid">The hashed PUID (Player User ID) of the player.</param>
/// <param name="friendCode">The player's friend code.</param>
/// <param name="reason">The reason for tracking this user.</param>
[method: JsonConstructor]
internal sealed class UserInfo(string playerName, string hashPuid, string friendCode, string reason)
{
    /// <summary>
    /// Checks if the player data matches this user information.
    /// </summary>
    /// <param name="data">The player data to check.</param>
    /// <returns>True if the player data matches; otherwise, false.</returns>
    internal bool CheckPlayerData(NetworkedPlayerInfo data) => CheckPlayerDataWithReason(data).check;

    /// <summary>
    /// Checks if the player data matches this user information and returns the reason.
    /// </summary>
    /// <param name="data">The player data to check.</param>
    /// <returns>A tuple containing the check result and the reason if matched.</returns>
    /// <remarks>
    /// Matches by hashed PUID, friend code, or player name.
    /// </remarks>
    internal (bool check, string reason) CheckPlayerDataWithReason(NetworkedPlayerInfo data)
    {
        if (!string.IsNullOrEmpty(data.GetHashPuid()) && HashPuid == data.GetHashPuid()
            || !string.IsNullOrEmpty(data.FriendCode) && FriendCode == data.FriendCode)
        {
            return (true, Reason);
        }
        else if (!string.IsNullOrEmpty(data.PlayerName) && PlayerName == data.PlayerName)
        {
            return (true, Reason);
        }

        return (false, string.Empty);
    }

    /// <summary>
    /// Gets or sets the player's name.
    /// </summary>
    [JsonPropertyName("playerName")]
    public string PlayerName { get; set; } = playerName;

    /// <summary>
    /// Gets or sets the hashed PUID (Player User ID) of the player.
    /// </summary>
    [JsonPropertyName("hashPuid")]
    public string HashPuid { get; set; } = hashPuid;

    /// <summary>
    /// Gets or sets the player's friend code.
    /// </summary>
    [JsonPropertyName("friendCode")]
    public string FriendCode { get; set; } = friendCode;

    /// <summary>
    /// Gets or sets the reason for tracking this user.
    /// </summary>
    [JsonPropertyName("reason")]
    public string Reason { get; set; } = reason;
}