using BetterAmongUs.Managers;
using HarmonyLib;

namespace BetterAmongUs.Patches.Client;

[HarmonyPatch]
internal static class AudioOverridePatch
{
    [HarmonyPatch(typeof(SoundStarter), nameof(SoundStarter.Awake))]
    [HarmonyPrefix]
    private static void SoundStarter_Awake_Prefix(SoundStarter __instance)
    {
        if (__instance.gameObject.name == "MainMenuBgMusic")
        {
            if (AudioOverrideManager.Music_MainMenuSong.TryGetAudioClip(out var audio))
            {
                __instance.SoundToPlay = audio;
            }
        }
    }

    [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
    [HarmonyPrefix]
    private static void LobbyBehaviour_Start_Prefix(LobbyBehaviour __instance)
    {
        if (AudioOverrideManager.Music_LobbySong.TryGetAudioClip(out var audio))
        {
            __instance.MapTheme = audio;
        }
    }

    [HarmonyPatch(typeof(MeetingCalledAnimation), nameof(MeetingCalledAnimation.CoShow))]
    [HarmonyPrefix]
    private static void MeetingCalledAnimation_CoShow_Prefix(MeetingCalledAnimation __instance)
    {
        if (AudioOverrideManager.Sounds_EmergencyMeetingStart.TryGetAudioClip(out var audio))
        {
            __instance.Stinger = audio;
        }
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Awake))]
    [HarmonyPrefix]
    private static void ChatController_Awake_Prefix(ChatController __instance)
    {
        if (AudioOverrideManager.Sounds_ChatNotification.TryGetAudioClip(out var audio))
        {
            __instance.messageSound = audio;
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Awake))]
    [HarmonyPrefix]
    private static void MeetingHud_Awake_Prefix(MeetingHud __instance)
    {
        if (AudioOverrideManager.Sounds_EmergencyMeetingVote.TryGetAudioClip(out var vote))
        {
            __instance.VoteSound = vote;
        }

        if (AudioOverrideManager.Sounds_EmergencyMeetingVoteLockin.TryGetAudioClip(out var lockin))
        {
            __instance.VoteLockinSound = lockin;
        }

        if (AudioOverrideManager.Sounds_EmergencyMeetingEnding.TryGetAudioClip(out var end))
        {
            __instance.VoteEndingSound = end;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Awake))]
    [HarmonyPostfix]
    private static void PlayerControl_Awake_Postfix(PlayerControl __instance)
    {
        if (AudioOverrideManager.Sounds_Kill.TryGetAudioClip(out var audio))
        {
            __instance.KillSfx = audio;
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    [HarmonyPostfix]
    private static void ShipStatus_Awake_Postfix(ShipStatus __instance)
    {
        if (__instance.VentExitSound == null)
        {
            __instance.VentExitSound = __instance.VentEnterSound;
        }

        if (AudioOverrideManager.Sounds_VentEnter.TryGetAudioClip(out var ventEnter))
        {
            __instance.VentEnterSound = ventEnter;
        }

        if (AudioOverrideManager.Sounds_VentExit.TryGetAudioClip(out var ventExit))
        {
            __instance.VentExitSound = ventExit;
        }

        if (AudioOverrideManager.Sounds_SabotageAlert.TryGetAudioClip(out var sabo))
        {
            __instance.SabotageSound = sabo;
        }
    }
}
