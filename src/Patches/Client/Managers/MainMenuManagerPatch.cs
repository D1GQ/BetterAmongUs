using BetterAmongUs.Utilities;
using BetterAmongUs.Managers;
using BetterAmongUs.Modules.Support;
using HarmonyLib;
using UnityEngine;

namespace BetterAmongUs.Patches.Client.Managers;

[HarmonyPatch]
internal static class MainMenuManagerPatch
{
    internal static PassiveButton? ButtonPrefab;

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.LateUpdate))]
    [HarmonyPostfix]
    private static void MainMenuManager_LateUpdate_Postfix(MainMenuManager __instance)
    {
        // Create list of all main menu buttons to recolor
        List<PassiveButton> buttons = [__instance.playButton, __instance.inventoryButton, __instance.shopButton, __instance.playLocalButton, __instance.PlayOnlineButton, __instance.backButtonOnline,
            __instance.newsButton, __instance.myAccountButton, __instance.settingsButton, __instance.howToPlayButton, __instance.freePlayButton, __instance.accountCTAButton, __instance.accountStatsButton];

        // Apply custom UI colors to each button's icon and background
        foreach (var button in buttons)
        {
            button.gameObject?.SetUIColors(sprite =>
            {
                // Only recolor white sprites to preserve original color variations
                return sprite.color == Color.white;
            },
            "Icon", "Background");
        }
    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    [HarmonyPostfix]
    private static void MainMenuManager_Start_Postfix(MainMenuManager __instance)
    {
        // Check if other mods haven't disabled the BAU logo replacement
        if (!BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_BAULogo))
        {
            // Find the original Among Us logo
            GameObject logo = GameObject.Find("LeftPanel/Sizer/LOGO-AU");
            GameObject sizer = logo.transform.parent.gameObject;

            // Adjust logo position downward and move it forward in Z-axis
            sizer.transform.localPosition = new Vector3(sizer.transform.localPosition.x, sizer.transform.localPosition.y - 0.035f, sizer.transform.localPosition.z);
            sizer.transform.position = new Vector3(sizer.transform.position.x, sizer.transform.position.y, -0.5f);

            // Scale down the logo slightly
            logo.transform.localScale = new Vector3(0.003f, 0.0025f, 0f);

            // Replace Among Us sprite with Better Among Us logo
            logo.GetComponent<SpriteRenderer>().sprite = Utils.LoadSprite("BetterAmongUs.Resources.Images.BetterAmongUs-Logo.png", 1f);
        }

        // Apply custom colors to main menu background
        __instance.transform.Find("MainUI/AspectScaler/BackgroundTexture")?.gameObject?.SetSpriteColors(sprite => GameObjectUtils.AddColor(sprite));

        // Create a reusable button prefab if it doesn't exist yet
        if (ButtonPrefab == null)
        {
            // Clone inventory button as template for custom UI elements
            ButtonPrefab = UnityEngine.Object.Instantiate(__instance.inventoryButton);
            ButtonPrefab.gameObject.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(ButtonPrefab);
        }

        // Notify UpdateManager that we're in the main menu
        BAUUpdateManager.Instance?.OnMainMenu();
    }

    // Disable eject button to prevent it from blocking update button
    [HarmonyPatch(typeof(EjectMainMenu), nameof(EjectMainMenu.Start))]
    [HarmonyPrefix]
    private static bool EjectMainMenu_Start_Prefix(EjectMainMenu __instance)
    {
        __instance.gameObject.SetActive(false);
        return false;
    }
}