using BetterAmongUs.Modules;
using BetterAmongUs.Modules.Support;
using HarmonyLib;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace BetterAmongUs.Patches.Unity;

[HarmonyPatch]
internal static class UnityWebRequestPatch
{
    // Build mod version header string
    public static string GetHeader()
    {
        var stringBuilder = new StringBuilder();

        // Format: "Version;BuildType;IsHotfix/HotfixNum/BetaNum"
        stringBuilder.Append(BAUPlugin.ModInfo.PLUGIN_VERSION);
        stringBuilder.Append(';');
        stringBuilder.Append(Enum.GetName(BAUPlugin.ModInfo.ReleaseBuildType));
        stringBuilder.Append(';');
        stringBuilder.Append(BAUPlugin.ModInfo.IS_HOTFIX);
        stringBuilder.Append('/');
        stringBuilder.Append(BAUPlugin.ModInfo.HOTFIX_NUM);
        stringBuilder.Append('/');
        stringBuilder.Append(BAUPlugin.ModInfo.BETA_NUM);

        return stringBuilder.ToString();
    }

    // Add custom header to game API requests
    [HarmonyPatch(typeof(UnityWebRequest), nameof(UnityWebRequest.SendWebRequest))]
    [HarmonyPrefix]
    private static void UnityWebRequest_SendWebRequest_Prefix(UnityWebRequest __instance)
    {
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_BAUHttpHeader))
            return;

        // Check if this is a game API request
        var path = new Uri(__instance.url).AbsolutePath;
        if (path.Contains("/api/games"))
        {
            // Add mod version header so server knows we're modded
            __instance.SetRequestHeader("BAU-Mod", GetHeader());
        }
    }

    // Check server response for mod support acknowledgment
    [HarmonyPatch(typeof(UnityWebRequest), nameof(UnityWebRequest.SendWebRequest))]
    [HarmonyPostfix]
    private static void UnityWebRequest_SendWebRequest_Postfix(UnityWebRequest __instance, UnityWebRequestAsyncOperation __result)
    {
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_BAUHttpHeader))
            return;

        var path = new Uri(__instance.url).AbsolutePath;
        if (path.Contains("/api/games"))
        {
            // Add callback when request completes
            __result.add_completed((Action<AsyncOperation>)(_ =>
            {
                if (!HttpUtils.IsSuccess(__instance.responseCode))
                    return;

                // Check if server supports BAU mod
                var responseHeader = __instance.GetResponseHeader("BAU-Mod-Processed");

                if (responseHeader != null)
                {
                    Logger_.Log("Connected to a supported Better Among Us matchmaking server");
                }
            }));
        }
    }
}