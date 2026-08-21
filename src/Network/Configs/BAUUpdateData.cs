using BetterAmongUs.Enums;
using BetterAmongUs.Modules;
using System.Collections;
using System.Text.Json.Serialization;
using UnityEngine;

namespace BetterAmongUs.Network.Configs;

/// <summary>
/// Represents update data retrieved from the remote repository.
/// </summary>
[Serializable]
internal sealed class BAUUpdateData
{
    /// <summary>
    /// Gets or sets the download link for the updated DLL file.
    /// </summary>
    [JsonPropertyName("dllLink")]
    public string DllLink { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version string of the update.
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the release type as an integer value.
    /// </summary>
    [JsonPropertyName("releaseType")]
    public int ReleaseType { get; set; }

    /// <summary>
    /// Gets or sets whether this update is a hotfix.
    /// </summary>
    [JsonPropertyName("isHotfix")]
    public bool IsHotfix { get; set; }

    /// <summary>
    /// Gets or sets the hotfix number if this is a hotfix update.
    /// </summary>
    [JsonPropertyName("hotfixNumber")]
    public int HotfixNumber { get; set; }

    /// <summary>
    /// Gets or sets the beta number if this is a beta release.
    /// </summary>
    [JsonPropertyName("betaNumber")]
    public int BetaNumber { get; set; }

    /// <summary>
    /// Determines if this update is newer than the currently installed version.
    /// </summary>
    /// <returns>True if the update is newer, false otherwise.</returns>
    /// <remarks>
    /// Compares version numbers, release types, and specific build numbers
    /// to determine if an update should be applied.
    /// </remarks>
    internal bool IsNewUpdate()
    {
        try
        {
            var updateVersion = new Version(Version);
            var modVersion = new Version(BAUPlugin.ModInfo.PLUGIN_VERSION);

            // 1. Compare main version (major.minor.build)
            if (updateVersion > modVersion)
            {
                return true;
            }
            else if (updateVersion < modVersion)
            {
                return false;
            }

            // 2. Versions are equal, compare release types
            var updateReleaseType = (ReleaseTypes)ReleaseType;
            var currentReleaseType = BAUPlugin.ModInfo.ReleaseBuildType;

            // Release is always preferred over Beta
            if (updateReleaseType == ReleaseTypes.Release && currentReleaseType == ReleaseTypes.Beta)
                return true;

            // Don't downgrade from Release to Beta
            if (updateReleaseType == ReleaseTypes.Beta && currentReleaseType == ReleaseTypes.Release)
                return false;

            // 3. Same version and release type, compare specific numbers
            if (updateReleaseType == ReleaseTypes.Beta)
            {
                return BetaNumber > int.Parse(BAUPlugin.ModInfo.BETA_NUM);
            }

            // Release version - check hotfixes
            if (IsHotfix && !BAUPlugin.ModInfo.IS_HOTFIX)
                return true;

            if (IsHotfix && BAUPlugin.ModInfo.IS_HOTFIX)
                return HotfixNumber > int.Parse(BAUPlugin.ModInfo.HOTFIX_NUM);

            // Same version, same type, no newer hotfix
            return false;
        }
        catch (Exception ex)
        {
            Logger_.Error($"Update check failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Downloads and applies the update.
    /// </summary>
    /// <returns>IEnumerator for coroutine execution.</returns>
    /// <remarks>
    /// Downloads the new DLL file, renames the current DLL to .old,
    /// and replaces it with the downloaded file.
    /// </remarks>
    internal IEnumerator CoDownload()
    {
        int count = 0;
        float delay = 0;
        while (!GithubAPI.IsInternetAvailable())
        {
            count++;
            if (count >= 17)
            {
                yield break;
            }
            if (delay < 30f) delay += 2.5f;
            yield return new WaitForSeconds(delay);
        }

        object waiting = true;
        var dllPath = BAUPlugin.ModInfo.Assembly.Location;
        yield return GitHubFile.CoDownloadFile(DllLink, dllPath + ".temp", path =>
        {
            File.Move(dllPath, dllPath + ".old");
            File.Move(path, dllPath);
            waiting = false;
        }, true);

        while (waiting is true)
        {
            yield return null;
        }
    }

    /// <summary>
    /// Returns a string representation of the update data.
    /// </summary>
    /// <returns>A formatted string containing version information.</returns>
    public override string ToString()
    {
        return $"{Version}:{ReleaseType}:{BetaNumber}:{HotfixNumber}";
    }
}