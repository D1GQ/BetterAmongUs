using AmongUs.Data;
using BetterAmongUs.Modules.Support;

namespace BetterAmongUs.Data.Json;

/// <summary>
/// Represents outfit data including hat, pet, skin, visor, and nameplate information.
/// </summary>
[Serializable]
internal sealed class OutfitData
{
    /// <summary>
    /// The ID of the hat.
    /// </summary>
    public string HatId = HatData.EmptyId;

    /// <summary>
    /// The ID of the pet.
    /// </summary>
    public string PetId = PetData.EmptyId;

    /// <summary>
    /// The ID of the skin.
    /// </summary>
    public string SkinId = SkinData.EmptyId;

    /// <summary>
    /// The ID of the visor.
    /// </summary>
    public string VisorId = VisorData.EmptyId;

    /// <summary>
    /// The ID of the nameplate.
    /// </summary>
    public string NamePlateId = NamePlateData.EmptyId;

    /// <summary>
    /// Gets the outfit data for the currently selected preset.
    /// </summary>
    /// <returns>The outfit data for the current preset.</returns>
    internal static OutfitData GetCurrentOutfitData() =>
        BetterDataManager.Files.BetterDataFile.OutfitData.ElementAt(BetterDataManager.Files.BetterDataFile.SelectedOutfitPreset);

    /// <summary>
    /// Gets the outfit data for a specific preset index.
    /// </summary>
    /// <param name="index">The index of the preset to retrieve.</param>
    /// <returns>The outfit data for the specified preset.</returns>
    internal static OutfitData GetOutfitDataAt(int index) =>
        BetterDataManager.Files.BetterDataFile.OutfitData.ElementAt(index);

    private static bool ignoreChange;

    /// <summary>
    /// Initializes the outfit data system and sets up change listeners.
    /// </summary>
    internal static void Initialize()
    {
        FindCurrentPreset();

        var Save = () =>
        {
            if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_OutfitPresets))
                return;

            if (ignoreChange)
                return;

            GetCurrentOutfitData().UpdateFromCustomizationData();
            BetterDataManager.Files.BetterDataFile.Save();
        };

        DataManager.Player.Customization.OnHatChanged += Save;
        DataManager.Player.Customization.OnPetChanged += Save;
        DataManager.Player.Customization.OnSkinChanged += Save;
        DataManager.Player.Customization.OnVisorChanged += Save;
        DataManager.Player.Customization.OnNamePlateChanged += Save;
        Save.Invoke();
    }

    /// <summary>
    /// Finds the preset that matches the current player customization and sets it as selected.
    /// </summary>
    internal static void FindCurrentPreset()
    {
        var allOutfitData = BetterDataManager.Files.BetterDataFile.OutfitData;
        for (int i = 0; i < allOutfitData.Count; i++)
        {
            var data = allOutfitData.ElementAt(i);
            if (data.HatId == DataManager.Player.Customization.Hat &&
                data.PetId == DataManager.Player.Customization.Pet &&
                data.SkinId == DataManager.Player.Customization.Skin &&
                data.VisorId == DataManager.Player.Customization.Visor &&
                data.NamePlateId == DataManager.Player.Customization.NamePlate)
            {
                BetterDataManager.Files.BetterDataFile.SelectedOutfitPreset = i;
                return;
            }
        }

        BetterDataManager.Files.BetterDataFile.SelectedOutfitPreset = 0;
    }

    /// <summary>
    /// Validates the outfit data by ensuring all IDs correspond to unlocked items.
    /// </summary>
    private void ValidateOutfit()
    {
        if (!HatManager.Instance.GetUnlockedHats().Any(item => item.ProductId == HatId))
            HatId = HatData.EmptyId;
        if (!HatManager.Instance.GetUnlockedPets().Any(item => item.ProductId == PetId))
            PetId = PetData.EmptyId;
        if (!HatManager.Instance.GetUnlockedSkins().Any(item => item.ProductId == SkinId))
            SkinId = SkinData.EmptyId;
        if (!HatManager.Instance.GetUnlockedVisors().Any(item => item.ProductId == VisorId))
            VisorId = VisorData.EmptyId;
        if (!HatManager.Instance.GetUnlockedNamePlates().Any(item => item.ProductId == NamePlateId))
            NamePlateId = NamePlateData.EmptyId;
    }

    /// <summary>
    /// Applies the outfit data to the player's customization, invoking a callback after loading.
    /// </summary>
    /// <param name="callback">The callback to invoke after loading the outfit.</param>
    internal void ApplyToCustomizationData(Action callback)
    {
        ValidateOutfit();

        ignoreChange = true;
        DataManager.Player.Customization.Hat = HatId;
        DataManager.Player.Customization.Pet = PetId;
        DataManager.Player.Customization.Skin = SkinId;
        DataManager.Player.Customization.Visor = VisorId;
        DataManager.Player.Customization.NamePlate = NamePlateId;
        ignoreChange = false;

        callback.Invoke();
        BetterDataManager.Files.BetterDataFile.Save();
    }

    /// <summary>
    /// Updates the outfit data from the player's current customization.
    /// </summary>
    internal void UpdateFromCustomizationData()
    {
        HatId = DataManager.Player.Customization.Hat;
        PetId = DataManager.Player.Customization.Pet;
        SkinId = DataManager.Player.Customization.Skin;
        VisorId = DataManager.Player.Customization.Visor;
        NamePlateId = DataManager.Player.Customization.NamePlate;
    }

    /// <summary>
    /// Converts to network player outfit.
    /// </summary>
    /// <returns></returns>
    internal NetworkedPlayerInfo.PlayerOutfit ToPlayerOutfit()
    {
        return new NetworkedPlayerInfo.PlayerOutfit
        {
            HatId = HatId,
            PetId = PetId,
            SkinId = SkinId,
            VisorId = VisorId,
            NamePlateId = NamePlateId,
            ColorId = DataManager.Player.Customization.Color
        };
    }
}