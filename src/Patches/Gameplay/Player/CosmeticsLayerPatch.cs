using BetterAmongUs.Utilities;
using BetterAmongUs.Modules.Support;
using HarmonyLib;

namespace BetterAmongUs.Patches.Gameplay.Player;

[HarmonyPatch]
internal static class CosmeticsLayerPatch
{
    [HarmonyPatch(typeof(CosmeticsLayer), nameof(CosmeticsLayer.GetColorBlindText))]
    [HarmonyPrefix]
    private static bool CosmeticsLayer_GetColorBlindText_Prefix(CosmeticsLayer __instance, ref string __result)
    {
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_CustomColorBlindText))
        {
            return true;
        }

        // Skip for custom colors not in vanilla palette
        if (__instance.bodyMatProperties.ColorId > Palette.PlayerColors.Length) return true;

        // Get color name from palette
        string colorName = Palette.GetColorName(__instance.bodyMatProperties.ColorId);

        if (!string.IsNullOrEmpty(colorName))
        {
            // Capitalize first letter, lowercase rest, and apply color formatting
            __result = (char.ToUpperInvariant(colorName[0]) + colorName[1..].ToLowerInvariant())
                .ToColor(Palette.PlayerColors[__instance.bodyMatProperties.ColorId]);
        }
        else
        {
            __result = string.Empty;
        }

        return false;
    }
}