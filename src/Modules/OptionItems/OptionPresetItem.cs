using BetterAmongUs.Data;
using BetterAmongUs.Data.Config;
using BetterAmongUs.Generated;
using BetterAmongUs.Patches.Gameplay.UI.Settings;

namespace BetterAmongUs.Modules.OptionItems;

/// <summary>
/// Represents a preset option item that set the settings preset.
/// </summary>
internal sealed class OptionPresetItem : OptionStringItem
{
    internal override bool CanLoad => false;

    /// <summary>
    /// Creates a new preset item for the options menu.
    /// </summary>
    /// <returns>The created or reused <see cref="OptionPresetItem"/> instance.</returns>
    internal static OptionPresetItem Create()
    {
        if (GetOptionByTranslationName(TranslationStrings.Setting_Presets) is OptionPresetItem stringItem)
        {
            stringItem.CreateBehavior();
            return stringItem;
        }

        OptionPresetItem Item = new();
        AllOptions.Add(Item);
        Item.Tab = GameSettingsPatch.BetterSettingsTab;
        Item.TranslationName = TranslationStrings.Setting_Presets;
        Item.TranslatorStrings = Enumerable.Repeat(new TranslationStrings.TranslationString(string.Empty), 10).ToArray();
        Item.Range = new IntRange(0, 10);
        Item.DefaultValue = 0;
        Item.Value = BAUConfigs.SettingsPreset.Value;

        Item.CreateBehavior();
        return Item;
    }

    internal override void OnValueChange(int oldValue, int newValue)
    {
        BAUConfigs.SettingsPreset.Value = newValue;
        BetterDataManager.Files.BetterGameSettingsFile = new();
        BetterDataManager.Files.BetterGameSettingsFile.Init();
        foreach (var opt in AllOptions)
        {
            opt.TryLoad(true);
        }
        GameSettingsPatch.BetterSettingsTab.UpdateVisuals();
    }

    public sealed override string ValueAsString()
    {
        return TranslationStrings.Setting_Preset.Format(Value.ToString());
    }
}
