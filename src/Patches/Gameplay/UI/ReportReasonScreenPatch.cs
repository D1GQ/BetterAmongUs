using BetterAmongUs.Utilities;
using HarmonyLib;

namespace BetterAmongUs.Patches.Gameplay.UI;

[HarmonyPatch]
internal static class ReportReasonScreenPatch
{
    [HarmonyPatch(typeof(ReportReasonScreen), nameof(ReportReasonScreen.Show))]
    [HarmonyPrefix]
    private static void ReportReasonScreen_Show_Prefix(ref string playerName)
    {
        if (Utils.IsHtmlText(playerName))
        {
            string extractedText = playerName.Split(["<color=#ffea00>", "</color>"], StringSplitOptions.None)[1];
            playerName = extractedText;
        }
    }
}
