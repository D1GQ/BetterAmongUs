using BepInEx.Unity.IL2CPP.Utils;
using BetterAmongUs.Attributes;
using BetterAmongUs.Network.Loaders;
using Il2CppInterop.Runtime.Attributes;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace BetterAmongUs.Network;

/// <summary>
/// Manages API connections to GitHub for news, updates, and user data.
/// </summary>
[RegisterInIl2Cpp]
internal sealed class GithubAPI : MonoBehaviour
{
    /// <summary>
    /// Gets the singleton instance of the GithubAPI.
    /// </summary>
    /// <value>The current instance, or null if not initialized.</value>
    internal static GithubAPI? Instance { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the API connection was successful.
    /// </summary>
    internal static bool HasConnectedAPI { get; private set; } = false;

    /// <summary>
    /// Gets a value indicating whether the API connection process has finished.
    /// </summary>
    internal static bool Finished { get; private set; } = false;

    /// Gets a value indicating whether content is currently being downloaded.
    /// </summary>
    internal static bool Downloading { get; private set; }

    private static bool hasTryConnect = false;

    /// <summary>
    /// Initializes and connects to the GitHub API.
    /// </summary>
    /// <remarks>
    /// Creates a persistent GameObject to manage API connections.
    /// This method ensures only one connection attempt is made.
    /// </remarks>
    internal static void Connect()
    {
        if (hasTryConnect)
            return;

        hasTryConnect = true;

        var obj = new GameObject("GithubAPI(BAU)") { hideFlags = HideFlags.HideAndDontSave };
        DontDestroyOnLoad(obj);
        Instance = obj.AddComponent<GithubAPI>();
    }

    internal void Start()
    {
        ConnectToAPI();
    }

    /// <summary>
    /// Connects to various GitHub API endpoints.
    /// </summary>
    [HideFromIl2Cpp]
    private void ConnectToAPI()
    {
        this.StartCoroutine(CoConnectToAPI());
    }

    /// <summary>
    /// Connects to various GitHub API endpoints.
    /// </summary>
    private IEnumerator CoConnectToAPI()
    {
        Downloading = true;

        var newsLoader = gameObject.AddComponent<NewsLoader>();
        yield return newsLoader.CoFetchNewsData();

        // Disable auto updating on Starlight
        if (!ModInfo.Starlight)
        {
            var updateLoader = gameObject.AddComponent<BAUUpdateLoader>();
            yield return updateLoader.CoFetchUpdateData();
        }

        Downloading = false;
    }

    /// <summary>
    /// Sets the API connection status based on the web request result.
    /// </summary>
    /// <param name="www">The UnityWebRequest that was made.</param>
    /// <param name="hasErrored">Whether an error occurred during the request.</param>
    internal static void SetConnectedAPI(UnityWebRequest www, bool hasErrored)
    {
        if (www.result == UnityWebRequest.Result.ConnectionError ||
            www.result == UnityWebRequest.Result.ProtocolError || hasErrored)
        {
            HasConnectedAPI = false;
        }
        else
        {
            HasConnectedAPI = true;
        }
    }

    /// <summary>
    /// Checks if internet connectivity is available.
    /// </summary>
    /// <returns>True if internet connection is available; otherwise, false.</returns>
    /// <remarks>
    /// Performs a quick test by attempting to connect to Google's 204 endpoint.
    /// </remarks>
    internal static bool IsInternetAvailable()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
            return false;

        UnityWebRequest? www = null;
        try
        {
            www = UnityWebRequest.Get("https://clients3.google.com/generate_204");
            www.SendWebRequest();
            while (!www.isDone) { }
            return www.result == UnityWebRequest.Result.Success && www.responseCode == 204;
        }
        catch
        {
            return false;
        }
        finally
        {
            www?.Dispose();
        }
    }
}