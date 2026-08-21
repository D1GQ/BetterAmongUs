using Semver;
using System.Reflection;

namespace BetterAmongUs;

internal partial class BAUPlugin
{
    /// <summary>
    /// Contains metadata and constants for the BetterAmongUs mod.
    /// </summary>
    internal static class ModInfo
    {
        /// <summary>
        /// The base version number of the mod in MAJOR.MINOR.PATCH format.
        /// </summary>
        internal const string VERSION_NUMBER = "1.3.4";

        /// <summary>
        /// The beta release number. Increment this for each beta release.
        /// </summary>
        internal const string BETA_NUMBER = "0";

        /// <summary>
        /// Gets the full version string for the current build configuration.
        /// </summary>
#if RELEASE
        internal const string VERSION = VERSION_NUMBER;
#elif BETA
        internal const string VERSION = $"{VERSION_NUMBER}-beta-{BETA_NUMBER}";
#endif

        /// <summary>
        /// Gets the full version string with a v prefix for the current build configuration.
        /// </summary>
        internal const string VERSION_STRING = "v" + VERSION;

        /// <summary>
        /// Gets the parsed semantic version of the mod.
        /// </summary>
        internal static readonly SemVersion SemVersion = SemVersion.Parse(VERSION);

        /// <summary>
        /// Gets the Git commit hash from assembly metadata.
        /// </summary>
        public static string CommitHash = ThisAssembly.Git.Commit;

        /// <summary>
        /// Gets the build date from assembly metadata.
        /// </summary>
        public static string BuildDate = ThisAssembly.Metadata.BuildDate;

        /// <summary>
        /// The name of BAU.
        /// </summary>
        internal const string PLUGIN_NAME = "BetterAmongUs";

        /// <summary>
        /// The GUID (Globally Unique Identifier) of BAU.
        /// </summary>
        internal const string PLUGIN_GUID = "com.d1gq.betteramongus";

        /// <summary>
        /// Gets the list of supported Among Us versions.
        /// </summary>
        internal static string[] SupportedAmongUsVersions =
        [
            "2026.8.18"
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
    }
}