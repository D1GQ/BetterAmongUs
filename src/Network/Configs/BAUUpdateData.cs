using BetterAmongUs.Modules;
using Semver;
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
    [JsonPropertyName("valid")]
    public bool Valid { get; set; } = false;

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
    /// Determines if this update is newer than the currently installed version.
    /// </summary>
    /// <returns>True if the update is newer, false otherwise.</returns>
    internal bool IsNewUpdate()
    {
        try
        {
            if (!Valid)
            {
                return false;
            }

            var updateVersion = SemVersion.Parse(Version);
            var modVersion = BAUPlugin.ModInfo.SemVersion;

            return updateVersion.ComparePrecedenceTo(modVersion) > 0;
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
        return $"{SemVersion.Parse(Version)}";
    }
}