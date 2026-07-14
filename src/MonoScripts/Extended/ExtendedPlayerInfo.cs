using AmongUs.GameOptions;
using BetterAmongUs.Attributes;
using BetterAmongUs.Generated;
using BetterAmongUs.Interfaces;
using BetterAmongUs.Managers;
using BetterAmongUs.Modules;
using BetterAmongUs.Modules.Support;
using BetterAmongUs.Patches.Gameplay.UI.Settings;
using BetterAmongUs.Utilities;
using HarmonyLib;
using Hazel;
using Il2CppInterop.Runtime.Attributes;
using InnerNet;
using UnityEngine;

namespace BetterAmongUs.MonoScripts.Extended;

/// <summary>
/// Extended player information with additional data and anti-cheat features.
/// </summary>
[RegisterInIl2Cpp]
internal sealed class ExtendedPlayerInfo : MonoBehaviour, IMonoExtension<NetworkedPlayerInfo>
{
    internal ExtendedPlayerInfo()
    {
        try
        {
            HandshakeHandler = new HandshakeHandler(this);
        }
        catch (Exception ex)
        {
            Logger_.Log("Handshake disabled: " + ex.Message);
            HandshakeHandler = null;
        }
    }

    public NetworkedPlayerInfo? BaseMono { get; set; }

    public void OnExtensionAwake(NetworkedPlayerInfo networkedPlayerInfo)
    {
        if (HandshakeHandler != null)
        {
            HandshakeHandler.WaitSendSecretToPlayer();
        }
    }

    private float timeAccumulator = 0f;
    internal void Update()
    {
        var time = Time.deltaTime;

        AntiCheatInfo.TimeSinceLastTask += time;

        if (AntiCheatInfo.RPCSentPS > 0)
        {
            if (BetterGameSettings.RpcRateLimiting.GetBool())
            {
                bool flag = BaseMono.IsCheater();

                if (AntiCheatInfo.RPCSentPS >= BetterGameSettings.RpcRateLimit.GetInt() && !flag)
                {
                    BetterNotificationManager.NotifyCheat(
                        BaseMono.Object,
                        TranslationStrings.AntiCheat_Reason_RPCSentPS.LocalizedString,
                        TranslationStrings.AntiCheat_UnauthorizedAction.LocalizedString
                    );

                    Logger_.LogCheat($"{BaseMono.Object.ExtendedData().RealName} {AntiCheatInfo.RPCSentPS} Sent.");
                }
            }

            timeAccumulator += time;

            if (timeAccumulator >= 0.25f - 0.005 * AntiCheatInfo.RPCSentPS)
            {
                AntiCheatInfo.RPCSentPS -= 1;
                timeAccumulator = 0f;
            }
        }
    }

    public void OnDestroy()
    {
        IMonoExtension.TryRemoveExtension(this);
    }

    internal void Deserialize(MessageReader reader)
    {
        if (BaseMono.OwnerId != -2)
            return;

        if (reader.BytesRemaining > 0)
        {
            try
            {
                if (reader.ReadString() == ModInfo.Constants.BAU_MODDED_PROTOCOL_FLAG)
                {
                    int flagCount = reader.ReadPackedInt32();
                    for (int i = 0; i < flagCount; i++)
                    {
                        BAUModdedSupportFlags.AddTempFlag(reader.ReadInt32());
                    }
                }
            }
            catch { }
        }
    }

    /// <summary>
    /// Gets the handshake handler for this player.
    /// </summary>
    [HideFromIl2Cpp]
    internal HandshakeHandler? HandshakeHandler { get; }

    /// <summary>
    /// Gets the player's real name.
    /// </summary>
    internal string RealName => BaseMono?.PlayerName ?? "???";

    /// <summary>
    /// Gets or sets the last name set for this player.
    /// </summary>
    internal string NameSetAsLast { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this player is a BetterAmongUs user.
    /// </summary>
    internal bool IsBetterUser { get; set; } = false;

    /// <summary>
    /// Gets or sets whether this player is a verified BetterAmongUs user.
    /// </summary>
    internal bool IsVerifiedBetterUser { get; set; } = false;

    /// <summary>
    /// Gets or sets whether disconnect message has been shown.
    /// </summary>
    internal bool HasShowDcMsg { get; set; } = false;

    /// <summary>
    /// Gets or sets the disconnect reason.
    /// </summary>
    internal DisconnectReasons DisconnectReason { get; set; } = DisconnectReasons.Unknown;

    /// <summary>
    /// Gets the extended role information.
    /// </summary>
    [HideFromIl2Cpp]
    internal ExtendedRoleInfo? RoleInfo { get; } = new();

    /// <summary>
    /// Gets the extended anti-cheat information.
    /// </summary>
    [HideFromIl2Cpp]
    internal ExtendedAntiCheatInfo? AntiCheatInfo { get; } = new();
}

/// <summary>
/// Contains anti-cheat monitoring information for a player.
/// </summary>
internal sealed class ExtendedAntiCheatInfo
{
    /// <summary>
    /// Gets or sets whether the player is banned by anti-cheat.
    /// </summary>
    internal bool BannedByAntiCheat { get; set; } = false;

    /// <summary>
    /// Gets or sets the list of AUM chat messages.
    /// </summary>
    internal List<string> AUMChats { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of MCC chat messages.
    /// </summary>
    internal List<string> MCCChats { get; set; } = [];

    /// <summary>
    /// Gets or sets the RPCs sent per second.
    /// </summary>
    internal int RPCSentPS { get; set; } = 0;

    /// <summary>
    /// Gets or sets the number of times attempted to kill.
    /// </summary>
    internal int TimesAttemptedKilled { get; set; } = 0;

    /// <summary>
    /// Gets or sets the number of open sabotages.
    /// </summary>
    internal int OpenSabotageNum { get; set; } = 0;

    /// <summary>
    /// Gets whether the player is fixing panel sabotage.
    /// </summary>
    internal bool IsFixingPanelSabotage => OpenSabotageNum != 0;

    /// <summary>
    /// Gets or sets the time since last task.
    /// </summary>
    internal float TimeSinceLastTask { get; set; } = 5f;

    /// <summary>
    /// Gets or sets the last task ID.
    /// </summary>
    internal uint LastTaskId { get; set; } = 999;

    /// <summary>
    /// Gets or sets whether the player has set their name.
    /// </summary>
    internal bool HasSetName { get; set; }

    /// <summary>
    /// Gets or sets whether the player has set their level.
    /// </summary>
    internal bool HasSetLevel { get; set; }
}

/// <summary>
/// Contains extended role information for a player.
/// </summary>
internal sealed class ExtendedRoleInfo
{
    /// <summary>
    /// Gets or sets the number of kills.
    /// </summary>
    internal int Kills { get; set; } = 0;

    /// <summary>
    /// Gets or sets whether noisemaker notification is enabled.
    /// </summary>
    internal bool HasNoisemakerNotify { get; set; } = false;

    /// <summary>
    /// Gets or sets the role to display when dead.
    /// </summary>
    internal RoleTypes DeadDisplayRole { get; set; }
}

/// <summary>
/// Extension methods for accessing extended player data.
/// </summary>
internal static class PlayerControlDataExtension
{
    [HarmonyPatch(typeof(NetworkedPlayerInfo))]
    class NetworkedPlayerInfoPatch
    {
        [HarmonyPatch(nameof(NetworkedPlayerInfo.Deserialize))]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPostfix]
        internal static void Deserialize_Postfix(NetworkedPlayerInfo __instance, MessageReader reader)
        {
            __instance.ExtendedData()?.Deserialize(reader);
        }
    }

    /// <summary>
    /// Gets extended player data from a PlayerControl.
    /// </summary>
    /// <param name="player">The PlayerControl instance.</param>
    /// <returns>The ExtendedPlayerInfo, or null if not found.</returns>
    internal static ExtendedPlayerInfo? ExtendedData(this PlayerControl player)
    {
        return IMonoExtension.GetExtension<ExtendedPlayerInfo>(player.Data);
    }

    /// <summary>
    /// Gets extended player data from a NetworkedPlayerInfo.
    /// </summary>
    /// <param name="data">The NetworkedPlayerInfo instance.</param>
    /// <returns>The ExtendedPlayerInfo, or null if not found.</returns>
    internal static ExtendedPlayerInfo? ExtendedData(this NetworkedPlayerInfo data)
    {
        return IMonoExtension.GetExtension<ExtendedPlayerInfo>(data);
    }

    /// <summary>
    /// Gets extended player data from a ClientData.
    /// </summary>
    /// <param name="data">The ClientData instance.</param>
    /// <returns>The ExtendedPlayerInfo, or null if not found.</returns>
    internal static ExtendedPlayerInfo? ExtendedData(this ClientData data)
    {
        var player = Utils.PlayerFromClientId(data.Id);
        return IMonoExtension.GetExtension<ExtendedPlayerInfo>(player.Data);
    }
}