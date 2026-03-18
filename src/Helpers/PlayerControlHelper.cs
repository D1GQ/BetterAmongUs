using AmongUs.GameOptions;
using BetterAmongUs.Data;
using BetterAmongUs.Modules;
using BetterAmongUs.Mono;
using BetterAmongUs.Patches.Gameplay.Player;
using BetterAmongUs.Patches.Gameplay.UI.Settings;
using InnerNet;
using UnityEngine;

namespace BetterAmongUs.Helpers;

/// <summary>
/// Provides extension methods and utilities for working with PlayerControl instances.
/// </summary>
internal static class PlayerControlHelper
{
    /// <summary>
    /// Gets the ClientData associated with a player.
    /// </summary>
    /// <param name="player">The player to get client data for.</param>
    /// <returns>The ClientData if found, null otherwise.</returns>
    internal static ClientData? GetClient(this PlayerControl player)
    {
        if (AmongUsClient.Instance == null || player == null)
            return null;

        foreach (var client in AmongUsClient.Instance.allClients)
        {
            if (client == null) continue;
            if (client.Character == null) continue;

            if (client.Character.PlayerId == player.PlayerId)
                return client;
        }
        return null;
    }

    /// <summary>
    /// Gets the client ID of a player.
    /// </summary>
    /// <param name="player">The player to get the client ID for.</param>
    /// <returns>The client ID, or -1 if not found.</returns>
    internal static int GetClientId(this PlayerControl player)
    {
        if (player == null)
            return -1;

        var client = player.GetClient();
        if (client == null)
            return -1;

        return client.Id;
    }

    /// <summary>
    /// Gets the player's name with color formatting based on their outfit color.
    /// </summary>
    /// <param name="player">The player to get the name for.</param>
    /// <returns>The colored player name string.</returns>
    internal static string GetPlayerNameAndColor(this PlayerControl player)
    {
        if (player == null)
            return string.Empty;

        if (player.Data == null)
            return string.Empty;

        try
        {
            return $"<color={Utils.Color32ToHex(Palette.PlayerColors[player.Data.DefaultOutfit.ColorId])}>{player.Data.PlayerName}</color>";
        }
        catch
        {
            return player.Data.PlayerName;
        }
    }

    /// <summary>
    /// Checks if a player's character data has been fully loaded and received from the host.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if player data is complete, false otherwise.</returns>
    internal static bool DataIsCollected(this PlayerControl player)
    {
        if (player == null) return false;

        if (player.isDummy || GameState.IsLocalGame)
        {
            return true;
        }

        if (player.cosmetics == null)
            return false;

        if (player.cosmetics.nameText == null)
            return false;

        string loading = Translator.GetString("Player.Loading");
        string nameText = player.cosmetics.nameText.text;

        if (nameText == "???" || nameText == "Player" || nameText == loading ||
            string.IsNullOrEmpty(nameText) ||
            player.Data == null ||
            player.CurrentOutfit == null ||
            player.CurrentOutfit.ColorId == -1)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Kicks a player from the game with optional ban and anti-cheat features.
    /// </summary>
    /// <param name="player">The player to kick.</param>
    /// <param name="ban">Whether to ban the player.</param>
    /// <param name="setReasonInfo">Custom reason message for the kick.</param>
    /// <param name="AntiCheatBan">Whether this is an anti-cheat related kick.</param>
    /// <param name="bypassDataCheck">Whether to bypass the data collection check.</param>
    /// <param name="forceBan">Whether to force a ban regardless of settings.</param>
    internal static void Kick(this PlayerControl player, bool ban = false, string setReasonInfo = "", bool AntiCheatBan = false, bool bypassDataCheck = false, bool forceBan = false)
    {
        var Ban = ban || forceBan;

        if (!GameState.IsHost || player.IsLocalPlayer() || (!player.DataIsCollected() && !bypassDataCheck) || player.IsHost() || player.isDummy)
        {
            return;
        }

        if (AntiCheatBan)
        {
            if (BetterGameSettings.WhenCheating.GetStringValue() == 0 && !forceBan)
            {
                return;
            }

            Ban = (Ban && BetterGameSettings.WhenCheating.GetStringValue() == 2) || forceBan;
        }

        if (setReasonInfo != "")
        {
            PlayerJoinAndLeftPatch.BetterShowNotification(player.Data, forceReasonText: string.Format(setReasonInfo, Ban ? Translator.GetString("AntiCheat.Ban").ToLower() : Translator.GetString("AntiCheat.Kick").ToLower()));
        }

        AmongUsClient.Instance.KickPlayer(player.GetClientId(), Ban);

        player.BetterData().AntiCheatInfo.BannedByAntiCheat = AntiCheatBan;
    }

    /// <summary>
    /// Sets the outline effect on a player's character.
    /// </summary>
    /// <param name="player">The player to modify.</param>
    /// <param name="active">Whether to enable the outline.</param>
    /// <param name="color">The outline color (optional).</param>
    internal static void SetOutline(this PlayerControl player, bool active, Color? color = null)
    {
        player.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", active ? 1 : 0);
        SpriteRenderer[] longModeParts = player.cosmetics.currentBodySprite.LongModeParts;
        for (int i = 0; i < longModeParts.Length; i++)
        {
            longModeParts[i].material.SetFloat("_Outline", active ? 1 : 0);
        }
        if (color != null)
        {
            player.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", color.Value);
            longModeParts = player.cosmetics.currentBodySprite.LongModeParts;
            for (int i = 0; i < longModeParts.Length; i++)
            {
                longModeParts[i].material.SetColor("_OutlineColor", color.Value);
            }
        }
    }

    /// <summary>
    /// Sets the outline effect on a player's character using a hex color string.
    /// </summary>
    /// <param name="player">The player to modify.</param>
    /// <param name="active">Whether to enable the outline.</param>
    /// <param name="hexColor">The hex color string for the outline.</param>
    internal static void SetOutlineByHex(this PlayerControl player, bool active, string hexColor = "")
    {
        Color color = Utils.HexToColor32(hexColor);
        player.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", active ? 1 : 0);
        SpriteRenderer[] longModeParts = player.cosmetics.currentBodySprite.LongModeParts;
        for (int i = 0; i < longModeParts.Length; i++)
        {
            longModeParts[i].material.SetFloat("_Outline", active ? 1 : 0);
        }

        player.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", color);
        longModeParts = player.cosmetics.currentBodySprite.LongModeParts;
        for (int i = 0; i < longModeParts.Length; i++)
        {
            longModeParts[i].material.SetColor("_OutlineColor", color);
        }
    }

    /// <summary>
    /// Checks if a player is the local player.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player is the local player.</returns>
    internal static bool IsLocalPlayer(this PlayerControl player) => player != null && PlayerControl.LocalPlayer != null && player == PlayerControl.LocalPlayer;

    /// <summary>
    /// Gets the ID of the vent the player is currently in.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>The vent ID, or -1 if not in a vent.</returns>
    internal static int GetPlayerVentId(this PlayerControl player)
    {
        if (player == null)
            return -1;

        if (!(ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Ventilation, out var systemType) &&
              systemType.TryCast<VentilationSystem>() is VentilationSystem ventilationSystem))
            return 0;

        return ventilationSystem.PlayersInsideVents.TryGetValue(player.PlayerId, out var playerIdVentId) ? playerIdVentId : -1;
    }

    /// <summary>
    /// Gets the custom position of a player.
    /// </summary>
    /// <param name="player">The player to get the position for.</param>
    /// <returns>The player's position as a Vector2.</returns>
    internal static Vector2 GetCustomPosition(this PlayerControl player) =>
        player.transform.position;

    /// <summary>
    /// Checks if a player is alive.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player is alive.</returns>
    internal static bool IsAlive(this PlayerControl player)
    {
        if (player == null)
            return false;

        var data = player.Data;
        return data != null && !data.IsDead;
    }

    /// <summary>
    /// Checks if a player is in a vent.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player is in a vent.</returns>
    internal static bool IsInVent(this PlayerControl player)
    {
        if (player == null)
            return false;

        if (player.inVent || player.walkingToVent)
            return true;

        var physics = player.MyPhysics;
        if (physics == null)
            return false;

        var animations = physics.Animations;
        if (animations == null)
            return false;

        return animations.IsPlayingEnterVentAnimation();
    }

    /// <summary>
    /// Gets the role name of a player.
    /// </summary>
    /// <param name="player">The player to get the role name for.</param>
    /// <returns>The role name string.</returns>
    internal static string GetRoleName(this PlayerControl player)
    {
        if (player == null)
            return string.Empty;

        if (!player.IsAlive() && !player.IsGhostRole())
        {
            return player.BetterData().RoleInfo.DeadDisplayRole.GetRoleName();
        }

        if (player.Data != null)
        {
            return player.Data.RoleType.GetRoleName();
        }

        return string.Empty;
    }

    /// <summary>
    /// Checks if a player is currently shapeshifting.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player is shapeshifting.</returns>
    internal static bool IsInShapeshift(this PlayerControl player) => player != null && (player.shapeshiftTargetPlayerId > -1 || player.shapeshifting) && !player.waitingForShapeshiftResponse;

    /// <summary>
    /// Checks if a player is in vanish mode as a Phantom.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player is in vanish mode.</returns>
    internal static bool IsInVanish(this PlayerControl player)
    {
        if (player != null && player.Data.Role is PhantomRole phantomRole)
        {
            return phantomRole.fading;
        }
        return false;
    }

    /// <summary>
    /// Gets the hex color code for a player's team.
    /// </summary>
    /// <param name="player">The player to get team color for.</param>
    /// <returns>The hex color string.</returns>
    internal static string GetTeamHexColor(this PlayerControl player) => player.Data.GetTeamHexColor();

    /// <summary>
    /// Gets the hex color code for a player data's team.
    /// </summary>
    /// <param name="data">The player data to get team color for.</param>
    /// <returns>The hex color string.</returns>
    internal static string GetTeamHexColor(this NetworkedPlayerInfo data)
    {
        if (data == null) return "#ffffff";

        if (data.IsImpostorTeam())
        {
            return Colors.ImpostorRed.ColorToHex();
        }
        else
        {
            return Colors.CrewmateBlue.ColorToHex();
        }
    }

    /// <summary>
    /// Checks if a player has a specific role type.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="role">The role type to check for.</param>
    /// <returns>True if the player has the specified role.</returns>
    internal static bool Is(this PlayerControl player, RoleTypes role)
    {
        if (player == null)
            return false;

        var data = player.Data;
        if (data == null)
            return false;

        return data.RoleType == role;
    }

    /// <summary>
    /// Checks if a player has a ghost role.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player has a ghost role.</returns>
    internal static bool IsGhostRole(this PlayerControl player)
    {
        if (player == null)
            return false;

        var data = player.Data;
        if (data == null)
            return false;

        return data.RoleType == RoleTypes.GuardianAngel;
    }

    /// <summary>
    /// Checks if a player is on the impostor team.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player is on the impostor team.</returns>
    internal static bool IsImpostorTeam(this PlayerControl player)
    {
        if (player == null)
            return false;

        var data = player.Data;
        if (data == null)
            return false;

        return data.IsImpostorTeam();
    }

    /// <summary>
    /// Checks if player data indicates the player is on the impostor team.
    /// </summary>
    /// <param name="data">The player data to check.</param>
    /// <returns>True if the player is on the impostor team.</returns>
    internal static bool IsImpostorTeam(this NetworkedPlayerInfo data)
    {
        if (data == null)
            return false;

        var roleBehaviour = data.RoleType.GetBehaviourPrefab();
        return roleBehaviour != null && roleBehaviour.IsImpostor;
    }

    /// <summary>
    /// Checks if a player is an impostor teammate of the local player.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player is an impostor teammate.</returns>
    internal static bool IsImpostorTeammate(this PlayerControl player)
    {
        if (player == null || PlayerControl.LocalPlayer == null)
            return false;

        bool localIsImpostor = PlayerControl.LocalPlayer.IsImpostorTeam();
        bool playerIsImpostor = player.IsImpostorTeam();

        return (player.IsLocalPlayer() && localIsImpostor) ||
               (localIsImpostor && playerIsImpostor);
    }

    /// <summary>
    /// Checks if a player is listed as a cheater in the anti-cheat database.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player is listed as a cheater.</returns>
    internal static bool IsCheater(this PlayerControl player)
    {
        if (player == null)
            return false;

        var betterDataFile = BetterDataManager.BetterDataFile;
        if (betterDataFile == null)
            return false;

        return betterDataFile.CheckPlayerData(player.Data) == true;
    }

    /// <summary>
    /// Checks if player data is listed as a cheater in the anti-cheat database.
    /// </summary>
    /// <param name="data">The player data to check.</param>
    /// <returns>True if the player data is listed as a cheater.</returns>
    internal static bool IsCheater(this NetworkedPlayerInfo data)
    {
        if (data == null)
            return false;

        var betterDataFile = BetterDataManager.BetterDataFile;
        if (betterDataFile == null)
            return false;

        return betterDataFile.CheckPlayerData(data) == true;
    }

    /// <summary>
    /// Checks if a player is the game host.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player is the host.</returns>
    internal static bool IsHost(this PlayerControl player)
    {
        if (player == null)
            return false;

        if (player.Data == null)
            return false;

        if (GameData.Instance == null)
            return false;

        return GameData.Instance.GetHost() == player.Data;
    }

    /// <summary>
    /// Gets the hashed PUID of a player.
    /// </summary>
    /// <param name="player">The player to get the hashed PUID for.</param>
    /// <returns>The hashed PUID string.</returns>
    internal static string GetHashPuid(this PlayerControl player)
    {
        return player.Data.GetHashPuid() ?? "";
    }

    /// <summary>
    /// Gets the hashed PUID from player data.
    /// </summary>
    /// <param name="data">The player data to get the hashed PUID for.</param>
    /// <returns>The hashed PUID string.</returns>
    internal static string GetHashPuid(this NetworkedPlayerInfo data)
    {
        if (data == null)
            return string.Empty;

        if (data.Puid == null)
            return string.Empty;

        return Utils.GetHashStr(data.Puid);
    }

    /// <summary>
    /// Gets the hashed friend code of a player.
    /// </summary>
    /// <param name="player">The player to get the hashed friend code for.</param>
    /// <returns>The hashed friend code string.</returns>
    internal static string GetHashFriendcode(this PlayerControl player)
    {
        return player.Data.GetHashFriendcode() ?? "";
    }

    /// <summary>
    /// Gets the hashed friend code from player data.
    /// </summary>
    /// <param name="data">The player data to get the hashed friend code for.</param>
    /// <returns>The hashed friend code string.</returns>
    internal static string GetHashFriendcode(this NetworkedPlayerInfo data)
    {
        if (data == null)
            return string.Empty;

        if (data.FriendCode == null)
            return string.Empty;

        return Utils.GetHashStr(data.FriendCode);
    }

    /// <summary>
    /// Reports a player with a specified reason.
    /// </summary>
    /// <param name="player">The player to report.</param>
    /// <param name="reason">The reason for the report.</param>
    internal static void ReportPlayer(this PlayerControl player, ReportReasons reason = ReportReasons.None)
    {
        if (player == null)
            return;

        var client = player.GetClient();
        if (client == null)
            return;

        if (client.HasBeenReported)
        {
            AmongUsClient.Instance.ReportPlayer(player.GetClientId(), reason);
        }
    }
}