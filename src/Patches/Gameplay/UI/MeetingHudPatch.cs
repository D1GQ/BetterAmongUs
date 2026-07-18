using BetterAmongUs.Generated;
using BetterAmongUs.Modules;
using BetterAmongUs.MonoScripts;
using BetterAmongUs.MonoScripts.Extended;
using BetterAmongUs.Patches.Gameplay.UI.Chat;
using BetterAmongUs.Utilities;
using HarmonyLib;
using Rewired;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Patches.Gameplay.UI;

[HarmonyPatch]
internal static class MeetingHudPatch
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    [HarmonyPostfix]
    private static void MeetingHud_Start_Postfix(MeetingHud __instance)
    {
        // Uncensor chats when meeting starts
        if (PlayerControl.LocalPlayer.IsAlive())
        {
            ChatPatch.UncensorPlayerChats();
        }

        if (__instance == null || __instance.playerStates == null)
            return;

        // Add meeting info display to each player state
        foreach (var pva in __instance.playerStates)
        {
            if (pva == null) continue;

            var target = Utils.PlayerFromPlayerId(pva.TargetPlayerId);
            if (target == null) continue;

            pva.gameObject.AddComponent<MeetingInfoDisplay>().Init(target, pva);
        }

        if (!GameState.IsOnlineGame)
            return;

        // Add host icon to meeting hud
        if (__instance.ProceedButton != null && __instance.HostIcon != null)
        {
            __instance.ProceedButton.gameObject.transform.localPosition = new(-2.5f, 2.2f, 0);
            __instance.ProceedButton.gameObject.GetComponent<SpriteRenderer>().enabled = false;
            __instance.ProceedButton.GetComponent<PassiveButton>().enabled = false;
            __instance.HostIcon.enabled = true;
            __instance.HostIcon.gameObject.SetActive(true);
            __instance.ProceedButton.gameObject.SetActive(true);
            UpdateHostIcon();
            MeetingHud.Instance.ProceedButton.DestroyTextTranslators();
        }

        Logger_.LogHeader("Meeting Has Started");
    }

    // Updates host icon with current host info
    internal static void UpdateHostIcon()
    {
        if (MeetingHud.Instance == null)
            return;

        if (GameData.Instance == null)
            return;

        var host = GameData.Instance.GetHost();
        if (host?.ExtendedData() == null)
            return;

        var hostColor = host.Color;
        var hostRealName = host.ExtendedData().RealName;

        if (MeetingHud.Instance.HostIcon == null || MeetingHud.Instance.ProceedButton == null)
            return;

        PlayerMaterial.SetColors(hostColor, MeetingHud.Instance.HostIcon);
        MeetingHud.Instance.ProceedButton.gameObject.GetComponentInChildren<TextMeshPro>().text = TranslationStrings.HostInMeeting.Format(hostRealName);
    }

    internal static float timeOpen = 0f;

    // Fix null exception
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    [HarmonyPrefix]
    private static bool MeetingHud_Update_Prefix(MeetingHud __instance)
    {
        if (__instance.playerStates == null || __instance.playerStates.Length == 0)
            return false;

        if (GameManager.Instance == null || GameManager.Instance.LogicOptions == null)
            return false;

        if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null)
            return false;

        for (int i = 0; i < __instance.playerStates.Length; i++)
        {
            if (__instance.playerStates[i] == null)
                return false;
        }

        try
        {
            if (ReInput.players == null || ReInput.players.GetPlayer(0) == null)
                return false;
        }
        catch
        {
            return false;
        }

        return true;
    }

    // Track how long meeting has been open
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    [HarmonyPostfix]
    private static void MeetingHud_Update_Postfix(MeetingHud __instance)
    {
        timeOpen += Time.deltaTime;
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
    [HarmonyPostfix]
    private static void MeetingHud_Close_Postfix()
    {
        // Censor chats when meeting ends
        if (PlayerControl.LocalPlayer.IsAlive())
        {
            ChatPatch.CensorPlayerChats();
        }

        timeOpen = 0f;
        Logger_.LogHeader("Meeting Has Ended");
    }
}