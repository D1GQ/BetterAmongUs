using HarmonyLib;
using UnityEngine;

namespace BetterAmongUs.Patches.Gameplay.UI;

[HarmonyPatch]
internal static class RoomTrackerPatch
{
    [HarmonyPatch(typeof(RoomTracker), nameof(RoomTracker.Awake))]
    [HarmonyPostfix]
    private static void RoomTracker_Awake_Postfix(RoomTracker __instance)
    {
        // Fix aspect positioning
        var originalParent = __instance.transform.parent;

        var holder = new GameObject("RoomTrackerHolder");
        holder.transform.SetParent(originalParent);

        __instance.transform.SetParent(holder.transform);

        var aspectPosition = holder.AddComponent<AspectPosition>();
        aspectPosition.Alignment = AspectPosition.EdgeAlignments.Bottom;
        aspectPosition.DistanceFromEdge = new Vector3(0f, 3f, 0f);
        aspectPosition.updateAlways = true;
    }
}
