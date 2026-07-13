using BetterAmongUs.Attributes;
using BetterAmongUs.Interfaces;
using HarmonyLib;
using UnityEngine;

namespace BetterAmongUs.MonoScripts.Extended;

/// <summary>
/// Extends PlayerControl with additional functionality.
/// </summary>
[RegisterInIl2Cpp]
internal sealed class ExtendedPlayerControl : MonoBehaviour, IMonoExtension<PlayerControl>, IMonoExtensionPatcher<PlayerControl>
{
    public IMonoExtensionPatcher.TargetPatch Target => new(typeof(PlayerControl), nameof(PlayerControl.Awake));

    public void AddExtensionPatch(PlayerControl playerControl)
    {
        IMonoExtension.AddExtension<ExtendedPlayerControl>(playerControl);
    }

    public void OnExtensionAwake(PlayerControl playerControl)
    {
        playerControl.gameObject.AddComponent<PlayerInfoDisplay>().Init(playerControl);
    }

    /// <summary>
    /// Gets or sets the base PlayerControl instance.
    /// </summary>
    public PlayerControl? BaseMono { get; set; }

    public void OnDestroy()
    {
        IMonoExtension.TryRemoveExtension(this);
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
            if (pc.ExtendedPlayerControl() == null)
            {
                pc.gameObject.AddComponent<ExtendedPlayerControl>();
            }
        }
    }

    /// <summary>
    /// Gets the extended player control for a PlayerControl.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <returns>The ExtendedPlayerControl, or null if not found.</returns>
    internal static ExtendedPlayerControl? ExtendedPlayerControl(this PlayerControl player)
    {
        return IMonoExtension.GetExtension<ExtendedPlayerControl>(player);
    }

    /// <summary>
    /// Gets the extended player control for a PlayerPhysics.
    /// </summary>
    /// <param name="playerPhysics">The PlayerPhysics instance.</param>
    /// <returns>The ExtendedPlayerControl, or null if not found.</returns>
    internal static ExtendedPlayerControl? ExtendedPlayerControl(this PlayerPhysics playerPhysics)
    {
        return IMonoExtension.GetExtension<ExtendedPlayerControl>(playerPhysics.myPlayer);
    }
}