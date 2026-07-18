using AmongUs.QuickChat;
using BetterAmongUs.Utilities;
using HarmonyLib;

namespace BetterAmongUs.Patches.Gameplay.UI.Chat;

[HarmonyPatch]
internal static class QuickChatPatch
{
    [HarmonyPatch(typeof(QuickChatMenu), nameof(QuickChatMenu.Awake))]
    [HarmonyPrefix]
    private static void QuickChatMenu_Awake_Prefix(QuickChatMenu __instance)
    {
        // Apply custom UI colors to close button
        __instance.closeButton?.gameObject?.SetUIColors("Icon");
    }

    [HarmonyPatch(typeof(QuickChatMenuLandingPage), nameof(QuickChatMenuLandingPage.Initialize))]
    [HarmonyPrefix]
    private static void QuickChatMenuLandingPage_Initialize_Prefix(QuickChatMenuLandingPage __instance)
    {
        // Apply custom UI colors to landing page buttons
        __instance.buttonTemplate?.Button?.gameObject?.SetUIColors("Icon");
        __instance.favoritesButton?.Button?.gameObject?.SetUIColors("Icon");
        __instance.remarksButton?.Button?.gameObject?.SetUIColors("Icon");
    }

    [HarmonyPatch(typeof(QuickChatMenuPhrasesPage), nameof(QuickChatMenuPhrasesPage.Awake))]
    [HarmonyPrefix]
    private static void QuickChatMenuPhrasesPage_Awake_Prefix(QuickChatMenuPhrasesPage __instance)
    {
        // Apply custom UI colors to crewmate and phrase buttons with multiple color layers
        __instance.crewmateButtonTemplate?.Button?.gameObject?.SetUIColors("Icon", "Background", "PlayerMask", "Skin", "Visor", "Back", "Front", "Normal", "Horse", "Seeker",
            "LongBoiBody", "LongHead", "LongNeck", "ForegroundNeck", "LongHands");
        __instance.phraseButtonTemplate?.Button?.gameObject?.SetUIColors("Icon");
    }
}