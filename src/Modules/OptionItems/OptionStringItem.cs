using BetterAmongUs.Generated;
using BetterAmongUs.Utilities;
using BetterAmongUs.Utilities.Extension;

namespace BetterAmongUs.Modules.OptionItems;

/// <summary>
/// Represents an option item that selects from a list of string values.
/// </summary>
public class OptionStringItem : OptionItem<int>
{
    /// <summary>
    /// Gets or sets the valid range of indices for this string option.
    /// </summary>
    protected IntRange Range { get; set; } = new();

    /// <summary>
    /// Gets or sets the array of translation keys for the string options.
    /// </summary>
    protected TranslationStrings.TranslationString[] TranslatorStrings { get; set; } = [];

    /// <summary>
    /// Creates a new string option item.
    /// </summary>
    /// <param name="tab">The tab this option belongs to.</param>
    /// <param name="translationStringName">The translation key for the option name.</param>
    /// <param name="translationStrings">Array of translation keys for the selectable string values.</param>
    /// <param name="defaultValue">The default index value.</param>
    /// <param name="parent">Optional parent option for hierarchical organization.</param>
    /// <returns>A new or existing OptionStringItem instance.</returns>
    /// <exception cref="ArgumentException">Thrown when tranStrings has less than 2 elements.</exception>
    internal static OptionStringItem Create(OptionTab tab, TranslationStrings.TranslationString translationStringName, TranslationStrings.TranslationString[] translationStrings, int defaultValue, OptionItem? parent = null)
    {
        if (translationStrings.Length < 2)
        {
            throw new ArgumentException("tranStrings must have more then 1 string!");
        }

        if (GetOptionByTranslationName(translationStringName) is OptionStringItem stringItem)
        {
            stringItem.CreateBehavior();
            return stringItem;
        }

        OptionStringItem Item = new();
        AllOptions.Add(Item);
        Item.Tab = tab;
        Item.TranslationName = translationStringName;
        Item.TranslatorStrings = translationStrings;
        Item.Range = new IntRange(0, translationStrings.Length - 1);
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
    /// Creates the UI behavior for this string option.
    /// </summary>
    protected sealed override void CreateBehavior()
    {
        TryLoad();
        if (!GameSettingMenu.Instance)
            return;

        AllOptionsTemp.Add(this);
        var numberOption = UnityEngine.Object.Instantiate(Tab.AUTab.numberOptionOrigin, Tab.AUTab.settingsContainer);
        Option = numberOption;
        Obj = Option.gameObject;
        Option.enabled = false;
        Tab.Children.Add(this);
        TitleTMP = numberOption.TitleText;
        ValueTMP = numberOption.ValueText;
        SetupText(numberOption.TitleText);
        SetupOptionBehavior();
        SetOptionVisuals();
    }

    /// <summary>
    /// Sets up the specific behavior for the NumberOption component.
    /// </summary>
    protected sealed override void SetupOptionBehavior()
    {
        if (Option is NumberOption numberOption)
        {
            SetupAUOption(Option);
            numberOption.DestroyTextTranslators();
            numberOption.TitleText.text = Name;
            numberOption.PlusBtn.OnClick = new();
            numberOption.PlusBtn.OnClick.AddListener(Increase);
            numberOption.MinusBtn.OnClick = new();
            numberOption.MinusBtn.OnClick.AddListener(Decrease);
        }
    }

    /// <summary>
    /// Increases the string selection index.
    /// </summary>
    protected void Increase()
    {
        SetValue(Value + 1);
    }

    /// <summary>
    /// Decreases the string selection index.
    /// </summary>
    protected void Decrease()
    {
        SetValue(Value - 1);
    }

    /// <summary>
    /// Sets the string selection value, clamping it to the valid range.
    /// </summary>
    /// <param name="newValue">The new string index to set.</param>
    public sealed override void SetValue(int newValue)
    {
        newValue = Math.Clamp(newValue, Range.min, Range.max);
        base.SetValue(newValue);
    }

    /// <summary>
    /// Updates the visual appearance of the string option based on its current value.
    /// </summary>
    /// <param name="updateTabVisuals">Whether to update the parent tab visuals as well.</param>
    internal sealed override void UpdateVisuals(bool updateTabVisuals = true)
    {
        if (Option is NumberOption numberOption)
        {
            numberOption.PlusBtn.SetInteractable(false);
            numberOption.MinusBtn.SetInteractable(false);

            if (Value < Range.max)
            {
                numberOption.PlusBtn.SetInteractable(true);
            }
            if (Value > Range.min)
            {
                numberOption.MinusBtn.SetInteractable(true);
            }

            numberOption.ValueText.text = $"{ValueAsString()}";
        }

        if (updateTabVisuals)
        {
            Tab.UpdateVisuals();
        }
    }

    /// <summary>
    /// Gets the translated string representation of the current selection.
    /// </summary>
    /// <returns>The translated string for the current index.</returns>
    public string GetString() => Translator.GetString(TranslatorStrings[Value], showInvalid: false);

    /// <summary>
    /// Gets the current index value of the selected string option.
    /// </summary>
    /// <returns>The current integer index value.</returns>
    public int GetStringIndex() => GetValue();

    /// <summary>
    /// Gets the translated string representation of the current selection.
    /// </summary>
    /// <returns>The translated string for the current index.</returns>
    public override string ValueAsString() => GetString();

    /// <summary>
    /// Checks if the option's string value matches a specific string.
    /// </summary>
    /// <param name="str">The string value to compare against.</param>
    /// <returns>True if the option value matches, false otherwise.</returns>
    public bool Is(string str) => TranslatorStrings[Value].Key == str || ValueAsString() == str;

    /// <summary>
    /// Checks if the option's index value matches a specific integer.
    /// </summary>
    /// <param name="value">The integer value to compare against.</param>
    /// <returns>True if the option value matches, false otherwise.</returns>
    public sealed override bool Is(int value) => Value == value;
}