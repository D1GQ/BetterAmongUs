using BetterAmongUs.Data.Config;
using BetterAmongUs.Helpers;
using BetterAmongUs.Managers;
using BetterAmongUs.Modules;
using BetterAmongUs.Modules.Support;
using BetterAmongUs.Mono;
using HarmonyLib;
using InnerNet;

namespace BetterAmongUs.Patches.Gameplay.Anticheat;

[HarmonyPatch]
internal class PlatformSpoofPatch
{
    [HarmonyPatch(typeof(PlatformSpecificData), nameof(PlatformSpecificData.Deserialize))]
    [HarmonyPostfix]
    internal static void PlatformSpecificData_Deserialize_Postfix(PlatformSpecificData __instance)
    {
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_Anticheat))
            return;

        if (!BAUConfigs.AntiCheat.Value || !GameState.IsVanillaServer)
            return;

        if (GameState.IsLobby)
        {
            try
            {
                LateTask.Schedule(() =>
                {
                    var player = BAUPlugin.AllPlayerControls.FirstOrDefault(pc => pc.GetClient().PlatformData == __instance);

                    if (player != null && __instance != null)
                    {
                        // Check Xbox/Windows store players for invalid platform ID length
                        if (__instance.Platform is Platforms.StandaloneWin10 or Platforms.Xbox)
                        {
                            if (__instance.XboxPlatformId.ToString().Length is < 10 or > 16)
                            {
                                // Invalid ID length, likely spoofing
                                player.ReportPlayer(ReportReasons.Cheating_Hacking);
                                BetterNotificationManager.NotifyCheat(player,
                                    Translator.GetString("AntiCheat.Reason.PlatformSpoofer"),
                                    Translator.GetString("AntiCheat.HasBeenDetectedWithCheat")
                                );
                                Logger_.LogCheat($"{player.BetterData().RealName} {Translator.GetString("AntiCheat.PlatformSpoofer")}: {__instance.XboxPlatformId}");
                            }
                        }

                        // Check Playstation players for invalid platform ID length
                        if (__instance.Platform is Platforms.Playstation)
                        {
                            if (__instance.PsnPlatformId.ToString().Length is < 14 or > 20)
                            {
                                // Invalid ID length, likely spoofing
                                player.ReportPlayer(ReportReasons.Cheating_Hacking);
                                BetterNotificationManager.NotifyCheat(player,
                                    Translator.GetString("AntiCheat.Reason.PlatformSpoofer"),
                                    Translator.GetString("AntiCheat.HasBeenDetectedWithCheat")
                                );
                                Logger_.LogCheat($"{player.BetterData().RealName} {Translator.GetString("AntiCheat.PlatformSpoofer")}: {__instance.PsnPlatformId}");
                            }
                        }

                        // Check for unknown or undefined platforms
                        if (__instance.Platform is Platforms.Unknown || !Enum.IsDefined(__instance.Platform))
                        {
                            BetterNotificationManager.NotifyCheat(player,
                                Translator.GetString("AntiCheat.Reason.PlatformSpoofer"),
                                Translator.GetString("AntiCheat.HasBeenDetectedWithCheat")
                            );
                        }
                    }

                }, 3.5f, shouldLog: false);
            }
            catch { }
        }
    }
}