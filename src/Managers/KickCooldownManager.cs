using System.Collections;
using BepInEx.Unity.IL2CPP.Utils;
using BetterAmongUs.Patches.Gameplay.UI.Settings;
using UnityEngine;

namespace BetterAmongUs.Managers;

/// <summary>
/// Manages the cooldown period between player kick actions.
/// This cooldown is necessary to avoid bans by Innersloth anti-cheat.
/// </summary>
internal static class KickCooldownManager
{
    private static float CooldownSeconds => BetterGameSettings.KickCooldown.GetFloat();

    private static float _lastTriggerTime;

    internal static bool IsReady() => Time.time - _lastTriggerTime > CooldownSeconds;
    internal static void Trigger() => _lastTriggerTime = Time.time;
    private static float TimeToNextAvailableTrigger() => CooldownSeconds - (Time.time - _lastTriggerTime);

    internal static void ScheduleAction(Action action)
    {
        AmongUsClient.Instance.StartCoroutine(RunScheduledAction(action));
    }

    private static IEnumerator RunScheduledAction(Action action)
    {
        while (!IsReady())
        {
            yield return new WaitForSeconds(TimeToNextAvailableTrigger());
        }
        Trigger();
        action();
    }
}