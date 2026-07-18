using BetterAmongUs.Modules;
using HarmonyLib;

namespace BetterAmongUs.Patches.Gameplay.Ship;

[HarmonyPatch]
internal static class ShipStatusPatch
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    [HarmonyPostfix]
    private static void ShipStatus_Awake_Postfix(ShipStatus __instance)
    {
        // Calculate vent groups when ship initializes
        VentGroups.CalculateAllVentGroups(__instance.AllVents);
    }
}