using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;

namespace BetterAmongUs.Modules.Support;

/// <summary>
/// Provides a system for modded plugins to interact with BetterAmongUs through reflection-based events.
/// Allows other plugins to define event handlers without direct assembly references.
/// </summary>
public static class BAUModdedSupportEvents
{
    /// <summary>
    /// Event triggered when BetterAmongUs is loading.
    /// </summary>
    internal static readonly BAUSupportEvent<bool?, BasePlugin> OnBAULoadEvent = new("bau:event.bau_load");

    /// <summary>
    /// Event triggered when BetterAmongUs game options have been loaded.
    /// </summary>
    internal static readonly BAUSupportEvent<object?, object[]> OnBAUOptionsLoadedEvent = new("bau:event.options_load");

    /// <summary>
    /// Event triggered when BetterAmongUs configuration entries have been loaded.
    /// </summary>
    internal static readonly BAUSupportEvent<object?, ConfigEntryBase[]> OnBAUConfigEntriesLoadedEvent = new("bau:event.configs_load");
}