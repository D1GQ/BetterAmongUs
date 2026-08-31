using BetterAmongUs.Data.Config;
using BetterAmongUs.Data.Json;
using BetterAmongUs.Utilities;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BetterAmongUs.Data;

/// <summary>
/// Manages data storage, settings, and ban lists for the BetterAmongUs mod.
/// </summary>
internal static class BetterDataManager
{
    /// <summary>
    /// Contains all directory paths used for storing data,
    /// </summary>
    internal static class Folders
    {
        /// <summary>
        /// Root directory for BetterAmongUs data (Starlight mode).
        /// </summary>
        internal static readonly string starLightDataFolderPath = Environment.GetEnvironmentVariable("STAR_DATA_PATH") ?? string.Empty;

        /// <summary>
        /// Root directory for BetterAmongUs data.
        /// </summary>
        internal static readonly string fileFolderPath = Path.Combine(GetPathToAmongUs(), $"Better_Data");

        /// <summary>
        /// Directory for audio overrides files.
        /// </summary>
        internal static readonly string audioOverridesFolderPath = Path.Combine(fileFolderPath, $"AudioOverrides");

        /// <summary>
        /// Directory for save information files.
        /// </summary>
        internal static readonly string saveInfoFolderPath = Path.Combine(fileFolderPath, $"SaveInfo");

        /// <summary>
        /// Directory for settings files.
        /// </summary>
        internal static readonly string settingsFolderPath = Path.Combine(fileFolderPath, $"Settings");

        /// <summary>
        /// Directory for game replay files.
        /// </summary>
        internal static readonly string replaysFolderPath = Path.Combine(fileFolderPath, $"Replays");
    }

    /// <summary>
    /// Contains references to all file paths for data.
    /// </summary>
    internal static class Files
    {
        /// <summary>
        /// The main data file containing outfit presets and cheat detection data.
        /// </summary>
        internal static BetterDataFile BetterDataFile = new();

        /// <summary>
        /// The game settings file with compressed storage.
        /// </summary>
        internal static BetterGameSettingsFile BetterGameSettingsFile = new();

        /// <summary>
        /// The log file path.
        /// </summary>
        internal static readonly string logFilePath_Legacy = Path.Combine(Folders.fileFolderPath, "better-log.txt");

        /// <summary>
        /// The previous log file path.
        /// </summary>
        internal static readonly string previousLogFilePath_Legacy = Path.Combine(Folders.fileFolderPath, "better-previous-log.txt");

        /// <summary>
        /// Legacy data file path (BetterData.json).
        /// </summary>
        internal static readonly string dataFilePath_Legacy = Path.Combine(GetPathToAmongUsData(), "BetterData.json");

        /// <summary>
        /// Current data file path (BetterDataV2.json).
        /// </summary>
        internal static readonly string dataFilePath = Path.Combine(GetPathToAmongUsData(), "BetterDataV2.json");

        /// <summary>
        /// Legacy settings file path.
        /// </summary>
        internal static readonly string settingsFilePath_Legacy = Path.Combine(Folders.settingsFolderPath, "Settings.dat");

        /// <summary>
        /// Current compressed settings file path.
        /// </summary>
        internal static string SettingsFilePath => GetSettingsFilePathFromPreset(BAUConfigs.SettingsPreset.Value);

        /// <summary>
        /// File containing banned player identifiers.
        /// </summary>
        internal static readonly string banPlayerListFilePath = Path.Combine(Folders.saveInfoFolderPath, "BanPlayerList.txt");

        /// <summary>
        /// File containing banned player names.
        /// </summary>
        internal static readonly string banNameListFilePath = Path.Combine(Folders.saveInfoFolderPath, "BanNameList.txt");

        /// <summary>
        /// Legacy file containing banned words/patterns.
        /// </summary>
        internal static readonly string banWordListFilePath_Legacy = Path.Combine(Folders.saveInfoFolderPath, "BanWordList.txt");

        /// <summary>
        /// File containing banned chat messages/patterns.
        /// </summary>
        internal static readonly string banChatListFilePath = Path.Combine(Folders.saveInfoFolderPath, "BanChatList.txt");
        
        /// <summary>
        /// File containing whitelisted player identifiers.
        /// </summary>
        internal static readonly string whiteListFilePath = Path.Combine(Folders.saveInfoFolderPath, "WhiteList.txt");
    }

    /// <summary>
    /// Array of file paths that should be checked during initialization.
    /// </summary>
    private static readonly string[] InitPaths =
    [
        Files.banPlayerListFilePath,
        Files.banNameListFilePath,
        Files.banChatListFilePath,
        Files.whiteListFilePath
    ];

    /// <summary>
    /// Gets the game installation path for Among Us.
    /// </summary>
    /// <returns>The game installation path string.</returns>
    internal static string GetPathToAmongUs()
    {
        if (!BAUPlugin.ModInfo.Starlight)
        {
            return Path.GetDirectoryName(Application.dataPath) ?? throw new Exception("Unable to find `Application.dataPath` path!");
        }
        else
        {
            return Folders.starLightDataFolderPath;
        }
    }

    /// <summary>
    /// Gets the file path for the settings file associated with a specific preset number.
    /// </summary>
    /// <param name="preset">The preset number to get the settings file for.</param>
    internal static string GetSettingsFilePathFromPreset(int preset)
    {
        return Path.Combine(Folders.settingsFolderPath, $"Preset-{preset}.json");
    }

    /// <summary>
    /// Gets the persistent data path for Among Us.
    /// </summary>
    /// <returns>The persistent data path string.</returns>
    internal static string GetPathToAmongUsData()
    {
        if (!BAUPlugin.ModInfo.Starlight)
        {
            return Application.persistentDataPath;
        }
        else
        {
            return Folders.starLightDataFolderPath;
        }
    }

    /// <summary>
    /// Initializes the data manager, loading files and ensuring required directories exist.
    /// </summary>
    internal static void Initialize()
    {
        LoadLegacyData();
        Files.BetterDataFile.Init();
        Files.BetterGameSettingsFile.Init();

        foreach (var path in InitPaths)
        {
            if (File.Exists(path)) continue;

            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var writer = File.CreateText(path);
            if (path == Files.banPlayerListFilePath)
            {
                writer.WriteLine(BAUPlugin.Constants.BAN_PLAYER_LIST_CONTENT);
            }
            else if (path == Files.banNameListFilePath)
            {
                writer.WriteLine(BAUPlugin.Constants.BAN_NAME_LIST_CONTENT);
            }
            else if (path == Files.banChatListFilePath)
            {
                writer.WriteLine(BAUPlugin.Constants.BAN_CHAT_LIST_CONTENT);
            }
            else if (path == Files.whiteListFilePath)
            {
                writer.WriteLine(BAUPlugin.Constants.WHITE_LIST_CONTENT);
            }
        }
    }

    /// <summary>
    /// Converts and loads legacy data if it exists.
    /// </summary>
    private static void LoadLegacyData()
    {
        if (File.Exists(Files.logFilePath_Legacy))
        {
            File.Delete(Files.logFilePath_Legacy);
        }

        if (File.Exists(Files.previousLogFilePath_Legacy))
        {
            File.Delete(Files.previousLogFilePath_Legacy);
        }

        if (File.Exists(Files.settingsFilePath_Legacy))
        {
            if (File.Exists(Files.SettingsFilePath))
            {
                File.Delete(Files.settingsFilePath_Legacy);
            }
            else
            {
                File.Move(Files.settingsFilePath_Legacy, Files.SettingsFilePath);
            }
        }

        if (File.Exists(Files.dataFilePath_Legacy))
        {
            if (File.Exists(Files.dataFilePath))
            {
                File.Delete(Files.dataFilePath_Legacy);
            }
            else
            {
                File.Move(Files.dataFilePath_Legacy, Files.dataFilePath);
            }
        }

        HandleMigrationFromWildcardToRegex();
    }

    /// <summary>
    /// Performs data migration from legacy wildcard‑based ban lists to the new regex‑based format.
    /// </summary>
    private static void HandleMigrationFromWildcardToRegex()
    {
        MigrateBanWordList();
        MigrateBanNameList();
    }

    /// <summary>
    /// Migrates the legacy wildcard‑based BanWordList to the new regex-based BanChatList.
    /// </summary>
    private static void MigrateBanWordList()
    {
        if (!File.Exists(Files.banWordListFilePath_Legacy)) return;

        var content = File.ReadLines(Files.banWordListFilePath_Legacy)
            .Where(line =>
                !string.IsNullOrWhiteSpace(line) &&
                !line.StartsWith("//") &&
                !line.StartsWith("#")
            ).ToArray();

        if (content.Any())
        {
            using var writer = File.CreateText(Files.banChatListFilePath);
            writer.WriteLine(BAUPlugin.Constants.BAN_CHAT_LIST_CONTENT);
            writer.WriteLine();
            writer.WriteLine("// Your old wildcard based filters have been converted to regex patterns");
            writer.WriteLine("// They will behave exactly the same as before");
            writer.WriteLine("// You can keep using them without changing anything else");
            writer.WriteLine();
            foreach (var line in content)
            {
                writer.Write("(?i)(?: |^)");
                if (line.StartsWith("**")) writer.Write(".*");
                writer.Write(Regex.Escape(line.Trim('*')));
                if (line.EndsWith("**")) writer.Write(".*");
                writer.WriteLine("(?: |$)");
            }
        }
        File.Delete(Files.banWordListFilePath_Legacy);
    }

    /// <summary>
    /// Migrates the BanNameList from the legacy wildcard format to the current regex‑based format.
    /// </summary>
    private static void MigrateBanNameList()
    {
        if (!File.Exists(Files.banNameListFilePath)) return;

        var lines = File.ReadLines(Files.banNameListFilePath)
            .Where(line => line != "// ")
            .ToArray();

        // Return if file contains new example comment
        if (lines.AnyLineInContentLines(BAUPlugin.Constants.BAN_NAME_LIST_CONTENT))
            return;

        var content = lines.Where(line =>
            !string.IsNullOrWhiteSpace(line) &&
            !line.StartsWith("//") &&
            !line.StartsWith("#")
        ).ToArray();

        if (!content.Any())
        {
            File.Delete(Files.banNameListFilePath);
            return;
        }

        if (!content.Any(s => Regex.IsMatch(s, @"\*\*")) &&
            !lines.AnyLineInContentLines(BAUPlugin.Constants.BAN_NAME_LIST_CONTENT_LEGACY)) return;

        using var writer = File.CreateText(Files.banNameListFilePath);
        writer.WriteLine(BAUPlugin.Constants.BAN_NAME_LIST_CONTENT);
        writer.WriteLine();
        writer.WriteLine("// Your old wildcard based filters have been converted to regex patterns");
        writer.WriteLine("// They will behave exactly the same as before");
        writer.WriteLine("// You can keep using them without changing anything else");
        writer.WriteLine();
        foreach (var line in content)
        {
            writer.Write("(?i)");
            if (!line.StartsWith("**")) writer.Write("^");
            writer.Write(Regex.Escape(line.Trim('*')));
            if (!line.EndsWith("**")) writer.Write("$");
            writer.WriteLine();
        }
    }

    /// <summary>
    /// Determines whether any line from the supplied array matches a line in the specified content string.
    /// </summary>
    /// <param name="lines">An array of strings to search.</param>
    /// <param name="content">A multi‑line string whose individual lines are compared against <paramref name="lines"/>.</param>
    /// <returns><c>true</c> if at least one line from <paramref name="lines"/> equals any line in <paramref name="content"/>, <c>false</c> otherwise.</returns>
    private static bool AnyLineInContentLines(this string[] lines, string content)
    {
        return content.Split(Environment.NewLine).Any(lines.Contains);
    }

    /// <summary>
    /// Saves a setting with the specified identifier to the game settings file.
    /// </summary>
    /// <param name="id">The unique identifier key for the setting.</param>
    /// <param name="input">The value to save. Can be any object type that is JSON-serializable.</param>
    internal static void SaveSetting(string id, object? input)
    {
        id = id.Split(".").Last();
        Files.BetterGameSettingsFile.Settings[id] = input;
        Files.BetterGameSettingsFile.Save();
    }

    /// <summary>
    /// Determines whether a setting exists and can be loaded as the specified type.
    /// </summary>
    /// <typeparam name="T">The target type to check compatibility with.</typeparam>
    /// <param name="id">The unique identifier key for the setting.</param>
    /// <returns>
    /// <see langword="true"/> if the setting exists and the value is of type <typeparamref name="T"/>; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    internal static bool CanLoadSetting<T>(string id)
    {
        id = id.Split(".").Last();
        if (Files.BetterGameSettingsFile.Settings.TryGetValue(id, out var value))
        {
            if (value is T)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Loads a setting with the specified identifier, returning a default value if the setting 
    /// does not exist or cannot be cast to the requested type.
    /// </summary>
    /// <typeparam name="T">The expected type of the setting value.</typeparam>
    /// <param name="id">The unique identifier key for the setting.</param>
    /// <param name="Default">
    /// The default value to return if the setting is not found. This value will also be 
    /// automatically saved to the settings file.
    /// </param>
    /// <returns>
    /// The stored setting value cast to type <typeparamref name="T"/>, or the specified 
    /// <paramref name="Default"/> value if the setting doesn't exist.
    /// </returns>
    internal static T? LoadSetting<T>(string id, T? Default = default)
    {
        id = id.Split(".").Last();
        if (Files.BetterGameSettingsFile.Settings.TryGetValue(id, out var value))
        {
            if (value is T castValue)
            {
                return castValue;
            }
        }

        SaveSetting(id, Default);
        return Default;
    }

    /// <summary>
    /// Adds a player to the ban list by friend code and/or hashed PUID.
    /// </summary>
    /// <param name="friendCode">The player's friend code (optional).</param>
    /// <param name="hashPUID">The player's hashed PUID (optional).</param>
    internal static void AddToBanList(string friendCode = "", string hashPUID = "")
    {
        AddPlayerIdentifierToFile(Files.banPlayerListFilePath, friendCode, hashPUID);
    }
    
    /// <summary>
    /// Adds a player to the whitelist by friend code and/or hashed PUID.
    /// </summary>
    /// <param name="friendCode">The player's friend code (optional).</param>
    /// <param name="hashPUID">The player's hashed PUID (optional).</param>
    internal static void AddToWhiteList(string friendCode = "", string hashPUID = "")
    {
        AddPlayerIdentifierToFile(Files.whiteListFilePath, friendCode, hashPUID);
    }

    /// <summary>
    /// Adds a player to the specified file by friend code and/or hashed PUID.
    /// </summary>
    /// <param name="filePath">The file to which the player identifier should be appended.</param>
    /// <param name="friendCode">The player's friend code (optional).</param>
    /// <param name="hashPUID">The player's hashed PUID (optional).</param>
    private static void AddPlayerIdentifierToFile(string filePath, string friendCode = "", string hashPUID = "")
    {
        if (string.IsNullOrEmpty(friendCode) && string.IsNullOrEmpty(hashPUID))
            return;

        // Create the new string with the separator if both are not empty
        string newText = string.Empty;

        if (!string.IsNullOrEmpty(friendCode))
        {
            newText = friendCode;
        }

        if (!string.IsNullOrEmpty(hashPUID))
        {
            if (!string.IsNullOrEmpty(newText))
            {
                newText += ", ";
            }
            newText += hashPUID.GetHashStr();
        }

        // Check if the file already contains the new entry
        if (!File.Exists(filePath) || !File.ReadLines(filePath).Any(line => line.Equals(newText)))
        {
            // Append the new string to the file if it's not already present
            File.AppendAllText(filePath, Environment.NewLine + newText);
        }
    }

    /// <summary>
    /// Removes a player from all cheat detection lists by identifier.
    /// </summary>
    /// <param name="identifier">The player identifier (name, hashPUID, or friend code).</param>
    /// <returns>True if the player was found and removed, false otherwise.</returns>
    internal static bool RemovePlayer(string identifier)
    {
        identifier = identifier.Replace(' ', '_');
        bool didFind = false;

        foreach (var info in Files.BetterDataFile.CheatData.ToArray())
        {
            if (info.PlayerName.Replace(' ', '_') == identifier || info.HashPuid == identifier || info.FriendCode == identifier)
            {
                Files.BetterDataFile.CheatData.Remove(info);
                didFind = true;
            }
        }
        foreach (var info in Files.BetterDataFile.SickoData.ToArray())
        {
            if (info.PlayerName.Replace(' ', '_') == identifier || info.HashPuid == identifier || info.FriendCode == identifier)
            {
                Files.BetterDataFile.SickoData.Remove(info);
                didFind = true;
            }
        }
        foreach (var info in Files.BetterDataFile.AUMData.ToArray())
        {
            if (info.PlayerName.Replace(' ', '_') == identifier || info.HashPuid == identifier || info.FriendCode == identifier)
            {
                Files.BetterDataFile.AUMData.Remove(info);
                didFind = true;
            }
        }
        foreach (var info in Files.BetterDataFile.KNData.ToArray())
        {
            if (info.PlayerName.Replace(' ', '_') == identifier || info.HashPuid == identifier || info.FriendCode == identifier)
            {
                Files.BetterDataFile.KNData.Remove(info);
                didFind = true;
            }
        }
        foreach (var info in Files.BetterDataFile.MMCData.ToArray())
        {
            if (info.PlayerName.Replace(' ', '_') == identifier || info.HashPuid == identifier || info.FriendCode == identifier)
            {
                Files.BetterDataFile.MMCData.Remove(info);
                didFind = true;
            }
        }

        if (didFind)
        {
            Files.BetterDataFile.Save();
        }

        return didFind;
    }

    /// <summary>
    /// Clears all cheat detection data from all categories.
    /// </summary>
    internal static void ClearCheatData()
    {
        Files.BetterDataFile.CheatData.Clear();
        Files.BetterDataFile.SickoData.Clear();
        Files.BetterDataFile.AUMData.Clear();
        Files.BetterDataFile.KNData.Clear();
        Files.BetterDataFile.MMCData.Clear();
        Files.BetterDataFile.Save();
    }
}