namespace BetterAmongUs.Managers;

/// <summary>
/// Manages custom loading bar functionality for BetterAmongUs.
/// </summary>
internal static class CustomLoadingBarManager
{
    /// <summary>
    /// Gets the current loading bar instance.
    /// </summary>
    internal static AmongUsLoadingBar? LoadingBar => LoadingBarManager.InstanceExists ? LoadingBarManager.Instance.loadingBar : null;

    /// <summary>
    /// Toggles the loading bar visibility.
    /// </summary>
    /// <param name="on">True to show the loading bar, false to hide it.</param>
    internal static void ToggleLoadingBar(bool on)
    {
        LoadingBarManager.Instance.loadingBar.gameObject.SetActive(on);
    }

    /// <summary>
    /// Sets the loading bar progress percentage and text.
    /// </summary>
    /// <param name="percent">The progress percentage (0-100).</param>
    /// <param name="loadText">The text to display on the loading bar.</param>
    internal static void SetLoadingPercent(float percent, string loadText)
    {
        var loadingBar = LoadingBarManager.Instance.loadingBar;
        loadingBar.SetLoadingPercent(percent, StringNames.None);
        loadingBar.loadingText.SetText(loadText);
    }
}