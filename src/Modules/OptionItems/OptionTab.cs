using BetterAmongUs.Helpers;
using BetterAmongUs.Modules.OptionItems.NoneOption;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Modules.OptionItems;

/// <summary>
/// Represents a tab in the options menu that groups related option items.
/// </summary>
internal sealed class OptionTab
{
    internal static List<OptionTab> AllTabs = [];

    internal readonly List<OptionItem> Children = [];

    /// <summary>
    /// Gets the unique identifier for this tab.
    /// </summary>
    internal int Id { get; private set; }

    /// <summary>
    /// Gets the translated name of this tab.
    /// </summary>
    internal string? Name => Translator.GetString(TranName);

    /// <summary>
    /// Gets or sets the translation key for the tab name.
    /// </summary>
    internal string? TranName { get; private set; }

    /// <summary>
    /// Gets the translated description of this tab.
    /// </summary>
    internal string? Description => Translator.GetString(TranDescription);

    /// <summary>
    /// Gets or sets the translation key for the tab description.
    /// </summary>
    internal string? TranDescription { get; private set; }

    /// <summary>
    /// Gets or sets the Among Us options menu tab instance.
    /// </summary>
    internal GameOptionsMenu? AUTab { get; private set; }

    /// <summary>
    /// Gets or sets the button that activates this tab.
    /// </summary>
    internal PassiveButton? TabButton { get; private set; }

    /// <summary>
    /// Gets or sets the color theme for this tab.
    /// </summary>
    internal Color Color { get; private set; }

    /// <summary>
    /// Creates a new option tab or returns an existing one with the same ID.
    /// </summary>
    /// <param name="Id">The unique identifier for the tab.</param>
    /// <param name="tranName">Translation key for the tab name.</param>
    /// <param name="tranDescription">Translation key for the tab description.</param>
    /// <param name="Color">The color theme for the tab.</param>
    /// <param name="doNotDestroyMapPicker">Whether to preserve the map picker UI.</param>
    /// <returns>A new or existing OptionTab instance.</returns>
    internal static OptionTab Create(int Id, string tranName, string tranDescription, Color Color, bool doNotDestroyMapPicker = false)
    {
        if (GetTabById(Id) is OptionTab optionTab)
        {
            optionTab.Children.Clear();
            optionTab.CreateBehavior(doNotDestroyMapPicker);
            return optionTab;
        }

        var Item = new OptionTab
        {
            Id = Id,
            TranName = tranName,
            TranDescription = tranDescription,
            Color = Color
        };
        AllTabs.Add(Item);

        Item.CreateBehavior(doNotDestroyMapPicker);
        return Item;
    }

    /// <summary>
    /// Gets an option tab by its ID.
    /// </summary>
    /// <param name="id">The ID of the tab to find.</param>
    /// <returns>The OptionTab with the matching ID, or null if not found.</returns>
    internal static OptionTab? GetTabById(int id) => AllTabs.FirstOrDefault(tab => tab.Id == id);

    /// <summary>
    /// Creates the UI behavior for this option tab.
    /// </summary>
    /// <param name="doNotDestroyMapPicker">Whether to preserve the map picker UI.</param>
    private void CreateBehavior(bool doNotDestroyMapPicker)
    {
        if (!GameSettingMenu.Instance)
            return;

        var SettingsButton = UnityEngine.Object.Instantiate(GameSettingMenu.Instance.GameSettingsButton, GameSettingMenu.Instance.GameSettingsButton.transform.parent);
        TabButton = SettingsButton;
        SettingsButton.DestroyTextTranslators();
        var title = SettingsButton.GetComponentInChildren<TextMeshPro>();
        title?.SetText(Name);

        SettingsButton.gameObject.SetActive(true);
        SettingsButton.name = Name;
        SettingsButton.OnClick.RemoveAllListeners();
        SettingsButton.OnMouseOver.RemoveAllListeners();

        var darkColor = Color * 0.5f;
        SettingsButton.activeSprites.GetComponent<SpriteRenderer>().color = darkColor * 0.9f;
        SettingsButton.inactiveSprites.GetComponent<SpriteRenderer>().color = darkColor * 0.8f;
        SettingsButton.selectedSprites.GetComponent<SpriteRenderer>().color = darkColor;
        SettingsButton.activeTextColor = Color * 0.9f;
        SettingsButton.inactiveTextColor = Color * 0.8f;
        SettingsButton.selectedTextColor = Color;

        SettingsButton.gameObject.GetComponent<BoxCollider2D>().size = new Vector2(2.5f, 0.6176f);

        SettingsButton.OnClick.AddListener(() =>
        {
            GameSettingMenu.Instance.ChangeTab(Id, false);
        });

        var SettingsTab = UnityEngine.Object.Instantiate(GameSettingMenu.Instance.GameSettingsTab, GameSettingMenu.Instance.GameSettingsTab.transform.parent);
        AUTab = SettingsTab;
        SettingsTab.name = Name;
        if (!doNotDestroyMapPicker) SettingsTab.scrollBar.Inner.DestroyChildren();

        AUTab.gameObject.SetActive(false);
    }

    /// <summary>
    /// Updates the visual layout of all options in this tab.
    /// </summary>
    internal void UpdateVisuals()
    {
        ShowOptions();
    }

    /// <summary>
    /// Shows and positions all option items in this tab.
    /// </summary>
    private void ShowOptions()
    {
        if (AUTab == null)
            return;

        AUTab.gameObject.SetActive(true);
        float spacingNum = 0f;
        foreach (var opt in Children)
        {
            if (opt == null) continue;
            if (opt.Obj == null) continue;
            if (opt.Tab == null) continue;

            if (opt.Tab.Id != Id || opt.Hide)
            {
                opt.Obj.gameObject.SetActive(false);
                continue;
            }

            opt.Obj.gameObject.SetActive(true);

            spacingNum += opt switch
            {
                OptionHeaderItem headerItem => headerItem.Distance.top,
                OptionTitleItem titleItem => titleItem.Distance.top,
                OptionDividerItem dividerItem => dividerItem.Distance.top,
                _ => 0f,
            };

            if (opt.IsOption)
            {
                opt.Obj.transform.localPosition = new Vector3(1.4f, 2f - 1f * spacingNum, 0f);
            }
            else
            {
                opt.Obj.transform.localPosition = new Vector3(-0.6f, 2f - 1f * spacingNum, 0f);
            }

            spacingNum += opt switch
            {
                OptionHeaderItem headerItem => headerItem.Distance.bottom,
                OptionTitleItem titleItem => titleItem.Distance.bottom,
                OptionDividerItem dividerItem => dividerItem.Distance.bottom,
                _ => 0.45f,
            };

            opt.UpdateVisuals(false);
        }

        AUTab?.scrollBar?.SetYBoundsMax(spacingNum - 2.5f);
        AUTab?.scrollBar?.ScrollRelative(new(0f, 0f));
    }

    /// <summary>
    /// Finds options by name (not implemented).
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <exception cref="NotImplementedException">Always thrown as this method is not implemented.</exception>
    internal static void FindOptions(string name)
    {
        throw new NotImplementedException();
    }
}