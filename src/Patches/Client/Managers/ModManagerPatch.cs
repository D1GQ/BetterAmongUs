using BetterAmongUs.Helpers;
using BetterAmongUs.Managers;
using BetterAmongUs.Modules.AntiCheat;
using BetterAmongUs.Modules.Support;
using HarmonyLib;
using UnityEngine;

namespace BetterAmongUs.Patches.Client.Managers;

[HarmonyPatch]
internal static class ModManagerPatch
{
    private static SpriteRenderer? modStamp;

    [HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
    [HarmonyPostfix]
    private static void LateUpdate_Postfix(ModManager __instance)
    {
        // Show the mod stamp only after the splash screen has fully loaded
        if (SplashIntroPatch.IsReallyDoneLoading)
        {
            __instance.ShowModStamp();
        }

        // Check if the mod stamp is currently visible in the UI
        if (__instance.ModStamp.gameObject.active == true)
        {
            // Cache the SpriteRenderer component on first access
            if (modStamp == null)
            {
                modStamp = __instance.ModStamp.GetComponent<SpriteRenderer>();
            }
            else
            {
                // Replace the mod stamp sprite with BAU's custom version
                // unless other mods have disabled custom mod stamps
                if (!BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_CustomModStamp))
                {
                    modStamp.sprite = Utils.LoadSprite("BetterAmongUs.Resources.Images.BetterAmongUs-Mod.png", 380f);
                }
            }
        }

        // Update various BAU systems each frame
        BetterAntiCheat.Update();
        LateTask.UpdateAll(Time.deltaTime);
        BetterNotificationManager.Update();
    }
}