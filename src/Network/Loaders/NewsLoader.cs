using BetterAmongUs.Attributes;
using BetterAmongUs.Modules;
using BetterAmongUs.Network.Configs;
using Il2CppInterop.Runtime.Attributes;
using System.Collections;
using System.Text.Json;
using UnityEngine;

namespace BetterAmongUs.Network.Loaders;

/// <summary>
/// Handles downloading and processing of news data from a remote repository.
/// </summary>
[RegisterInIl2Cpp]
internal sealed class NewsLoader : MonoBehaviour
{
    /// <summary>
    /// Coroutine to fetch the news data from the remote repository.
    /// </summary>
    /// <returns>IEnumerator for coroutine execution.</returns>
    [HideFromIl2Cpp]
    internal IEnumerator CoFetchNewsData()
    {
        int count = 0;
        float delay = 0;
        while (!GithubAPI.IsInternetAvailable())
        {
            count++;
            if (count >= 17)
            {
                Destroy(this);
                yield break;
            }
            if (delay < 30f) delay += 2.5f;
            yield return new WaitForSeconds(delay);
        }

        string callBack = "";
        yield return GitHubFile.CoFetchTextFile(GitUrlPath.RepositoryApi.Combine("manifest.json").ToString(), text =>
        {
            callBack = text;
        });

        if (string.IsNullOrEmpty(callBack))
            yield break;

        var options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
        };

        var response = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(callBack, options);

        if (response == null || !response.ContainsKey("News"))
        {
            Logger_.Error("manifest.json deserialization failed or no 'News' key found.");
            yield break;
        }

        foreach (var file in response["News"])
        {
            yield return CoDownloadNewsFile(file);
        }

        yield return CoLoadNewsTest();

        Logger_.Log($"Loaded {ModNews.NewsDataToProcess.Count} news files");

        Destroy(this);
    }

    /// <summary>
    /// Coroutine to download an individual news file from the remote repository.
    /// </summary>
    /// <param name="fileName">The name of the news file to download.</param>
    /// <returns>IEnumerator for coroutine execution.</returns>
    /// <remarks>
    /// If the download fails or the file cannot be deserialized, the process is skipped.
    /// </remarks>
    [HideFromIl2Cpp]
    private IEnumerator CoDownloadNewsFile(string fileName)
    {
        string callBack = "";
        yield return GitHubFile.CoFetchTextFile(GitUrlPath.News.Combine(fileName).ToString(), text =>
        {
            callBack = text;
        });

        if (string.IsNullOrEmpty(callBack))
            yield break;

        try
        {
            var config = NewsData.Serialize(callBack);
            if (config == null || !config.Show) yield break;
            ModNews.NewsDataToProcess.Add(config);
        }
        catch (Exception ex)
        {
            Logger_.Error($"Failed to deserialize yaml for '{fileName}': {ex.Message}");
            yield break;
        }
    }

    /// <summary>
    /// Loads a test news configuration from an embedded resource for local testing purposes.
    /// </summary>
    /// <returns>IEnumerator for coroutine execution.</returns>
    [HideFromIl2Cpp]
    private IEnumerator CoLoadNewsTest()
    {
        string yamlDirectory = "BetterAmongUs.Resources.NewsTest";
        var assembly = ModInfo.Assembly;
        using Stream? resourceStream = assembly.GetManifestResourceStream(yamlDirectory);
        if (resourceStream != null)
        {
            using StreamReader reader = new(resourceStream);

            try
            {
                var config = NewsData.Serialize(reader.ReadToEnd());
                if (config == null || !config.Show) yield break;
                ModNews.NewsDataToProcess.Add(config);
            }
            catch (Exception ex)
            {
                Logger_.Error($"Failed to deserialize yaml for '{yamlDirectory}': {ex.Message}");
                yield break;
            }
        }
    }
}