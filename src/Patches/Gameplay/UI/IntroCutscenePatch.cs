using BepInEx.Unity.IL2CPP.Utils;
using BetterAmongUs.Utilities;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace BetterAmongUs.Patches.Gameplay.UI;

[HarmonyPatch]
internal static class IntroCutscenePatch
{
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
    [HarmonyPostfix]
    private static void IntroCutscene_CoBegin_Postfix(IntroCutscene __instance)
    {
        // Start coroutine to customize intro after text appears
        __instance.StartCoroutine(CoWaitForShowRole(__instance));
    }

    // Waits for intro text to appear, then customizes colors to match player's role
    private static IEnumerator CoWaitForShowRole(IntroCutscene __instance)
    {
        // Wait until "You are" text is active
        while (!__instance.YouAreText.gameObject.active)
        {
            yield return null;
        }

        // Get role color for local player
        var introCutscene = __instance;
        Color RoleColor = Utils.HexToColor32(PlayerControl.LocalPlayer.Data.RoleType.GetRoleHex());

        // Hide vanilla team text
        introCutscene.ImpostorText.gameObject.SetActive(false);
        introCutscene.TeamTitle.gameObject.SetActive(false);

        // Apply role color to all intro elements
        introCutscene.BackgroundBar.material.color = RoleColor;
        introCutscene.BackgroundBar.transform.SetLocalZ(-15);
        introCutscene.transform.Find("BackgroundLayer").transform.SetLocalZ(-16);
        introCutscene.YouAreText.color = RoleColor;
        introCutscene.RoleText.color = RoleColor;
        introCutscene.RoleBlurbText.color = RoleColor;
    }
}