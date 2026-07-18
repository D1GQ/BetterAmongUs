using BetterAmongUs.Generated;
using BetterAmongUs.Utilities;
using UnityEngine;

namespace BetterAmongUs.Modules.OptionItems.NoneOption;

/// <summary>
/// Represents a title item used to group options with a title in the UI.
/// </summary>
internal sealed class OptionTitleItem : OptionItem
{
    private ToggleOption? optionBehaviour;

    /// <summary>
    /// Gets a value indicating whether this option item can be loaded.
    /// </summary>
    /// <value>Always returns false for title items since they are non-interactive.</value>
    internal override bool CanLoad => false;

    /// <summary>
    /// Gets or sets the distance values for spacing around the title.
    /// </summary>
    /// <value>A tuple containing top and bottom distance values.</value>
    internal (float top, float bottom) Distance { get; set; }

    /// <summary>
    /// Creates a new title option item.
    /// </summary>
    /// <param name="tab">The tab where this title item will be placed.</param>
    /// <param name="translationString">The translation string for the title text.</param>
    /// <param name="topDistance">The top spacing distance (default: 0.15f).</param>
    /// <param name="bottomDistance">The bottom spacing distance (default: 0.50f).</param>
    /// <returns>A new <see cref="OptionTitleItem"/> instance.</returns>
    internal static OptionTitleItem Create(OptionTab tab, TranslationStrings.TranslationString translationString, float topDistance = 0.15f, float bottomDistance = 0.50f)
    {
        var Item = new OptionTitleItem
        {
            TranslationName = translationString,
            Tab = tab,
            Distance = (topDistance, bottomDistance)
        };
        Item.CreateBehavior();

        return Item;
    }

    /// <summary>
    /// Creates the visual behavior for the title item.
    /// </summary>
    /// <remarks>
    /// This method sets up the UI elements, positioning, and styling for the title.
    /// </remarks>
    private void CreateBehavior()
    {
        if (!GameSettingMenu.Instance)
            return;

        AllOptionsTemp.Add(this);
        optionBehaviour = UnityEngine.Object.Instantiate(Tab.AUTab.checkboxOrigin, Tab.AUTab.settingsContainer);
        Obj = optionBehaviour.gameObject;
        optionBehaviour.transform.localPosition = new Vector3(0.952f, 2f, -2f);
        SetupAUOption(optionBehaviour);

        optionBehaviour.CheckMark.transform.parent.gameObject.DestroyObj();
        optionBehaviour.GetComponent<ToggleOption>().DestroyMono();
        optionBehaviour.TitleText.DestroyTextTranslators();
        optionBehaviour.TitleText.text = Name;
        optionBehaviour.TitleText.alignment = TMPro.TextAlignmentOptions.Center;
        optionBehaviour.TitleText.outlineColor = Color.black;
        optionBehaviour.TitleText.outlineWidth = 0.2f;
        optionBehaviour.TitleText.transform.localPosition += new Vector3(0f, 0.05f, 0f);
        optionBehaviour.LabelBackground.color = new Color(0.6f, 0.6f, 0.6f, 1f);
        optionBehaviour.LabelBackground.transform.SetLocalZ(1f);

        optionBehaviour.LabelBackground.transform.localScale = new Vector3(1.6f, 1f);
        optionBehaviour.LabelBackground.transform.SetLocalX(-2.4f);
        optionBehaviour.TitleText.enableAutoSizing = false;
        optionBehaviour.TitleText.transform.SetLocalX(-2.4f);
        optionBehaviour.TitleText.alignment = TMPro.TextAlignmentOptions.Center;
        optionBehaviour.TitleText.enableWordWrapping = false;
        optionBehaviour.TitleText.fontSize = 3.5f;
        Tab.Children.Add(this);
    }

    /// <summary>
    /// Updates the visual representation of the title item.
    /// </summary>
    /// <param name="updateTabVisuals">Whether to update tab visuals (unused for title items).</param>
    internal sealed override void UpdateVisuals(bool updateTabVisuals = true)
    {
        if (optionBehaviour != null)
        {
            optionBehaviour.TitleText.text = Name;
        }
    }

    public sealed override string ValueAsString()
    {
        throw new NotImplementedException();
    }

    public override object GetBoxedValue()
    {
        throw new NotImplementedException();
    }
}