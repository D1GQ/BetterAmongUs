using BetterAmongUs.Generated;
using UnityEngine;

namespace BetterAmongUs.Modules.OptionItems.NoneOption;

/// <summary>
/// Represents a header item used to group options with a title in the UI.
/// </summary>
internal sealed class OptionHeaderItem : OptionItem
{
    private CategoryHeaderMasked? categoryHeaderMasked;

    /// <summary>
    /// Gets whether this option item loads values from persistent storage.
    /// </summary>
    internal override bool CanLoad => false;

    /// <summary>
    /// Gets whether this item represents a configurable option (false for headers).
    /// </summary>
    internal override bool IsOption => false;

    /// <summary>
    /// Gets or sets the spacing distances above and below the header.
    /// </summary>
    internal (float top, float bottom) Distance { get; set; }

    /// <summary>
    /// Creates a new header option item.
    /// </summary>
    /// <param name="tab">The tab this header belongs to.</param>
    /// <param name="translationString">The translation key for the header title.</param>
    /// <param name="topDistance">Spacing distance above the header.</param>
    /// <param name="bottomDistance">Spacing distance below the header.</param>
    /// <returns>A new OptionHeaderItem instance.</returns>
    internal static OptionHeaderItem Create(OptionTab tab, TranslationStrings.TranslationString translationString, float topDistance = 0.35f, float bottomDistance = 0.80f)
    {
        var Item = new OptionHeaderItem
        {
            TranslationName = translationString,
            Tab = tab,
            Distance = (topDistance, bottomDistance)
        };
        Item.CreateBehavior();

        return Item;
    }

    /// <summary>
    /// Creates the UI behavior for this header item.
    /// </summary>
    private void CreateBehavior()
    {
        if (!GameSettingMenu.Instance)
            return;

        AllOptionsTemp.Add(this);
        categoryHeaderMasked = UnityEngine.Object.Instantiate(Tab.AUTab.categoryHeaderOrigin, Tab.AUTab.settingsContainer);
        Obj = categoryHeaderMasked.gameObject;
        categoryHeaderMasked.transform.localScale = Vector3.one * 0.63f;
        categoryHeaderMasked.transform.localPosition = new Vector3(-0.903f, 2f, -2f);
        categoryHeaderMasked.Title.text = Name;
        categoryHeaderMasked.Title.outlineColor = Color.black;
        categoryHeaderMasked.Title.outlineWidth = 0.2f;
        categoryHeaderMasked.Title.fontSizeMax = 5f;
        categoryHeaderMasked.Background.material.SetInt(PlayerMaterial.MaskLayer, MaskLayer);
        if (categoryHeaderMasked.Divider != null)
        {
            categoryHeaderMasked.Divider.material.SetInt(PlayerMaterial.MaskLayer, MaskLayer);
        }
        categoryHeaderMasked.Title.fontMaterial.SetFloat("_StencilComp", 3f);
        categoryHeaderMasked.Title.fontMaterial.SetFloat("_Stencil", MaskLayer);
        Tab.Children.Add(this);
    }

    /// <summary>
    /// Updates the visual appearance of the header.
    /// </summary>
    /// <param name="updateTabVisuals">Whether to update the parent tab visuals as well.</param>
    internal sealed override void UpdateVisuals(bool updateTabVisuals = true)
    {
        if (categoryHeaderMasked != null)
        {
            categoryHeaderMasked.Title.text = Name;
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