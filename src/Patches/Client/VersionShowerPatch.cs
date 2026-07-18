using BetterAmongUs.Generated;
using BetterAmongUs.Utilities;
using HarmonyLib;

namespace BetterAmongUs.Patches.Client;

[HarmonyPatch]
internal static class VersionShowerPatch
{
    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    [HarmonyPostfix]
    private static void VersionShower_Start_Postfix(VersionShower __instance)
    {
        string mark = TranslationStrings.BAUMark.LocalizedString;
        string bau = TranslationStrings.BAU.LocalizedString;
        __instance.text.text = $"<color=#0dff00>{mark}{bau}{mark} {BAUPlugin.GetVersionText()}</color> <color=#ababab>~</color> {Utils.GetPlatformName(BAUPlugin.PlatformData.Platform)} v{BAUPlugin.AmongUsVersion} ({BAUPlugin.AppVersion})";
    }
}
