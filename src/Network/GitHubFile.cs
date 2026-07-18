using BetterAmongUs.Managers;
using BetterAmongUs.Modules;
using Il2CppInterop.Runtime.Attributes;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace BetterAmongUs.Network;

/// <summary>
/// Provides methods for downloading files from GitHub repositories.
/// </summary>
internal static class GitHubFile
{
    /// <summary>
    /// Downloads an individual file from the remote repository and saves it locally.
    /// </summary>
    /// <param name="url">The URL of the file to download.</param>
    /// <param name="localFilePath">The local file path to save the downloaded file to.</param>
    /// <param name="callback">Optional callback to execute after successful download.</param>
    /// <param name="showProgress">Whether to show a progress bar during download.</param>
    /// <returns>IEnumerator for coroutine execution.</returns>
    /// <remarks>
    /// Handles errors and logs any failed downloads to avoid missing assets.
    /// </remarks>
    [HideFromIl2Cpp]
    internal static IEnumerator CoDownloadFile(string url, string localFilePath, Action<string>? callback = null, bool showProgress = false)
    {
        var www = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET)
        {
            downloadHandler = new DownloadHandlerBuffer()
        };

        if (showProgress)
        {
            CustomLoadingBarManager.ToggleLoadingBar(true);
            CustomLoadingBarManager.SetLoadingPercent(0f, "Starting download...");
        }

        var operation = www.SendWebRequest();

        // Track progress while downloading
        while (!operation.isDone)
        {
            if (showProgress)
            {
                int dotCount = (int)(Time.time * 2f) % 4;
                string dots = new('.', dotCount);
                float progress = www.downloadProgress * 100f;
                if (progress < 1f)
                {
                    CustomLoadingBarManager.SetLoadingPercent(0f, $"Starting Download{dots}");
                }
                CustomLoadingBarManager.SetLoadingPercent(progress, $"Downloading{dots}");
            }
            yield return null;
        }

        if (www.result == UnityWebRequest.Result.Success)
        {
            CustomLoadingBarManager.SetLoadingPercent(100f, "Saving File!");
            yield return new WaitForSeconds(1f);
            CustomLoadingBarManager.ToggleLoadingBar(false);
        }
        else
        {
            Logger_.Error($"Error downloading file from URL '{url}': {www.error} (Response Code: {(int)www.responseCode})");
            if (showProgress)
            {
                CustomLoadingBarManager.SetLoadingPercent(100f, "Download Failed!");
                yield return new WaitForSeconds(2f);
                CustomLoadingBarManager.ToggleLoadingBar(false);
            }
            yield break;
        }

        byte[] bytes = www.downloadHandler.GetNativeData().ToArray();
        File.WriteAllBytes(localFilePath, bytes);

        Logger_.Log($"Saved file: {localFilePath}");
        callback?.Invoke(localFilePath);
    }

    /// <summary>
    /// Downloads a text file from the remote repository.
    /// </summary>
    /// <param name="url">The URL of the text file to download.</param>
    /// <param name="Callback">Callback to execute with the downloaded text.</param>
    /// <returns>IEnumerator for coroutine execution.</returns>
    [HideFromIl2Cpp]
    internal static IEnumerator CoFetchTextFile(string url, Action<string> Callback)
    {
        var www = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET)
        {
            downloadHandler = new DownloadHandlerBuffer()
        };
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Logger_.Error($"Error downloading {url}: {www.error}");
            yield break;
        }

        var response = www.downloadHandler.text;
        www.Dispose();
        Callback.Invoke(response);
    }
}