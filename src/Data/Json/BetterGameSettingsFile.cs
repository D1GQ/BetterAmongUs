using BetterAmongUs.Data.Config;
using BetterAmongUs.Modules;
using BetterAmongUs.Modules.OptionItems;
using BetterAmongUs.Patches.Gameplay.UI.Settings;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BetterAmongUs.Data.Json;

/// <summary>
/// Represents a compressed JSON file for storing game settings with GZIP compression.
/// </summary>
internal sealed class BetterGameSettingsFile : AbstractJsonFile
{
    /// <summary>
    /// Gets or initializes the custom file path override for the settings file.
    /// </summary>
    internal string? OverrideFilePath { get; init; }

    /// <summary>
    /// The current version identifier for the settings file format.
    /// </summary>
    internal const string SETTINGS_VERSION = "2.0";

    /// <summary>
    /// The dictionary key used to store the settings file version.
    /// </summary>
    internal const string SETTINGS_VERSION_KEY = "FileVer";

    /// <summary>
    /// Gets the file path for the game settings file.
    /// </summary>
    internal override string FilePath => OverrideFilePath ?? BetterDataManager.Files.SettingsFilePath;

    /// <summary>
    /// Loads the settings file and converts JSON elements to their appropriate types.
    /// </summary>
    /// <returns>True if loading was successful, false otherwise.</returns>
    protected override bool Load()
    {
        var success = base.Load();
        if (success)
        {
            foreach (var kvp in Settings.ToArray())
            {
                if (kvp.Value is JsonElement jsonElement)
                {
                    try
                    {
                        Settings[kvp.Key] = jsonElement.ValueKind switch
                        {
                            JsonValueKind.Number when jsonElement.TryGetInt32(out int intValue) => intValue,
                            JsonValueKind.Number when jsonElement.TryGetSingle(out float floatValue) => floatValue,
                            JsonValueKind.Number when jsonElement.TryGetByte(out byte byteValue) => byteValue,
                            JsonValueKind.True => true,
                            JsonValueKind.False => false,
                            JsonValueKind.String => jsonElement.GetString(),
                            _ => throw new NotSupportedException($"Unsupported JSON type: {jsonElement.ValueKind}")
                        };
                    }
                    catch (Exception ex)
                    {
                        Logger_.Error($"Failed to convert JSON element for key {kvp.Key}: {ex.Message}");
                    }
                }
            }

            // Validate settings file version
            object? versionObject = null;
            Settings.TryGetValue(SETTINGS_VERSION_KEY, out versionObject);
            if (versionObject == null || versionObject is not SETTINGS_VERSION)
            {
                Settings.Clear();
                Save();
                foreach (var opt in OptionItem.AllOptions)
                {
                    opt.TryLoad(true);
                }
                GameSettingsPatch.BetterSettingsTab?.UpdateVisuals();
                return false;
            }
        }
        return success;
    }

    /// <summary>
    /// Saves the current settings to the file, ensuring the version key is included.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the save operation was successful;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    internal override bool Save()
    {
        Settings[SETTINGS_VERSION_KEY] = SETTINGS_VERSION;
        Settings = Settings.OrderBy(kvp => kvp.Key != SETTINGS_VERSION_KEY)
                           .ThenBy(kvp => kvp.Key)
                           .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        return base.Save();
    }

    /// <summary>
    /// Writes JSON data to the file with GZIP compression and base64 encoding.
    /// </summary>
    /// <param name="json">The JSON string to write to the file.</param>
    protected override void WriteToFile(string json)
    {
        if (!BAUConfigs.CompressSettingFiles.Value)
        {
            base.WriteToFile(json);
            return;
        }

        var jsonDoc = JsonDocument.Parse(json);
        var settingsDict = jsonDoc.RootElement.GetProperty(nameof(Settings));
        var sb = new StringBuilder();

        foreach (var kvp in settingsDict.EnumerateObject())
        {
            if (sb.Length > 0) sb.Append('|');
            sb.Append(kvp.Name).Append('/');

            // Check if the value is a float and format it with a decimal point
            var valueKind = kvp.Value.ValueKind;
            if (valueKind == JsonValueKind.Number)
            {
                if (kvp.Value.TryGetSingle(out float floatValue))
                {
                    if (Settings.TryGetValue(kvp.Name, out var originalValue) && originalValue is float)
                    {
                        sb.Append(floatValue.ToString("0.0########", System.Globalization.CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        sb.Append(kvp.Value.GetRawText());
                    }
                }
                else
                {
                    sb.Append(kvp.Value.GetRawText());
                }
            }
            else
            {
                sb.Append(kvp.Value.GetRawText());
            }
        }

        byte[] flattenedData = Encoding.UTF8.GetBytes(sb.ToString());
        using var ms = new MemoryStream();
        using (var gzip = new GZipStream(ms, CompressionMode.Compress, true))
        {
            gzip.Write(flattenedData, 0, flattenedData.Length);
        }
        ms.Position = 0;
        File.WriteAllText(FilePath, Convert.ToBase64String(ms.ToArray()));
    }

    /// <summary>
    /// Reads compressed JSON data from the file and decompresses it.
    /// </summary>
    /// <returns>The decompressed JSON string.</returns>
    protected override string ReadFromFile()
    {
        string text = File.ReadAllText(FilePath).Trim();

        byte[] compressedBytes = new byte[text.Length];
        if (!Convert.TryFromBase64String(text, compressedBytes, out var bytesWritten))
        {
            return text; // Not valid base64, return original json
        }
        Array.Resize(ref compressedBytes, bytesWritten);

        using var ms = new MemoryStream(compressedBytes);
        using var gzip = new GZipStream(ms, CompressionMode.Decompress);
        using var reader = new StreamReader(gzip);
        string flattenedData = reader.ReadToEnd();

        var settingsDict = new Dictionary<string, JsonElement>();
        foreach (var pair in flattenedData.Split('|', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('/', 2);
            if (parts.Length == 2)
            {
                using var doc = JsonDocument.Parse(parts[1]);
                settingsDict[parts[0]] = doc.RootElement.Clone();
            }
        }

        var resultDict = new Dictionary<string, object?>();
        foreach (var kvp in settingsDict)
        {
            resultDict[kvp.Key] = kvp.Value.Deserialize<object?>();
        }

        return JsonSerializer.Serialize(new { Settings = resultDict });
    }

    /// <summary>
    /// Gets the dictionary of game settings with integer keys and various value types.
    /// </summary>
    [JsonPropertyName(nameof(Settings))]
    public Dictionary<string, object?> Settings { get; set; } = [];
}