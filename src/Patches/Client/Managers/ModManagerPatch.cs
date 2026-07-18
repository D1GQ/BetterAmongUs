using BetterAmongUs.Managers;
using BetterAmongUs.Modules.AntiCheat;
using BetterAmongUs.Modules.Support;
using BetterAmongUs.Network;
using BetterAmongUs.Utilities;
using HarmonyLib;
using UnityEngine;

namespace BetterAmongUs.Patches.Client.Managers;

[HarmonyPatch]
internal static class ModManagerPatch
{
    private static SpriteRenderer? modStamp;
    private static SpriteRenderer? DownloadIcon;
    private static float _alpha = 1;

    [HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
    [HarmonyPostfix]
    private static void LateUpdate_Postfix(ModManager __instance)
    {
        if (DownloadIcon == null)
        {
            DownloadIcon = UnityEngine.Object.Instantiate(__instance.ModStamp, __instance.transform);
            DownloadIcon.sprite = Utils.LoadSprite($"BetterAmongUs.Resources.Images.Icons.Downloading.png", 250);
            DownloadIcon.name = "DownloadIcon";
        }
        else
        {
            DownloadIcon.transform.localPosition = __instance.ModStamp.transform.localPosition + new Vector3(-0.7f, 0f, 1f);
            DownloadIcon.enabled = GithubAPI.Downloading;
            if (GithubAPI.Downloading)
            {
                _alpha = Mathf.PingPong(Time.time * 0.5f, 0.5f) + 0.25f;
                DownloadIcon.color = new Color(DownloadIcon.color.r, DownloadIcon.color.g, DownloadIcon.color.b, _alpha);
            }
        }

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
                    modStamp.sprite = Utils.LoadSprite("BetterAmongUs.Resources.Images.BetterAmongUs-Mod.png", 250f);
                }
            }
        }

        // Update various BAU systems each frame
        BetterAntiCheat.Update();
        BetterNotificationManager.Update();
    }
}