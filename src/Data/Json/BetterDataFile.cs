using BetterAmongUs.Generated;
using BetterAmongUs.Structs;
using BetterAmongUs.Utilities;
using System.Text.Json.Serialization;

namespace BetterAmongUs.Data.Json;

/// <summary>
/// Represents the main data file for BetterAmongUs, containing outfit presets and cheat detection data.
/// </summary>
internal sealed class BetterDataFile : AbstractJsonFile
{
    /// <summary>
    /// Gets the file path for the BetterAmongUs data file.
    /// </summary>
    internal override string FilePath => BetterDataManager.Files.dataFilePath;

    /// <summary>
    /// Loads the data file and performs post-load processing.
    /// </summary>
    /// <returns>True if loading was successful, false otherwise.</returns>
    protected override bool Load()
    {
        var success = base.Load();
        if (success)
        {
            SelectedOutfitPreset = Math.Clamp(SelectedOutfitPreset, 0, 5);
            AllCheatData = [.. CheatData, .. SickoData, .. AUMData, .. KNData, .. MMCData];
        }
        return success;
    }

    /// <summary>
    /// Saves the data file, updating the combined cheat data before saving.
    /// </summary>
    /// <returns>True if saving was successful, false otherwise.</returns>
    internal override bool Save()
    {
        AllCheatData = [.. CheatData, .. SickoData, .. AUMData, .. KNData, .. MMCData];
        return base.Save();
    }

    /// <summary>
    /// Attempts to get cheat information for the specified player.
    /// </summary>
    /// <param name="data">The player information to check.</param>
    /// <param name="info">When this method returns, contains the title and hex color if cheat info is found; otherwise, empty strings.</param>
    /// <returns>True if cheat information was found for the player; otherwise, false.</returns>
    internal bool TryGetCheatInfo(NetworkedPlayerInfo data, out (string title, string hexColor) info)
    {
        info = ("", "");

        if (SickoData.Any(info => info.CheckPlayerData(data)))
        {
            info = (TranslationStrings.Player_SickoUser.LocalizedString, Colors.SickoHexColor);
            return true;
        }
        else if (AUMData.Any(info => info.CheckPlayerData(data)))
        {
            info = (TranslationStrings.Player_AUMUser.LocalizedString, Colors.AUMHexColor);
            return true;
        }
        else if (KNData.Any(info => info.CheckPlayerData(data)))
        {
            info = (TranslationStrings.Player_KNUser.LocalizedString, Colors.KNHexColor);
            return true;
        }
        else if (MMCData.Any(info => info.CheckPlayerData(data)))
        {
            info = (TranslationStrings.Player_MMCUser.LocalizedString, Colors.MMCHexColor);
            return true;
        }
        else if (CheatData.Any(info => info.CheckPlayerData(data)))
        {
            info = (TranslationStrings.Player_FlaggedPlayer.LocalizedString, Colors.CheaterHexColor);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if player data matches any known cheat entries.
    /// </summary>
    /// <param name="data">The player information to check.</param>
    /// <returns>True if the player matches a cheat entry, false otherwise.</returns>
    internal bool CheckPlayerData(NetworkedPlayerInfo data) =>
        CheckPlayerDataWithReason(data).check;

    /// <summary>
    /// Checks if player data matches any known cheat entries and provides a reason if found.
    /// </summary>
    /// <param name="data">The player information to check.</param>
    /// <returns>A tuple containing whether a match was found and the reason for the match.</returns>
    internal (bool check, string reason) CheckPlayerDataWithReason(NetworkedPlayerInfo data)
    {
        foreach (var info in AllCheatData)
        {
            var (check, reason) = info.CheckPlayerDataWithReason(data);
            if (check)
            {
                return (true, reason);
            }
        }

        return (false, "");
    }

    /// <summary>
    /// Gets or sets the combined collection of all cheat detection data.
    /// </summary>
    internal HashSet<UserInfo> AllCheatData { get; set; } = [];

    /// <summary>
    /// Gets or sets the index of the currently selected outfit preset.
    /// </summary>
    [JsonPropertyName("selectedOutfitPreset")]
    public int SelectedOutfitPreset { get; set; } = 0;

    /// <summary>
    /// Gets or sets the collection of outfit presets.
    /// </summary>
    [JsonPropertyName("outfitData")]
    public HashSet<OutfitData> OutfitData { get; set; } = [new(), new(), new(), new(), new(), new()];

    /// <summary>
    /// Gets or sets the collection of known cheat user data.
    /// </summary>
    [JsonPropertyName("cheatData")]
    public HashSet<UserInfo> CheatData { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of Sicko cheat user data.
    /// </summary>
    [JsonPropertyName("sickoData")]
    public HashSet<UserInfo> SickoData { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of AUM cheat user data.
    /// </summary>
    [JsonPropertyName("aumData")]
    public HashSet<UserInfo> AUMData { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of Kill Network cheat user data.
    /// </summary>
    [JsonPropertyName("knData")]
    public HashSet<UserInfo> KNData { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of Mod Menu Crew cheat user data.
    /// </summary>
    [JsonPropertyName("mmcData")]
    public HashSet<UserInfo> MMCData { get; set; } = [];
}