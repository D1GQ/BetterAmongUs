using BetterAmongUs.Modules;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BetterAmongUs.Data.Json;

/// <summary>
/// Provides a base class for JSON-backed data files with automatic
/// loading, validation, and serialization.
/// </summary>
internal abstract class AbstractJsonFile
{
    /// <summary>
    /// Gets the path to the JSON file.
    /// </summary>
    internal abstract string FilePath { get; }

    private bool _initialized;

    /// <summary>
    /// Gets the <see cref="JsonSerializerOptions"/> used for serialization
    /// and deserialization.
    /// </summary>
    protected virtual JsonSerializerOptions SerializerOptions { get; } = new()
    {
        WriteIndented = true,
        IncludeFields = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Initializes the file by loading existing data if possible and
    /// writing the current state back to disk.
    /// </summary>
    internal virtual void Init()
    {
        if (_initialized)
            return;

        _initialized = true;

        if (IsFileValid())
            Load();

        Save();
    }

    /// <summary>
    /// Loads the file contents into the current instance.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the file was successfully loaded;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected virtual bool Load()
    {
        try
        {
            var json = ReadFromFile();

            if (string.IsNullOrWhiteSpace(json))
                return false;

            var data = JsonSerializer.Deserialize(json, GetType(), SerializerOptions);
            if (data is null)
                return false;

            foreach (var property in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.CanWrite &&
                    property.GetCustomAttribute<JsonPropertyNameAttribute>() != null)
                {
                    property.SetValue(this, property.GetValue(data));
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Logger_.Error(ex);
            return false;
        }
    }

    /// <summary>
    /// Saves the current instance to the JSON file.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the file was successfully written;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    internal virtual bool Save()
    {
        try
        {
            WriteToFile(JsonSerializer.Serialize(this, GetType(), SerializerOptions));
            return true;
        }
        catch (Exception ex)
        {
            Logger_.Error(ex);
            return false;
        }
    }

    /// <summary>
    /// Determines whether the JSON file exists and contains valid,
    /// non-empty JSON.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the file is valid; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    private bool IsFileValid()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);

        if (!File.Exists(FilePath))
            return false;

        try
        {
            var json = ReadFromFile();

            if (string.IsNullOrWhiteSpace(json))
                return false;

            var element = JsonSerializer.Deserialize<JsonElement>(json);

            return element.ValueKind switch
            {
                JsonValueKind.Object => element.EnumerateObject().Any(),
                JsonValueKind.Array => element.EnumerateArray().Any(),
                _ => true
            };
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Reads the contents of the JSON file.
    /// </summary>
    /// <returns>The raw JSON string.</returns>
    protected virtual string ReadFromFile()
    {
        return File.ReadAllText(FilePath);
    }

    /// <summary>
    /// Writes JSON content to the file.
    /// </summary>
    /// <param name="json">The JSON string to write.</param>
    protected virtual void WriteToFile(string json)
    {
        File.WriteAllText(FilePath, json);
    }
}