using BetterAmongUs.Generated;

namespace BetterAmongUs.Modules.OptionItems;

/// <summary>
/// Represents a percentage option item that displays values from 0% to 100% with color coding.
/// </summary>
public sealed class OptionPercentItem : OptionFloatItem
{
    /// <summary>
    /// Creates a new percentage option item.
    /// </summary>
    /// <param name="tab">The tab this option belongs to.</param>
    /// <param name="translationString">The translation key for the option name.</param>
    /// <param name="defaultValue">The default percentage value (0-100).</param>
    /// <param name="parent">Optional parent option for hierarchical organization.</param>
    /// <returns>A new or existing OptionPercentItem instance.</returns>
    internal static OptionPercentItem Create(OptionTab tab, TranslationStrings.TranslationString translationString, float defaultValue, OptionItem? parent = null)
    {
        if (GetOptionByTranslationName(translationString) is OptionPercentItem floatItem)
        {
            floatItem.CreateBehavior();
            return floatItem;
        }

        OptionPercentItem Item = new();
        AllOptions.Add(Item);
        Item.Tab = tab;
        Item.TranslationName = translationString;
        Item.Increment = 5;
        Item.Range = new FloatRange(0f, 100f);
        Item.DefaultValue = defaultValue;
        Item.Fixs = ("", "");

        if (parent != null)
        {
            Item.Parent = parent;
            parent.Children.Add(Item);
        }

        Item.CreateBehavior();
        return Item;
    }

    /// <summary>
    /// Gets the string representation of the percentage value with color coding.
    /// </summary>
    /// <returns>A colored string showing the percentage (e.g., "75%").</returns>
    public sealed override string ValueAsString() => $"<color={GetColor(Value)}>{Value}%</color>";

    /// <summary>
    /// Gets the color code for a percentage value based on its magnitude.
    /// </summary>
    /// <param name="num">The percentage value (0-100).</param>
    /// <returns>A hex color code string representing the value range.</returns>
    internal string GetColor(float num)
    {
        switch (num)
        {
            case float n when n <= 0f:
                return "#ff0600"; // Red
            case float n when n <= 25f:
                return "#ff9d00"; // Orange
            case float n when n <= 50f:
                return "#fff900"; // Yellow
            case float n when n <= 75f:
                return "#80ff00"; // Light Green
            default:
                return "#80ff00"; // Green (76-100%)
        }
    }
}