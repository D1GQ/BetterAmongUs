using BetterAmongUs.Generated;
using BetterAmongUs.Utilities;
using BetterAmongUs.Utilities.Extension;
using UnityEngine;

namespace BetterAmongUs.Modules.OptionItems;

/// <summary>
/// Represents a floating-point number option item with adjustable value range.
/// </summary>
public class OptionFloatItem : OptionItem<float>
{
    /// <summary>
    /// Gets whether child options should be shown, based on whether the value is greater than zero.
    /// </summary>
    internal override bool ShowChildren => base.ShowChildren && Value > 0f;

    /// <summary>
    /// Gets or sets the valid range for this float option.
    /// </summary>
    protected FloatRange? Range { get; set; }

    /// <summary>
    /// Gets or sets the increment/decrement step size for this option.
    /// </summary>
    protected float Increment { get; set; }

    /// <summary>
    /// Gets or sets whether this option can represent infinite values (value ≤ 0).
    /// </summary>
    protected bool CanBeInfinite { get; set; }

    /// <summary>
    /// Gets or sets the prefix and postfix strings for value display.
    /// </summary>
    protected (string prefix, string postfix) Fixs { get; set; }

    /// <summary>
    /// Creates a new float option item.
    /// </summary>
    /// <param name="tab">The tab this option belongs to.</param>
    /// <param name="translationString">The translation key for the option name.</param>
    /// <param name="Min_Max_Increment">Tuple containing min value, max value, and increment step.</param>
    /// <param name="defaultValue">The default value for the option.</param>
    /// <param name="Prefix_Postfix">Tuple containing prefix and postfix strings for display.</param>
    /// <param name="parent">Optional parent option for hierarchical organization.</param>
    /// <param name="canBeInfinite">Whether this option can represent infinite values.</param>
    /// <returns>A new or existing OptionFloatItem instance.</returns>
    internal static OptionFloatItem Create(OptionTab tab, TranslationStrings.TranslationString translationString, (float minValue, float maxValue, float incrementValue) Min_Max_Increment, float defaultValue, (string prefix, string postfix) Prefix_Postfix = new(), OptionItem? parent = null, bool canBeInfinite = false)
    {
        if (GetOptionByTranslationName(translationString) is OptionFloatItem floatItem)
        {
            floatItem.CreateBehavior();
            return floatItem;
        }

        OptionFloatItem Item = new();
        AllOptions.Add(Item);
        Item.Tab = tab;
        Item.TranslationName = translationString;
        Item.Increment = Min_Max_Increment.incrementValue;
        Item.CanBeInfinite = canBeInfinite;
        Item.Range = new FloatRange(Min_Max_Increment.minValue, Min_Max_Increment.maxValue);
        Item.DefaultValue = defaultValue;
        Item.Fixs = Prefix_Postfix;

        if (parent != null)
        {
            Item.Parent = parent;
            parent.Children.Add(Item);
        }

        Item.CreateBehavior();
        return Item;
    }

    /// <summary>
    /// Creates the UI behavior for this float option.
    /// </summary>
    protected override void CreateBehavior()
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
    protected override void SetupOptionBehavior()
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
    /// Increases the value based on modifier keys for larger increments.
    /// </summary>
    protected void Increase()
    {
        int plus = 1;
        if (Input.GetKey(KeyCode.LeftShift))
            plus = 5;
        if (Input.GetKey(KeyCode.LeftControl))
            plus = 10;
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl))
            plus = 25;
        var value = Value;
        value += Increment * plus;
        SetValue(value);
    }

    /// <summary>
    /// Decreases the value based on modifier keys for larger decrements.
    /// </summary>
    protected void Decrease()
    {
        int plus = 1;
        if (Input.GetKey(KeyCode.LeftShift))
            plus = 5;
        if (Input.GetKey(KeyCode.LeftControl))
            plus = 10;
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl))
            plus = 25;
        var value = Value;
        value -= Increment * plus;
        SetValue(value);
    }

    /// <summary>
    /// Sets the value, clamping it to the valid range.
    /// </summary>
    /// <param name="newValue">The new value to set.</param>
    public override void SetValue(float newValue)
    {
        newValue = Math.Clamp(newValue, Range.min, Range.max);
        base.SetValue(newValue);
    }

    /// <summary>
    /// Updates the visual appearance of the float option based on its current value.
    /// </summary>
    /// <param name="updateTabVisuals">Whether to update the parent tab visuals as well.</param>
    internal override void UpdateVisuals(bool updateTabVisuals = true)
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

            numberOption.ValueText.text = ValueAsString().Replace(InfiniteIcon, InfiniteIcon.Size(200f));
        }

        if (updateTabVisuals)
        {
            Tab.UpdateVisuals();
        }
    }

    /// <summary>
    /// Gets the string representation of the float value with formatting.
    /// </summary>
    /// <returns>A formatted string showing the value with prefix/postfix or infinity symbol.</returns>
    public override string ValueAsString()
    {
        if (CanBeInfinite)
        {
            if (Value <= 0f)
            {
                return InfiniteIcon;
            }
        }

        return $"{Fixs.prefix}{Value}{Fixs.postfix}";
    }

    /// <summary>
    /// Gets the float value of this option.
    /// </summary>
    /// <returns>The current float value.</returns>
    public float GetFloat() => GetValue();

    /// <summary>
    /// Checks if the option value matches a specific float.
    /// </summary>
    /// <param name="value">The float value to compare against.</param>
    /// <returns>True if the option value matches, false otherwise.</returns>
    public override bool Is(float value) => value == GetFloat();
}