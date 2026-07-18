using HarmonyLib;
using System.Reflection;

namespace BetterAmongUs.Patches.Unity;

[HarmonyPatch]
internal static class StandaloneSteamPatch
{
    // Try to get the SteamAPI type from the game assembly
    private static readonly Type? _type = Type.GetType("Steamworks.SteamAPI, Assembly-CSharp-firstpass", false);

    // Only apply this patch if SteamAPI type exists (game has Steam integration)
    [HarmonyPrepare]
    private static bool Prepare()
    {
        return _type != null;
    }

    // Target the RestartAppIfNecessary method in SteamAPI
    [HarmonyTargetMethod]
    private static MethodBase TargetMethod()
    {
        return AccessTools.Method(_type, "RestartAppIfNecessary");
    }

    private static bool Prefix(out bool __result)
    {
        const string file = "steam_appid.txt";

        // Create steam_appid.txt with Among Us app ID if it doesn't exist
        // This allows the game to run without Steam running
        if (!File.Exists(file))
        {
            File.WriteAllText(file, "945360"); // Among Us Steam App ID
        }

        __result = false;
        return false;
    }
}