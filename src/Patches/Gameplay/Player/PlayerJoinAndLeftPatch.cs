using BepInEx.Unity.IL2CPP.Utils;
using BetterAmongUs.Data;
using BetterAmongUs.Data.Config;
using BetterAmongUs.Generated;
using BetterAmongUs.Modules;
using BetterAmongUs.Modules.Support;
using BetterAmongUs.MonoScripts.Extended;
using BetterAmongUs.Patches.Gameplay.UI;
using BetterAmongUs.Patches.Gameplay.UI.Settings;
using BetterAmongUs.Utilities;
using HarmonyLib;
using InnerNet;
using System.Collections;

namespace BetterAmongUs.Patches.Gameplay.Player;

[HarmonyPatch]
internal static class PlayerJoinAndLeftPatch
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
    [HarmonyPostfix]
    private static void AmongUsClient_OnGameJoined_Postfix()
    {
        // Fix host icon color display on modded servers
        if (!GameState.IsVanillaServer)
        {
            var host = AmongUsClient.Instance.GetHost().Character;
            host?.SetColor(-2);
            host?.SetColor(host.CurrentOutfit.ColorId);
        }

        Logger_.Log($"Successfully joined {GameCode.IntToGameName(AmongUsClient.Instance.GameId)}", "OnGameJoinedPatch");
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
    [HarmonyPostfix]
    private static void AmongUsClient_OnPlayerJoined_Postfix(AmongUsClient __instance, ClientData data)
    {
        if (GameState.IsHost)
        {
            __instance.StartCoroutine(CoCheckPlayerOnJoin(data));
        }
    }

    private static IEnumerator CoCheckPlayerOnJoin(ClientData data)
    {
        while (!data.InScene || data.Character == null || data.Character.Data == null || data.Character.Data.IsIncomplete)
        {
            yield return null;
        }

        if (GameState.IsInGame)
        {
            var player = Utils.PlayerFromClientId(data.Id);

            // Check if player is in ban list by friend code or PUID
            if (BetterGameSettings.UseBanPlayerList.GetBool())
            {
                if (player != null)
                {
                    if (TextFileHandler.CompareStringMatch(BetterDataManager.Files.banPlayerListFilePath,
                        BAUPlugin.AllPlayerControls.Select(player => player.Data.FriendCode)
                        .Concat(BAUPlugin.AllPlayerControls.Select(player => player.GetHashPuid())).ToArray()))
                    {
                        player.Kick(true, TranslationStrings.AntiCheat_BanPlayerListMessage.LocalizedString, bypassDataCheck: true);
                        yield break;
                    }
                }
            }

            // Check if player name matches banned name patterns
            if (BetterGameSettings.UseBanNameList.GetBool())
            {
                if (player != null)
                {
                    if (TextFileHandler.CompareStringFilters(BetterDataManager.Files.banNameListFilePath, [player.Data.PlayerName]))
                    {
                        player?.Kick(true, TranslationStrings.AntiCheat_BanPlayerListMessage.LocalizedString, bypassDataCheck: true);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
    [HarmonyPostfix]
    private static void AmongUsClient_OnPlayerLeft_Postfix(ClientData data, DisconnectReasons reason)
    {
        // Reclaim favorite color when player leaves in lobby
        if (GameState.IsLobby)
        {
            if (!BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_FavoriteColor))
            {
                var favColorId = (byte)BAUConfigs.FavoriteColor.Value;
                if (BAUConfigs.FavoriteColor.Value >= 0)
                {
                    if (PlayerControl.LocalPlayer.cosmetics.ColorId != favColorId && data.ColorId == favColorId)
                    {
                        PlayerControl.LocalPlayer.CmdCheckColor(favColorId);
                    }
                }
            }
        }

        // Update host icon in meeting
        MeetingHudPatch.UpdateHostIcon();
    }

    [HarmonyPatch(typeof(GameData))]
    [HarmonyPatch(nameof(GameData.HandleDisconnect))]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch([typeof(PlayerControl), typeof(DisconnectReasons)])]
    [HarmonyPrefix]
    private static void GameData_HandleDisconnect_Prefix(PlayerControl player, DisconnectReasons reason)
    {
        // Store disconnect reason in player's BetterData
        if (player.ExtendedData() != null)
        {
            player.ExtendedData().DisconnectReason = reason;
        }

        // Show custom disconnect notification
        BetterShowNotification(player.Data, reason);
    }

    [HarmonyPatch(typeof(GameData), nameof(GameData.ShowNotification))]
    [HarmonyPrefix]
    internal static bool GameData_ShowNotification_Prefix()
    {
        // Disable vanilla disconnect notifications (use BAU's instead)
        return false;
    }

    internal static void BetterShowNotification(NetworkedPlayerInfo playerData, DisconnectReasons reason = DisconnectReasons.Unknown, string forceReasonText = "")
    {
        if (playerData == null)
            return;

        // Prevent showing duplicate notifications
        if (playerData.ExtendedData().AntiCheatInfo.BannedByAntiCheat || playerData.ExtendedData().HasShowDcMsg)
            return;

        playerData.ExtendedData().HasShowDcMsg = true;

        string? playerName = playerData.ExtendedData().RealName;

        // Use custom reason text if provided
        if (forceReasonText != "")
        {
            var ReasonText = $"<color=#ff0>{playerData.ExtendedData().RealName}</color> {forceReasonText}";

            Logger_.Log(ReasonText);

            HudManager.Instance.Notifier.AddDisconnectMessage(ReasonText);
        }
        else
        {
            string ReasonText;

            // Format disconnect message based on reason type
            switch (reason)
            {
                case DisconnectReasons.ExitGame:
                    ReasonText = TranslationStrings.DisconnectReason_Left.Format(playerName);
                    break;
                case DisconnectReasons.ClientTimeout:
                    ReasonText = TranslationStrings.DisconnectReason_Disconnect.Format(playerName);
                    break;
                case DisconnectReasons.Kicked:
                    ReasonText = TranslationStrings.DisconnectReason_Kicked.Format(playerName, AmongUsClient.Instance.GetHost().Character.Data.PlayerName);
                    break;
                case DisconnectReasons.Banned:
                    ReasonText = TranslationStrings.DisconnectReason_Banned.Format(playerName, AmongUsClient.Instance.GetHost().Character.Data.PlayerName);
                    break;
                case DisconnectReasons.Hacking:
                    ReasonText = TranslationStrings.DisconnectReason_Cheater.Format(playerName);
                    break;
                case DisconnectReasons.Error:
                    ReasonText = TranslationStrings.DisconnectReason_Error.Format(playerName);
                    break;
                case DisconnectReasons.Unknown:
                    ReasonText = TranslationStrings.DisconnectReason_Unknown.Format(playerName);
                    break;
                default:
                    ReasonText = TranslationStrings.DisconnectReason_Left.Format(playerName);
                    break;
            }

            Logger_.Log(ReasonText);

            // Add formatted disconnect message to game UI
            HudManager.Instance.Notifier.AddDisconnectMessage(ReasonText);
        }
    }
}