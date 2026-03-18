using BepInEx.Unity.IL2CPP.Utils;
using BetterAmongUs.Modules;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using System.Collections;
using UnityEngine;

namespace BetterAmongUs.Mono;

/// <summary>
/// Extends PlayerControl with additional functionality.
/// </summary>
internal sealed class ExtendedPlayerControl : MonoBehaviour, IMonoExtension<PlayerControl>
{
    /// <summary>
    /// Gets or sets the base PlayerControl instance.
    /// </summary>
    public PlayerControl? BaseMono { get; set; }

    /// <summary>
    /// Gets the PlayerControl instance.
    /// </summary>
    internal PlayerControl? _Player => BaseMono;

    private void Awake()
    {
        if (!this.RegisterExtension())
            return;

        this.StartCoroutine(CoAddBetterData());
        _Player.gameObject.AddComponent<PlayerInfoDisplay>().Init(_Player);
    }

    /// <summary>
    /// Coroutine to add extended player data.
    /// </summary>
    /// <returns>IEnumerator for coroutine.</returns>
    [HideFromIl2Cpp]
    private IEnumerator CoAddBetterData()
    {
        while (_Player == null || _Player.Data == null)
        {
            yield return null;
        }

        TryCreateExtendedData(_Player.Data);
    }

    /// <summary>
    /// Attempts to create extended data for a player.
    /// </summary>
    /// <param name="data">The player data to extend.</param>
    internal static void TryCreateExtendedData(NetworkedPlayerInfo data)
    {
        if (data.BetterData() == null)
        {
            ExtendedPlayerInfo newBetterData = data.gameObject.AddComponent<ExtendedPlayerInfo>();
            newBetterData.SetInfo(data);
        }
    }

    internal readonly ValueTracker<bool> InVentTracker = new();
    private void Update()
    {
        if (_Player == null)
            return;

        InVentTracker.Update(_Player.inVent);
    }

    private void OnDestroy()
    {
        this.UnregisterExtension();
    }
}

/// <summary>
/// Extension methods for PlayerControl.
/// </summary>
internal static class PlayerControlExtension
{
    [HarmonyPatch(typeof(PlayerControl))]
    class PlayerControlPatch
    {
        [HarmonyPatch(nameof(PlayerControl.Awake))]
        [HarmonyPrefix]
        internal static void Awake_Prefix(PlayerControl __instance)
        {
            TryCreateExtendedPlayerControl(__instance);
        }

        /// <summary>
        /// Creates extended player control if it doesn't exist.
        /// </summary>
        /// <param name="pc">The PlayerControl instance.</param>
        internal static void TryCreateExtendedPlayerControl(PlayerControl pc)
        {
            if (pc.BetterPlayerControl() == null)
            {
                ExtendedPlayerControl newExtendedPc = pc.gameObject.AddComponent<ExtendedPlayerControl>();
            }
        }
    }

    /// <summary>
    /// Gets the extended player control for a PlayerControl.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <returns>The ExtendedPlayerControl, or null if not found.</returns>
    internal static ExtendedPlayerControl? BetterPlayerControl(this PlayerControl player)
    {
        return MonoExtensionManager.Get<ExtendedPlayerControl>(player);
    }

    /// <summary>
    /// Gets the extended player control for a PlayerPhysics.
    /// </summary>
    /// <param name="playerPhysics">The PlayerPhysics instance.</param>
    /// <returns>The ExtendedPlayerControl, or null if not found.</returns>
    internal static ExtendedPlayerControl? BetterPlayerControl(this PlayerPhysics playerPhysics)
    {
        return MonoExtensionManager.Get<ExtendedPlayerControl>(playerPhysics.myPlayer);
    }
}