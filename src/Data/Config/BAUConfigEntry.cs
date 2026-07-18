using BepInEx.Configuration;

namespace BetterAmongUs.Data.Config;

/// <summary>
/// Represents a configuration entry wrapper for Better Among Us.
/// Provides type-safe access to BepInEx configuration entries with implicit conversion support.
/// </summary>
/// <typeparam name="T">The type of the configuration value.</typeparam>
internal sealed class BAUConfigEntry<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BAUConfigEntry{T}"/> class.
    /// </summary>
    /// <param name="category">The configuration category/section name.</param>
    /// <param name="name">The name of the configuration entry.</param>
    /// <param name="defaultValue">The default value for the configuration entry.</param>
    internal BAUConfigEntry(string category, string name, T defaultValue)
    {
        _config = BAUPlugin.Instance.Config.Bind(category, name, defaultValue);
    }

    private readonly ConfigEntry<T> _config;

    /// <summary>
    /// Gets or sets the current value of the configuration entry.
    /// </summary>
    internal T Value
    {
        get
        {
            return _config.Value;
        }
        set
        {
            _config.Value = value;
        }
    }

    /// <summary>
    /// Implicitly converts a <see cref="BAUConfigEntry{T}"/> to a <see cref="ConfigEntry{T}"/>.
    /// </summary>
    /// <param name="entry">The BAU configuration entry to convert.</param>
    /// <returns>The underlying BepInEx configuration entry.</returns>
    public static implicit operator ConfigEntry<T>(BAUConfigEntry<T> entry)
    {
        return entry._config;
    }

    /// <summary>
    /// Implicitly converts a <see cref="BAUConfigEntry{T}"/> to a <see cref="ConfigEntryBase"/>.
    /// </summary>
    /// <param name="entry">The BAU configuration entry to convert.</param>
    /// <returns>The underlying BepInEx configuration entry as a base type.</returns>
    public static implicit operator ConfigEntryBase(BAUConfigEntry<T> entry)
    {
        return entry._config;
    }
}