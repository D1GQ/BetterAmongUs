using BepInEx.Unity.IL2CPP.Utils;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using BetterAmongUs.Data.Config;
using BetterAmongUs.Generated;
using BetterAmongUs.Managers;
using BetterAmongUs.Modules;
using BetterAmongUs.Modules.OptionItems;
using BetterAmongUs.Modules.Support;
using BetterAmongUs.MonoScripts.Extended;
using BetterAmongUs.Patches.Gameplay.UI.Chat;
using BetterAmongUs.Utilities;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace BetterAmongUs.Patches.Gameplay.Player;

[HarmonyPatch]
internal static class PlayerControlPatch
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
    [HarmonyPostfix]
    private static void PlayerControl_Start_Postfix(PlayerControl __instance, ref Il2CppSystem.Collections.IEnumerator __result)
    {
        // Add player to global player list
        BAUPlugin.AllPlayerControls.Add(__instance);

        // Update option UI values for all players
        OptionPlayerItem.UpdateAllValues();

        // Append favorite color setting to player initialization coroutine
        __result = Effects.Sequence(__result, CoStartPostfix(__instance).WrapToIl2Cpp());
    }

    private static IEnumerator CoStartPostfix(PlayerControl player)
    {
        if (GameState.IsLobby)
        {
            if (player.AmOwner)
            {
                // Apply player's favorite color setting if they own this character
                if (!BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_FavoriteColor))
                {
                    if (BAUConfigs.FavoriteColor.Value >= 0 && player.cosmetics.ColorId != (byte)BAUConfigs.FavoriteColor.Value)
                    {
                        // Send command to server to change color
                        player.CmdCheckColor((byte)BAUConfigs.FavoriteColor.Value);
                    }
                }

                if (GameState.IsModdedProtocol)
                {
                    BetterNotificationManager.Notify(TranslationStrings.AntiCheat_DisabledModdedProtocol.LocalizedString, 6f, true);
                }
            }
        }

        yield break;
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.OnDestroy))]
    [HarmonyPostfix]
    private static void PlayerControl_OnDestroy_Postfix(PlayerControl __instance)
    {
        // Remove player from global list when destroyed
        BAUPlugin.AllPlayerControls.Remove(__instance);

        // Update option UI values
        OptionPlayerItem.UpdateAllValues();
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Die))]
    [HarmonyPostfix]
    private static void PlayerControl_Die_Postfix(PlayerControl __instance)
    {
        if (__instance.IsLocalPlayer())
        {
            ChatPatch.UncensorPlayerChats();
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    [HarmonyPostfix]
    private static void PlayerControl_MurderPlayer_Postfix(PlayerControl __instance, PlayerControl target, MurderResultFlags resultFlags)
    {
        // Check for null references
        if (__instance == null || target == null || target.Data == null || __instance.Data == null)
            return;

        // Log kill event with player names and roles
        Logger_.LogPrivate($"{__instance.Data.PlayerName} Has killed {target.Data.PlayerName} as {__instance.Data.RoleType.GetRoleName()}", "EventLog");

        // Track kill count in player's BetterData
        if (resultFlags.HasFlag(MurderResultFlags.Succeeded) || resultFlags.HasFlag(MurderResultFlags.DecisionByHost))
        {
            __instance.ExtendedData().RoleInfo.Kills += 1;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
    [HarmonyPostfix]
    private static void PlayerControl_Shapeshift_Postfix(PlayerControl __instance, PlayerControl targetPlayer, bool animate)
    {
        if (targetPlayer == null)
            return;

        if (!BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_CustomColorBlindText))
        {
            if (targetPlayer.Data.PlayerId == __instance.Data.PlayerId)
            {
                if (animate)
                {
                    // SetColor early so color blind text doesn't reveal previous color during animation
                    __instance.StartCoroutine(CoSetColorEarly(__instance));
                }
            }
        }

        // Log shapeshift events (both shifting and unshifting)
        if (__instance != targetPlayer)
            Logger_.LogPrivate($"{__instance.Data.PlayerName} Has Shapeshifted into {targetPlayer.Data.PlayerName}, did animate: {animate}", "EventLog");
        else
            Logger_.LogPrivate($"{__instance.Data.PlayerName} Has Un-Shapeshifted, did animate: {animate}", "EventLog");
    }

    private static IEnumerator CoSetColorEarly(PlayerControl __instance)
    {
        yield return new WaitForSeconds(0.3f);
        __instance.cosmetics.SetColor(__instance.Data.Outfits[PlayerOutfitType.Default].ColorId);
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetRoleInvisibility))]
    [HarmonyPostfix]
    private static void PlayerControl_SetRoleInvisibility_Postfix(PlayerControl __instance, bool isActive, bool shouldAnimate)
    {
        // Log Phantom role visibility changes
        if (isActive)
            Logger_.LogPrivate($"{__instance.Data.PlayerName} Has Vanished as Phantom, did animate: {shouldAnimate}", "EventLog");
        else
            Logger_.LogPrivate($"{__instance.Data.PlayerName} Has Appeared as Phantom, did animate: {shouldAnimate}", "EventLog");
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetName))]
    [HarmonyPostfix]
    private static void PlayerControl_SetName_Postfix(PlayerControl __instance, string playerName)
    {
        // Store the last set name in player's BetterData
        __instance.StartCoroutine(CoSetLastName(__instance, playerName));
    }

    private static IEnumerator CoSetLastName(PlayerControl player, string playerName)
    {
        while (player.Data == null || player.Data.ExtendedData() == null)
        {
            yield return null;
        }

        player.ExtendedData().NameSetAsLast = playerName;
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.BootFromVent))]
    [HarmonyPostfix]
    private static void PlayerPhysics_BootFromVent_Postfix(PlayerPhysics __instance, int ventId)
    {
        // Log vent boot events (when engineer boots someone)
        Logger_.LogPrivate($"{__instance.myPlayer.Data.PlayerName} Has been booted from vent: {ventId}, as {__instance.myPlayer.Data.RoleType.GetRoleName()}", "EventLog");
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoEnterVent))]
    [HarmonyPostfix]
    private static void PlayerPhysics_CoEnterVent_Postfix(PlayerPhysics __instance, int id)
    {
        // Log vent entry events
        Logger_.LogPrivate($"{__instance.myPlayer.Data.PlayerName} Has entered vent: {id}, as {__instance.myPlayer.Data.RoleType.GetRoleName()}", "EventLog");
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoExitVent))]
    [HarmonyPostfix]
    private static void PlayerPhysics_CoExitVent_Postfix(PlayerPhysics __instance, int id)
    {
        // Log vent exit events
        Logger_.LogPrivate($"{__instance.myPlayer.Data.PlayerName} Has exit vent: {id}, as {__instance.myPlayer.Data.RoleType.GetRoleName()}", "EventLog");
    }
}