#pragma warning disable CA2211

using BepInEx.Unity.IL2CPP;

namespace BetterAmongUs.Modules.Support;

/// <summary>
/// Provides modded support functionality for BetterAmongUs by allowing other mods to declare flags
/// that control various features and behaviors of BetterAmongUs.
/// </summary>
public static class BAUModdedSupportFlags
{
    // ============================================
    // Client Features
    // ============================================

    /// <summary>
    /// Disables the bau splash intro.
    /// When enabled by another mod, BetterAmongUs will not play the bau splash intro after the original.
    /// </summary>
    public static string Disable_CustomSplashIntro = "client.disable.bausplashintro";

    /// <summary>
    /// Disables the enhanced ping display.
    /// When enabled by another mod, BetterAmongUs will not replace the default ping tracker.
    /// </summary>
    public static string Disable_BetterPingTracker = "client.disable.betterpingtracker";

    /// <summary>
    /// Disables all theming and customization features.
    /// When enabled by another mod, BetterAmongUs will use the default game appearance.
    /// </summary>
    public static string Disable_Theme = "client.disable.theme";

    /// <summary>
    /// Disables the custom mod badge/stamp.
    /// When enabled by another mod, BetterAmongUs will use the default mod indicator.
    /// </summary>
    public static string Disable_CustomModStamp = "client.disable.custommodstamp";

    /// <summary>
    /// Disables Outfit Presets integration.
    /// When enabled by another mod, BetterAmongUs will not add outfit presets to cosmetic menu.
    /// </summary>
    public static string Disable_OutfitPresets = "client.disable.outfitpresets";

    /// <summary>
    /// Disables Favorite Color integration.
    /// When enabled by another mod, BetterAmongUs will not add favorite color to cosmetic menu.
    /// </summary>
    public static string Disable_FavoriteColor = "client.disable.favoritecolor";

    /// <summary>
    /// Disables the custom server region dropdown menu.
    /// When enabled by another mod, BetterAmongUs will use the default server selection interface.
    /// </summary>
    public static string Disable_ServerDropDown = "client.disable.serverdropdown";

    /// <summary>
    /// Disables Discord Rich Presence integration.
    /// When enabled by another mod, BetterAmongUs will not update the Discord status.
    /// </summary>
    public static string Disable_DiscordRP = "client.disable.discordrp";

    /// <summary>
    /// Disables the custom HTTP header that identifies BetterAmongUs clients to matchmaking servers.
    /// When enabled by another mod, BetterAmongUs will use standard HTTP headers.
    /// </summary>
    public static string Disable_BAUHttpHeader = "client.disable.bauhttpheader";

    // ============================================
    // Anti-Cheat System
    // ============================================

    /// <summary>
    /// Completely disables the anti-cheat system.
    /// When enabled by another mod, BetterAmongUs will not perform any anti-cheat checks.
    /// </summary>
    public static string Disable_Anticheat = "anticheat.disable";

    /// <summary>
    /// Prefix for disabling specific RPC handlers or handler flags.
    /// Format: "anticheat.disable.rpchandler=HandlerClassName" to disable an entire handler,
    /// or "anticheat.disable.rpchandler=HandlerClassName:HandlerFlagName" for specific flags.
    /// When enabled by another mod, BetterAmongUs will not validate the specified RPC handlers.
    /// <seealso cref="AntiCheat.RPCHandler"/> for the base handler class.
    /// <seealso cref="Enums.HandlerFlag"/> for available handler flags.
    /// </summary>
    public static string Disable_RPCHandler = "anticheat.disable.rpchandler=";

    // ============================================
    // Command System
    // ============================================

    /// <summary>
    /// Disables the entire command system.
    /// When enabled by another mod, BetterAmongUs will not provide any commands.
    /// </summary>
    public static string Disable_AllCommands = "command.disable.allcommands";

    /// <summary>
    /// Prefix for disabling specific commands.
    /// Format: "command.disable=COMMAND_NAME"
    /// When enabled by another mod, BetterAmongUs will not provide the specified command.
    /// <seealso cref="Commands.BaseCommand"/> for the base command class.
    /// </summary>
    public static string Disable_Command = "command.disable=";

    /// <summary>
    /// Forces all BetterAmongUs commands to use the "bau:" prefix.
    /// When enabled by another mod, BetterAmongUs will require the "bau:" prefix for all commands.
    /// </summary>
    public static string Force_BAU_Command_Prefix = "command.force.bau.prefix";

    // ============================================
    // Game Options & Settings
    // ============================================

    /// <summary>
    /// Disables all custom game option modifications.
    /// When enabled by another mod, BetterAmongUs will use the default game options.
    /// </summary>
    public static string Disable_AllGameOptions = "gameoption.disable.allgameoptions";

    /// <summary>
    /// Prefix for disabling specific game options.
    /// Format: "gameoption.disable=TRANSLATION_NAME"
    /// When enabled by another mod, BetterAmongUs will hide the specified option and use its default value.
    /// <seealso cref="OptionItems.OptionItem"/> for the base option class.
    /// </summary>
    public static string Disable_GameOption = "gameoption.disable=";

    // ============================================
    // Lobby Features
    // ============================================

    /// <summary>
    /// Disables the ability to cancel the game start countdown.
    /// When enabled by another mod, BetterAmongUs will not allow the start game countdown to be cancelled.
    /// </summary>
    public static string Disable_CancelStartingGame = "lobby.disable.cancelstartinggame";

    /// <summary>
    /// Disables the custom loading bar.
    /// When enabled by another mod, BetterAmongUs will use the default loading bar behavior.
    /// </summary>
    public static string Disable_CustomLoadingBar = "lobby.disable.customloadingbar";

    // ============================================
    // Main Menu Features
    // ============================================

    /// <summary>
    /// Disables the mod update notification button.
    /// When enabled by another mod, BetterAmongUs will not display update checks and prompts in the main menu.
    /// </summary>
    public static string Disable_ModUpdate = "mainmenu.disable.modupdate";

    /// <summary>
    /// Disables the BetterAmongUs logo/branding in the main menu.
    /// When enabled by another mod, BetterAmongUs will not display the BAU logo.
    /// </summary>
    public static string Disable_BAULogo = "mainmenu.disable.baulogo";

    // ============================================
    // Gameplay Features
    // ============================================

    /// <summary>
    /// Disables the enhanced role assignment algorithm.
    /// When enabled by another mod, BetterAmongUs will use the default role distribution system.
    /// </summary>
    public static string Disable_BetterRoleAlgorithm = "gameplay.disable.betterrolealgorithm";

    /// <summary>
    /// Disables the detailed end-game summary screen.
    /// When enabled by another mod, BetterAmongUs will not display the detailed end-game summary screen.
    /// </summary>
    public static string Disable_EndGameSummary = "gameplay.disable.endgamesummary";

    /// <summary>
    /// Disables custom icons on the mini map.
    /// When enabled by another mod, BetterAmongUs will not generate custom icons on the mini map.
    /// </summary>
    public static string Disable_MinimapIcons = "gameplay.disable.minimapicons";

    /// <summary>
    /// Disables custom vent color highlights.
    /// When enabled by another mod, BetterAmongUs will not use vent groups for highlighted vent colors.
    /// </summary>
    public static string Disable_VentColorGroups = "gameplay.disable.ventcolorgroups";

    /// <summary>
    /// Disables forcibly overriding player name.
    /// When enabled by another mod, BetterAmongUs will not override the player name.
    /// </summary>
    public static string Disable_NameOverride = "gameplay.disable.nameoverride";

    /// <summary>
    /// Disables forcibly overriding player chat name.
    /// When enabled by another mod, BetterAmongUs will not override the player name in chat.
    /// </summary>
    public static string Disable_ChatNameOverride = "gameplay.disable.chatnameoverride";

    /// <summary>
    /// Disables custom info displayed above player.
    /// When enabled by another mod, BetterAmongUs not display custom info above the player.
    /// </summary>
    public static string Disable_PlayerInfo = "gameplay.disable.playerinfo";

    /// <summary>
    /// Disables custom info displayed in player vote area.
    /// When enabled by another mod, BetterAmongUs not display custom info player vote area.
    /// </summary>
    public static string Disable_PlayerMeetingInfo = "gameplay.disable.playermeetinginfo";

    /// <summary>
    /// Disables custom color blind text.
    /// When enabled by another mod, BetterAmongUs will use the default color blind text.
    /// </summary>
    public static string Disable_CustomColorBlindText = "gameplay.disable.customcolorblindtext";

    private static readonly HashSet<string> _flags = [];
    private static readonly HashSet<int> _tempFlags = [];
    private static bool _initialized = false;

    private static readonly BAUSupportVar<string[]> BAUSupportFlags = new("bau:flags");

    /// <summary>
    /// Initializes the modded support system by collecting flags from all loaded plugins.
    /// </summary>
    internal static void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;
        List<string> flags = [];
        foreach (var pluginInfo in IL2CPPChainloader.Instance.Plugins.Values)
        {
            if (pluginInfo == null)
                continue;

            var modFlags = BAUSupportFlags.GetValue(pluginInfo);
            if (modFlags != null)
            {
                flags.AddRange(modFlags);
            }
        }

        foreach (var flag in flags.ToHashSet())
        {
            AddFlag(flag);
        }
    }

    /// <summary>
    /// Manually adds a flag to the internal flag collection.
    /// </summary>
    /// <param name="flag">The flag string to add to the collection.</param>
    internal static void AddFlag(string flag)
    {
        _flags.Add(flag);
    }

    /// <summary>
    /// Manually adds a temporary flag to the internal temporary flag collection.
    /// </summary>
    /// <param name="flag">The flag hash to add to the collection.</param>
    internal static void AddTempFlag(int flag)
    {
        _tempFlags.Add(flag);
    }

    /// <summary>
    /// CLears all temporary flags.
    /// </summary>
    internal static void ClearTempFlags()
    {
        _tempFlags.Clear();
    }

    /// <summary>
    /// Checks if a specific flag has been declared by any loaded mod.
    /// </summary>
    /// <param name="flag">The flag to check for presence in the collected flags.</param>
    /// <returns><c>true</c> if the flag is present; otherwise, <c>false</c>.</returns>
    public static bool HasFlag(string flag)
    {
        return _flags.Contains(flag) || _tempFlags.Contains(GetFlagHash(flag));
    }

    /// <summary>
    /// Generates a integer hash from a string for use as a flag identifier.
    /// </summary>
    /// <param name="input">The input string to hash. Can be null or empty.</param>
    /// <returns>
    /// A integer hash value. Returns 0 for null or empty strings.
    /// </returns>
    internal static int GetFlagHash(string input)
    {
        if (string.IsNullOrEmpty(input)) return 0;

        int hash = 17;
        hash = hash * 31 + input.Length;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            hash = hash * 31 + c;
            hash = hash * 31 + i;
        }

        hash = hash * 31 + input.Length;
        return hash;
    }
}