using BetterAmongUs.Generated;
using BetterAmongUs.Utilities;
using BetterAmongUs.Utilities.Extension;
using UnityEngine;

namespace BetterAmongUs.Modules.OptionItems;

/// <summary>
/// Represents a checkbox option item that can be toggled on or off.
/// </summary>
public sealed class OptionCheckboxItem : OptionItem<bool>
{
    /// <summary>
    /// Gets whether child options should be shown, based on the checkbox value.
    /// </summary>
    internal sealed override bool ShowChildren => base.ShowChildren && Value;

    /// <summary>
    /// Creates a new checkbox option item.
    /// </summary>
    /// <param name="tab">The tab this option belongs to.</param>
    /// <param name="translationString">The translation key for the option name.</param>
    /// <param name="defaultValue">The default value for the checkbox.</param>
    /// <param name="parent">Optional parent option for hierarchical organization.</param>
    /// <returns>A new or existing OptionCheckboxItem instance.</returns>
    internal static OptionCheckboxItem Create(OptionTab tab, TranslationStrings.TranslationString translationString, bool defaultValue, OptionItem? parent = null)
    {
        if (GetOptionByTranslationName(translationString) is OptionCheckboxItem checkboxItem)
        {
            checkboxItem.CreateBehavior();
            return checkboxItem;
        }

        OptionCheckboxItem Item = new();
        AllOptions.Add(Item);
        Item.Tab = tab;
        Item.TranslationName = translationString;
        Item.DefaultValue = defaultValue;

        if (parent != null)
        {
            Item.Parent = parent;
            parent.Children.Add(Item);
        }

        Item.CreateBehavior();
        return Item;
    }

    /// <summary>
    /// Creates the UI behavior for this checkbox option.
    /// </summary>
    protected sealed override void CreateBehavior()
    {
        TryLoad();
        if (!GameSettingMenu.Instance)
            return;

        AllOptionsTemp.Add(this);
        var ToggleOption = UnityEngine.Object.Instantiate(Tab.AUTab.checkboxOrigin, Tab.AUTab.settingsContainer);
        Option = ToggleOption;
        Obj = Option.gameObject;
        Option.enabled = false;
        Tab.Children.Add(this);
        TitleTMP = ToggleOption.TitleText;
        SetupText(ToggleOption.TitleText);
        SetupOptionBehavior();
        SetOptionVisuals();
    }

    /// <summary>
    /// Sets up the specific behavior for the ToggleOption component.
    /// </summary>
    protected sealed override void SetupOptionBehavior()
    {
        if (Option is ToggleOption toggleOption)
        {
            SetupAUOption(Option);
            toggleOption.DestroyTextTranslators();
            toggleOption.TitleText.text = Name;
            var button = toggleOption.buttons[0];
            button.OnClick = new();
            button.OnClick.AddListener(() => SetValue(!Value));
        }
    }

    /// <summary>
    /// Updates the visual appearance of the checkbox based on its current value.
    /// </summary>
    /// <param name="updateTabVisuals">Whether to update the parent tab visuals as well.</param>
    internal sealed override void UpdateVisuals(bool updateTabVisuals = true)
    {
        if (Option is ToggleOption toggleOption)
        {
            toggleOption.CheckMark.enabled = Value;
        }

        if (updateTabVisuals)
        {
            Tab.UpdateVisuals();
        }
    }

    /// <summary>
    /// Gets the string representation of the checkbox value with color formatting.
    /// </summary>
    /// <returns>A colored string indicating "On" (green) or "Off" (red).</returns>
    public sealed override string ValueAsString()
    {
        Color color = Value ? Color.green : Color.red;
        string @bool = Value ? "On" : "Off";
        return $"<color={Colors.Color32ToHex(color)}>{@bool}</color>";
    }

    /// <summary>
    /// Gets the boolean value of this checkbox option.
    /// </summary>
    /// <returns>The current boolean value.</returns>
    public bool GetBool() => GetValue();

    /// <summary>
    /// Checks if the checkbox value matches a specific boolean.
    /// </summary>
    /// <param name="value">The boolean value to compare against.</param>
    /// <returns>True if the checkbox value matches, false otherwise.</returns>
    public sealed override bool Is(bool value) => value == GetBool();
}