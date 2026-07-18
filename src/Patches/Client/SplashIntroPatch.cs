using BetterAmongUs.Utilities;
using BetterAmongUs.Modules.Support;
using HarmonyLib;
using UnityEngine;

namespace BetterAmongUs.Patches.Client;

[HarmonyPatch]
internal static class SplashIntroPatch
{
    private const float MinIntroDuration = 3f;
    private const float AudioDestroyTime = 1.8f;

    internal static bool Skip = false;
    internal static bool BetterIntro = false;
    internal static bool IsReallyDoneLoading = false;

    private static GameObject? _betterLogo;

    [HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Start))]
    [HarmonyPrefix]
    private static void SplashManager_Start_Prefix(SplashManager __instance)
    {
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_CustomSplashIntro))
            return;

        // Reset all flags when splash screen starts
        Skip = false;
        BetterIntro = false;
        IsReallyDoneLoading = false;

        // Hide black overlay by moving it out of view
        __instance.logoAnimFinish.transform
            .Find("BlackOverlay").transform
            .SetLocalY(100f);
    }

    [HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Update))]
    [HarmonyPrefix]
    private static bool SplashManager_Update_Prefix(SplashManager __instance)
    {
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_CustomSplashIntro))
            return true;

        if (Skip)
        {
            CheckIfDone(__instance);
            return false;
        }

        HandleAudioDestruction(__instance);

        if (TryHandleSkipClick(__instance))
        {
            Skip = true;
            return false;
        }

        if (CanStartBetterIntro(__instance))
        {
            StartBetterIntro(__instance);
            return false;
        }

        CheckIfDone(__instance);
        return false;
    }

    private static void HandleAudioDestruction(SplashManager __instance)
    {
        if (!BetterIntro)
            return;

        if (Time.time - __instance.startTime > AudioDestroyTime)
        {
            UnityEngine.Object.Destroy(__instance.logoAnimFinish.GetComponent<AudioSource>());
        }
    }

    private static bool TryHandleSkipClick(SplashManager __instance)
    {
        if (!Input.GetKeyDown(KeyCode.Mouse0) && !Input.GetKeyDown(KeyCode.Mouse1))
            return false;

        return CheckIfDone(__instance, true);
    }

    private static bool CanStartBetterIntro(SplashManager __instance)
    {
        return __instance.doneLoadingRefdata &&
               !__instance.startedSceneLoad &&
               Time.time - __instance.startTime > __instance.minimumSecondsBeforeSceneChange;
    }

    private static void StartBetterIntro(SplashManager __instance)
    {
        if (BetterIntro)
            return;

        // Start BAU custom intro sequence
        __instance.startTime = Time.time;
        __instance.logoAnimFinish.gameObject.SetActive(false);
        __instance.logoAnimFinish.gameObject.SetActive(true);

        // Replace InnerSloth logo with BAU logo
        ReplaceLogo(__instance);

        // Show black overlay
        __instance.logoAnimFinish.transform
            .Find("BlackOverlay").transform
            .SetLocalY(0f);

        BetterIntro = true;
    }

    private static void ReplaceLogo(SplashManager __instance)
    {
        var innerLogo = __instance.logoAnimFinish.transform
            .Find("LogoRoot/ISLogo").gameObject;

        _betterLogo = UnityEngine.Object.Instantiate(innerLogo, innerLogo.transform.parent);
        innerLogo.DestroyObj();

        _betterLogo.name = "BetterLogo";
        _betterLogo.GetComponent<SpriteRenderer>().sprite =
            Utils.LoadSprite("BetterAmongUs.Resources.Images.BetterAmongUs-Logo.png", 150f);
    }

    private static bool CheckIfDone(SplashManager __instance, bool isSkip = false)
    {
        // Allow transition if intro played for minimum duration or user skipped
        var introComplete = (Time.time - __instance.startTime > MinIntroDuration && BetterIntro) ||
                           (isSkip && BetterIntro);

        if (!introComplete) return false;

        IsReallyDoneLoading = true;

        // Allow scene transition to proceed
        __instance.sceneChanger.AllowFinishLoadingScene();
        __instance.startedSceneLoad = true;
        __instance.loadingObject.SetActive(true);

        return true;
    }
}