using BetterAmongUs.Enums;
using System.Reflection;

namespace BetterAmongUs;

/// <summary>
/// Contains metadata and constants for the BetterAmongUs mod.
/// </summary>
internal static class ModInfo
{
    /// <summary>
    /// Gets the release type of the current build.
    /// </summary>
    internal static readonly ReleaseTypes ReleaseBuildType = ReleaseTypes.Release;

    /// <summary>
    /// Gets the Git commit hash from assembly metadata.
    /// </summary>
    public static string CommitHash = ThisAssembly.Git.Commit;

    /// <summary>
    /// Gets the build date from assembly metadata.
    /// </summary>
    public static string BuildDate = ThisAssembly.Metadata.BuildDate;

    /// <summary>
    /// The beta number for beta releases.
    /// </summary>
    internal const string BETA_NUM = "0";

    /// <summary>
    /// The hotfix number for hotfix releases.
    /// </summary>
    internal const string HOTFIX_NUM = "0";

    /// <summary>
    /// Indicates whether this is a hotfix release.
    /// </summary>
    internal const bool IS_HOTFIX = false;

    /// <summary>
    /// The name of BAU.
    /// </summary>
    internal const string PLUGIN_NAME = "BetterAmongUs";

    /// <summary>
    /// The GUID (Globally Unique Identifier) of BAU.
    /// </summary>
    internal const string PLUGIN_GUID = "com.d1gq.betteramongus";

    /// <summary>
    /// The version of BAU.
    /// </summary>
    internal const string PLUGIN_VERSION = "1.3.3";

    /// <summary>
    /// Gets the list of supported Among Us versions.
    /// </summary>
    internal static string[] SupportedAmongUsVersions =
    [
        "2026.6.5",
        "2026.4.7", // Andiroid only update
        "2026.3.31",
        "2026.3.17",
        "2025.11.18"
    ];

    /// <summary>
    /// The GitHub repository URL for BAU.
    /// </summary>
    internal const string GITHUB = ThisAssembly.Git.RepositoryUrl;

    /// <summary>
    /// The Discord invite URL for BAU.
    /// </summary>
    internal const string DISCORD = "https://discord.gg/vjYrXpzNAn";

    /// <summary>
    /// Indicator rather that BAU is running on Starlight for Android.
    /// </summary>
    internal static readonly bool Starlight = OperatingSystem.IsAndroid();

    /// <summary>
    /// Retrieves metadata from the assembly attributes.
    /// </summary>
    /// <param name="key">The metadata key to retrieve.</param>
    /// <returns>The metadata value, or an empty string if not found.</returns>
    private static string GetAssemblyMetadata(string key)
    {
        var attribute = Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => a.Key == key);

        return attribute?.Value ?? string.Empty;
    }

    /// <summary>
    /// The assembly associated to this mod.
    /// </summary>
    internal static Assembly Assembly
    {
        get
        {
            if (field == null)
            {
                field = Assembly.GetExecutingAssembly();
            }
            return field;
        }
    }

    /// <summary>
    /// Contains constants for Among Us.
    /// </summary>
    internal static class AmongUs
    {
        /// <summary>
        /// The process name of the Among Us executable.
        /// </summary>
        internal const string PROCESS_NAME = "Among Us.exe";
    }

    internal static class Constants
    {
        internal const int MAX_CHAT_TEXT = 120;
        internal const string BAU_CUSTOM_RPC_FLAG = "bau:rpc";
        internal const string BAU_MODDED_PROTOCOL_FLAG = "bau:flags";
    }
}