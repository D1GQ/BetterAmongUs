using BetterAmongUs.Data.Config;
using BetterAmongUs.Modules;
using BetterAmongUs.Modules.Support;
using HarmonyLib;
using UnityEngine;

namespace BetterAmongUs.Patches.Gameplay.Ship;

[HarmonyPatch]
internal static class VentPatch
{
    [HarmonyPatch(typeof(Vent), nameof(Vent.SetOutline))]
    [HarmonyPrefix]
    private static bool Vent_SetOutline_Prefix(Vent __instance, bool on, bool mainTarget)
    {
        // Skip if vent color groups are disabled - use vanilla behavior
        if (!BAUConfigs.VentColorGroups.Value || BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_VentColorGroups)) return true;

        // Get group color for this vent and apply outline
        Color color = VentGroups.GetVentGroupColor(__instance);
        __instance.myRend.material.SetFloat("_Outline", on ? 1f : 0f);
        __instance.myRend.material.SetColor("_OutlineColor", color);
        __instance.myRend.material.SetColor("_AddColor", mainTarget ? color : Color.clear);

        return false;
    }
}