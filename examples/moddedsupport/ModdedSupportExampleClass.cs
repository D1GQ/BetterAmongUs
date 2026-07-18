using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using BetterAmongUs.Helpers;
using System.ComponentModel;

namespace BetterAmongUs.Examples;

/// <summary>
/// Example class demonstrating how to create modded support for BetterAmongUs.
/// </summary>
/// <remarks>
/// All methods and fields must be static and decorated with the appropriate <see cref="CategoryAttribute"/>
/// to be discovered by BetterAmongUs through reflection.
/// </remarks>
internal class ModdedSupportExampleClass
{
    /// <summary>
    /// Array of flags to control BetterAmongUs behavior.
    /// Check BAUModdedSupportFlags for available flag constants.
    /// </summary>
    [Category("bau:flags")]
    public static string[] BAUFlags = [];

    /// <summary>
    /// Called when BetterAmongUs is loading. Return false to prevent BAU from loading.
    /// </summary>
    /// <param name="bauPlugin">The BetterAmongUs plugin instance.</param>
    /// <returns>True to allow BAU to load, false to prevent it.</returns>
    [Category("bau:event.bau_load")]
    public static bool OnBAULoad(BasePlugin bauPlugin)
    {
        return true;
    }

    /// <summary>
    /// Called when BetterAmongUs game options have been loaded.
    /// </summary>
    /// <param name="options">
    /// Array of game option objects from BetterAmongUs. Common types include:
    /// OptionItem, OptionItem{T}, OptionCheckboxItem, OptionFloatItem,
    /// OptionIntItem, OptionPercentItem, OptionPlayerItem, OptionStringItem
    /// </param>
    [Category("bau:event.options_load")]
    public static void OnBAUOptionsLoaded(object[] options)
    {
    }

    /// <summary>
    /// Called when BetterAmongUs configuration entries have been loaded.
    /// </summary>
    /// <param name="configs">Array of BepInEx ConfigEntryBase objects from BetterAmongUs.</param>
    [Category("bau:event.configs_load")]
    public static void OnBAUConfigEntriesLoaded(ConfigEntryBase[] configs)
    {
    }
}