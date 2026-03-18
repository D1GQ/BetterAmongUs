using BepInEx.Configuration;
using BetterAmongUs.Helpers;
using BetterAmongUs.Patches.Client;
using UnityEngine;

namespace BetterAmongUs.Modules;

/// <summary>
/// Represents a customizable client option item that can be toggled in the options menu.
/// </summary>
internal sealed class ClientOptionItem
{
    /// <summary>
    /// Gets the configuration entry associated with this option.
    /// </summary>
    internal ConfigEntry<bool>? Config { get; }

    /// <summary>
    /// Gets the toggle button behavior for this option.
    /// </summary>
    internal ToggleButtonBehaviour ToggleButton { get; }

    /// <summary>
    /// Gets the list of all created client option items.
    /// </summary>
    internal static readonly Dictionary<int, List<ClientOptionItem>> ClientOptions = [];

    /// <summary>
    /// Creates a toggle option with configuration binding.
    /// </summary>
    public static ClientOptionItem CreateToggle(string name, ConfigEntry<bool> config, int page, OptionsMenuBehaviour optionsMenuBehaviour, Action? onToggle = null, Func<bool>? toggleCheck = null)
    {
        var toggleButton = CreateToggleButton(name, optionsMenuBehaviour, GetOrCreatePage(page, optionsMenuBehaviour).transform);
        var item = new ClientOptionItem(name, config, toggleButton);

        item.SetupToggleButton(onToggle, toggleCheck);
        if (!ClientOptions.TryGetValue(page, out var options))
        {
            options = ClientOptions[page] = [];
        }
        options.Add(item);

        UpdateAllButtonPositions();

        return item;
    }

    /// <summary>
    /// Creates a button option without toggle state.
    /// </summary>
    public static ClientOptionItem CreateButton(string name, int page, OptionsMenuBehaviour optionsMenuBehaviour, Action onClick, Func<bool>? clickCheck = null)
    {
        var toggleButton = CreateToggleButton(name, optionsMenuBehaviour, GetOrCreatePage(page, optionsMenuBehaviour).transform);
        var item = new ClientOptionItem(name, null, toggleButton);

        item.SetupButton(onClick, clickCheck);
        if (!ClientOptions.TryGetValue(page, out var options))
        {
            options = ClientOptions[page] = [];
        }
        options.Add(item);

        UpdateAllButtonPositions();

        return item;
    }

    /// <summary>
    /// Initializes a new instance of the ClientOptionItem class.
    /// </summary>
    internal ClientOptionItem(string name, ConfigEntry<bool>? config, ToggleButtonBehaviour toggleButton)
    {
        Config = config;
        ToggleButton = toggleButton;
        ToggleButton.name = name;
    }

    /// <summary>
    /// Creates a toggle button GameObject for the options menu.
    /// </summary>
    private static ToggleButtonBehaviour CreateToggleButton(string name, OptionsMenuBehaviour optionsMenuBehaviour, Transform parent)
    {
        var mouseMoveToggle = optionsMenuBehaviour.DisableMouseMovement;
        var toggleButton = UnityEngine.Object.Instantiate(mouseMoveToggle, parent);
        toggleButton.name = name;
        toggleButton.Text.text = name;

        return toggleButton;
    }

    /// <summary>
    /// Calculates all option positions.
    /// </summary>
    private static void UpdateAllButtonPositions()
    {
        foreach (var kvp in ClientOptions)
        {
            int page = kvp.Key;
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                ClientOptionItem? option = kvp.Value[i];
                option.ToggleButton.gameObject.transform.localPosition = CalculateButtonPosition(page, i);
            }
        }
    }

    /// <summary>
    /// Calculates the position for a new button based on the current number of options.
    /// </summary>
    private static Vector3 CalculateButtonPosition(int page, int count)
    {
        if (page == -1)
        {
            if (!ClientOptions.TryGetValue(page, out var options))
            {
                options = ClientOptions[page] = [];
            }

            return new Vector3(
                options.Count == 1 ? 0f : count % 2 == 0 ? -1.3f : 1.3f,
                -1.8f,
                -6f
            );
        }

        return new Vector3(
            count % 2 == 0 ? -1.3f : 1.3f,
            1.8f - 0.5f * (count / 2),
            -6f
        );
    }

    /// <summary>
    /// Sets up a configuration-bound toggle button with click handler.
    /// </summary>
    internal void SetupToggleButton(Action? onToggle, Func<bool>? toggleCheck)
    {
        var passiveButton = ToggleButton.GetComponent<PassiveButton>();
        passiveButton.OnClick = new();

        passiveButton.OnClick.AddListener(() =>
        {
            if (toggleCheck?.Invoke() == false)
                return;

            if (Config != null)
            {
                Config.Value = !Config.Value;
                UpdateToggle();
            }
            onToggle?.Invoke();
        });

        UpdateToggle();
    }

    /// <summary>
    /// Sets up a button (non-toggle) with click handler.
    /// </summary>
    internal void SetupButton(Action onClick, Func<bool>? clickCheck)
    {
        var passiveButton = ToggleButton.GetComponent<PassiveButton>();
        passiveButton.OnClick = new();

        // Style for button (not toggle)
        ToggleButton.Text.text = ToggleButton.name;
        ToggleButton.Rollover?.ChangeOutColor(new Color32(0, 150, 0, 255));
        ToggleButton.Text.color = new Color(1f, 1f, 1f, 1f);

        passiveButton.OnClick.AddListener(() =>
        {
            if (clickCheck?.Invoke() == false)
                return;

            onClick?.Invoke();
        });
    }

    /// <summary>
    /// Updates the visual state of a config-bound toggle button.
    /// </summary>
    internal void UpdateToggle()
    {
        if (ToggleButton == null || Config == null)
            return;

        UpdateToggleVisuals(Config.Value);
    }

    /// <summary>
    /// Updates the visual appearance of a toggle button based on its state.
    /// </summary>
    private void UpdateToggleVisuals(bool isEnabled)
    {
        var color = isEnabled ?
            new Color32(0, 150, 0, 255) :
            new Color32(77, 77, 77, 255);

        var textColor = isEnabled ?
            new Color(1f, 1f, 1f, 1f) :
            new Color(1f, 1f, 1f, 0.5f);

        ToggleButton.Background.color = color;
        ToggleButton.Rollover?.ChangeOutColor(color);
        ToggleButton.Text.color = textColor;
        ToggleButton.Text.text = $"{ToggleButton.name}: {(isEnabled ? "On" : "Off")}";
    }

    /// <summary>
    /// Retrieves an existing page GameObject or creates a new one with navigation buttons.
    /// </summary>
    private static GameObject? GetOrCreatePage(int page, OptionsMenuBehaviour optionsMenuBehaviour, bool doNotCreate = false)
    {
        if (page == -1)
        {
            return OptionsMenuBehaviourPatch.BetterOptionsTab.Content;
        }

        string name = "Page " + page;
        var currentPage = OptionsMenuBehaviourPatch.BetterOptionsTab.Content.transform.Find(name)?.gameObject;
        if (currentPage == null)
        {
            if (doNotCreate)
            {
                return null;
            }

            currentPage = new GameObject(name);
            currentPage.SetActive(page == 1);
            currentPage.transform.SetParent(OptionsMenuBehaviourPatch.BetterOptionsTab.Content.transform);
            currentPage.transform.localPosition = Vector3.zero;
            currentPage.transform.localScale = Vector3.one;

            int previous = page - 1;
            if (previous > 0)
            {
                var previousPage = GetOrCreatePage(previous, optionsMenuBehaviour, true);
                if (previousPage != null)
                {
                    CreatePreviousButton(currentPage, previousPage, optionsMenuBehaviour);
                    CreateNextButton(previousPage, currentPage, optionsMenuBehaviour);
                }
            }
        }

        return currentPage;
    }

    /// <summary>
    /// Creates a "Next" navigation button that switches to the specified next page.
    /// </summary>
    private static void CreateNextButton(GameObject page, GameObject nextPage, OptionsMenuBehaviour optionsMenuBehaviour)
    {
        var button = CreateToggleButton("Next >", optionsMenuBehaviour, page.transform);
        button.transform.localPosition = new Vector3(2f, -2.5f, 0f);
        var background = button.transform.Find("Background");
        if (background != null)
        {
            background.localScale = new Vector3(0.5f, 1f, 1f);
        }
        var buttonHighlight = button.transform.Find("ButtonHighlight");
        if (buttonHighlight != null)
        {
            buttonHighlight.localScale = new Vector3(0.5f, 0.95f, 1f);
        }
        var passiveButton = button.GetComponent<PassiveButton>();
        passiveButton.OnClick = new();
        passiveButton.OnClick.AddListener(() =>
        {
            page.SetActive(false);
            nextPage.SetActive(true);
        });
    }

    /// <summary>
    /// Creates a "Previous" navigation button that switches to the specified previous page.
    /// </summary>
    private static void CreatePreviousButton(GameObject page, GameObject previousPage, OptionsMenuBehaviour optionsMenuBehaviour)
    {
        var button = CreateToggleButton("< Prev", optionsMenuBehaviour, page.transform);
        button.transform.localPosition = new Vector3(-2f, -2.5f, 0f);
        var background = button.transform.Find("Background");
        if (background != null)
        {
            background.localScale = new Vector3(0.5f, 1f, 1f);
        }
        var buttonHighlight = button.transform.Find("ButtonHighlight");
        if (buttonHighlight != null)
        {
            buttonHighlight.localScale = new Vector3(0.5f, 0.95f, 1f);
        }
        var passiveButton = button.GetComponent<PassiveButton>();
        passiveButton.OnClick = new();
        passiveButton.OnClick.AddListener(() =>
        {
            page.SetActive(false);
            previousPage.SetActive(true);
        });
    }
}