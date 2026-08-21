// auto-generated
using BetterAmongUs.Modules;

namespace BetterAmongUs.Generated;

/// <summary>
/// Provides strongly-typed translation keys for BAU.
/// Each constant represents a translation key that can be used with the Translator.
/// </summary>
public static class TranslationStrings
{
    /// <summary>
    /// Represents a translation key with the ability to get the localized string.
    /// </summary>
    public readonly struct TranslationString(string key)
    {
        public readonly string Key = key;
        public string LocalizedString => Translator.GetString(this);
        public override string ToString() => LocalizedString;
        public string Format(params string[] strings)
        {
            return string.Format(LocalizedString, args: strings);
        }
        public string Format(params TranslationString[] translationStrings)
        {
            var stringArgs = translationStrings.Select(ts => ts.LocalizedString).ToArray();
            return string.Format(LocalizedString, stringArgs);
        }
        public string Format(params object[] args)
        {
            var stringArgs = args.Select(arg => arg.ToString()).ToArray();
            return string.Format(LocalizedString, stringArgs);
        }
    }

    /// <summary>
    /// Base Translation: BAU
    /// </summary>
    public static readonly TranslationString BAU = new("BAU");

    /// <summary>
    /// Base Translation: BetterAmongUs
    /// </summary>
    public static readonly TranslationString BetterAmongUs = new("BetterAmongUs");

    /// <summary>
    /// Base Translation: ♻
    /// </summary>
    public static readonly TranslationString BAUMark = new("BAUMark");

    /// <summary>
    /// Base Translation: ⚠
    /// </summary>
    public static readonly TranslationString WarningIcon = new("WarningIcon");

    /// <summary>
    /// Base Translation: Host
    /// </summary>
    public static readonly TranslationString Host = new("Host");

    /// <summary>
    /// Base Translation: Kills
    /// </summary>
    public static readonly TranslationString Kills = new("Kills");

    /// <summary>
    /// Base Translation: Tasks
    /// </summary>
    public static readonly TranslationString Tasks = new("Tasks");

    /// <summary>
    /// Base Translation: Alive
    /// </summary>
    public static readonly TranslationString Alive = new("Alive");

    /// <summary>
    /// Base Translation: Dead
    /// </summary>
    public static readonly TranslationString Dead = new("Dead");

    /// <summary>
    /// Base Translation: D/C
    /// </summary>
    public static readonly TranslationString DC = new("DC");

    /// <summary>
    /// Base Translation: Ping
    /// </summary>
    public static readonly TranslationString Ping = new("Ping");

    /// <summary>
    /// Base Translation: Timer
    /// </summary>
    public static readonly TranslationString Timer = new("Timer");

    /// <summary>
    /// Base Translation: System Message
    /// </summary>
    public static readonly TranslationString SystemMessage = new("SystemMessage");

    /// <summary>
    /// Base Translation: System Notification
    /// </summary>
    public static readonly TranslationString SystemNotification = new("SystemNotification");

    /// <summary>
    /// Base Translation: {0} does not support\n&lt;b&gt;Modded Lobbies&lt;/b&gt;
    /// </summary>
    public static readonly TranslationString ModdedLobbyMsg = new("ModdedLobbyMsg");

    /// <summary>
    /// Base Translation: Host: {0}
    /// </summary>
    public static readonly TranslationString HostInMeeting = new("HostInMeeting");

    /// <summary>
    /// Base Translation: Preset {0}
    /// </summary>
    public static readonly TranslationString Setting_Preset = new("Setting.Preset");

    /// <summary>
    /// Base Translation: Presets
    /// </summary>
    public static readonly TranslationString Setting_Presets = new("Setting.Presets");

    /// <summary>
    /// Base Translation: Loading
    /// </summary>
    public static readonly TranslationString Player_Loading = new("Player.Loading");

    /// <summary>
    /// Base Translation: No Friend Code
    /// </summary>
    public static readonly TranslationString Player_NoFriendCode = new("Player.NoFriendCode");

    /// <summary>
    /// Base Translation: Platform Hidden
    /// </summary>
    public static readonly TranslationString Player_PlatformHidden = new("Player.PlatformHidden");

    /// <summary>
    /// Base Translation: Better User
    /// </summary>
    public static readonly TranslationString Player_BetterUser = new("Player.BetterUser");

    /// <summary>
    /// Base Translation: Sicko User
    /// </summary>
    public static readonly TranslationString Player_SickoUser = new("Player.SickoUser");

    /// <summary>
    /// Base Translation: AUM User
    /// </summary>
    public static readonly TranslationString Player_AUMUser = new("Player.AUMUser");

    /// <summary>
    /// Base Translation: KN User
    /// </summary>
    public static readonly TranslationString Player_KNUser = new("Player.KNUser");

    /// <summary>
    /// Base Translation: MMC User
    /// </summary>
    public static readonly TranslationString Player_MMCUser = new("Player.MMCUser");

    /// <summary>
    /// Base Translation: Flagged Player
    /// </summary>
    public static readonly TranslationString Player_FlaggedPlayer = new("Player.FlaggedPlayer");

    /// <summary>
    /// Base Translation: {0} Left the game!
    /// </summary>
    public static readonly TranslationString DisconnectReason_Left = new("DisconnectReason.Left");

    /// <summary>
    /// Base Translation: {0} Disconnected!
    /// </summary>
    public static readonly TranslationString DisconnectReason_Disconnect = new("DisconnectReason.Disconnect");

    /// <summary>
    /// Base Translation: {0} Was kicked by {1}!
    /// </summary>
    public static readonly TranslationString DisconnectReason_Kicked = new("DisconnectReason.Kicked");

    /// <summary>
    /// Base Translation: {0} Was banned by {1}!
    /// </summary>
    public static readonly TranslationString DisconnectReason_Banned = new("DisconnectReason.Banned");

    /// <summary>
    /// Base Translation: {0} Was banned by Innersloth Anti-Cheat!
    /// </summary>
    public static readonly TranslationString DisconnectReason_Cheater = new("DisconnectReason.Cheater");

    /// <summary>
    /// Base Translation: {0} Was kicked due to an error!
    /// </summary>
    public static readonly TranslationString DisconnectReason_Error = new("DisconnectReason.Error");

    /// <summary>
    /// Base Translation: {0} Left the game due to unknown reason?
    /// </summary>
    public static readonly TranslationString DisconnectReason_Unknown = new("DisconnectReason.Unknown");

    /// <summary>
    /// Base Translation: Left The Game
    /// </summary>
    public static readonly TranslationString DisconnectReasonMeeting_Left = new("DisconnectReasonMeeting.Left");

    /// <summary>
    /// Base Translation: Disconnected
    /// </summary>
    public static readonly TranslationString DisconnectReasonMeeting_Disconnect = new("DisconnectReasonMeeting.Disconnect");

    /// <summary>
    /// Base Translation: Kicked By Host
    /// </summary>
    public static readonly TranslationString DisconnectReasonMeeting_Kicked = new("DisconnectReasonMeeting.Kicked");

    /// <summary>
    /// Base Translation: Banned By Host
    /// </summary>
    public static readonly TranslationString DisconnectReasonMeeting_Banned = new("DisconnectReasonMeeting.Banned");

    /// <summary>
    /// Base Translation: Banned By Anti-Cheat
    /// </summary>
    public static readonly TranslationString DisconnectReasonMeeting_AntiCheat = new("DisconnectReasonMeeting.AntiCheat");

    /// <summary>
    /// Base Translation: Banned By Server
    /// </summary>
    public static readonly TranslationString DisconnectReasonMeeting_Cheater = new("DisconnectReasonMeeting.Cheater");

    /// <summary>
    /// Base Translation: Welcome To {0}
    /// </summary>
    public static readonly TranslationString WelcomeMsg_WelcomeToBAU = new("WelcomeMsg.WelcomeToBAU");

    /// <summary>
    /// Base Translation: Thanks for downloading!
    /// </summary>
    public static readonly TranslationString WelcomeMsg_ThanksForDownloading = new("WelcomeMsg.ThanksForDownloading");

    /// <summary>
    /// Base Translation: &lt;color=#0dff00&gt;{0}&lt;/color&gt; Is a mod for improving the vanilla Among Us experience with a built-in {1} and other features, &lt;color=#0dff00&gt;{0}&lt;/color&gt; is a client-sided mod so it can be used with other vanilla Among Us players.
    /// </summary>
    public static readonly TranslationString WelcomeMsg_BAUDescription1 = new("WelcomeMsg.BAUDescription1");

    /// <summary>
    /// Base Translation: Better Options
    /// </summary>
    public static readonly TranslationString BetterOption = new("BetterOption");

    /// <summary>
    /// Base Translation: Next &gt;
    /// </summary>
    public static readonly TranslationString BetterOption_Next = new("BetterOption.Next");

    /// <summary>
    /// Base Translation: &lt; Prev
    /// </summary>
    public static readonly TranslationString BetterOption_Previous = new("BetterOption.Previous");

    /// <summary>
    /// Base Translation: &lt;color=#4f92ff&gt;Anti-Cheat&lt;/color&gt;
    /// </summary>
    public static readonly TranslationString BetterOption_AntiCheat = new("BetterOption.AntiCheat");

    /// <summary>
    /// Base Translation: &lt;color=#4f92ff&gt;Send Better RPC&lt;/color&gt;
    /// </summary>
    public static readonly TranslationString BetterOption_SendBetterRpc = new("BetterOption.SendBetterRpc");

    /// <summary>
    /// Base Translation: &lt;color=#4f92ff&gt;Better Notifications&lt;/color&gt;
    /// </summary>
    public static readonly TranslationString BetterOption_BetterNotifications = new("BetterOption.BetterNotifications");

    /// <summary>
    /// Base Translation: Force System language
    /// </summary>
    public static readonly TranslationString BetterOption_ForceOwnLanguage = new("BetterOption.ForceOwnLanguage");

    /// <summary>
    /// Base Translation: Chat Dark Mode
    /// </summary>
    public static readonly TranslationString BetterOption_ChatDarkMode = new("BetterOption.ChatDarkMode");

    /// <summary>
    /// Base Translation: Chat In Gameplay
    /// </summary>
    public static readonly TranslationString BetterOption_ChatInGame = new("BetterOption.ChatInGame");

    /// <summary>
    /// Base Translation: Show Lobby Info
    /// </summary>
    public static readonly TranslationString BetterOption_LobbyInfo = new("BetterOption.LobbyInfo");

    /// <summary>
    /// Base Translation: Disable Lobby Theme
    /// </summary>
    public static readonly TranslationString BetterOption_LobbyTheme = new("BetterOption.LobbyTheme");

    /// <summary>
    /// Base Translation: UnlockFPS
    /// </summary>
    public static readonly TranslationString BetterOption_UnlockFPS = new("BetterOption.UnlockFPS");

    /// <summary>
    /// Base Translation: ShowFPS
    /// </summary>
    public static readonly TranslationString BetterOption_ShowFPS = new("BetterOption.ShowFPS");

    /// <summary>
    /// Base Translation: Vent Color Groups
    /// </summary>
    public static readonly TranslationString BetterOption_VentColorGroups = new("BetterOption.VentColorGroups");

    /// <summary>
    /// Base Translation: Minimap Icons
    /// </summary>
    public static readonly TranslationString BetterOption_MinimapIcons = new("BetterOption.MinimapIcons");

    /// <summary>
    /// Base Translation: Compress Setting Files
    /// </summary>
    public static readonly TranslationString BetterOption_CompressSettingFiles = new("BetterOption.CompressSettingFiles");

    /// <summary>
    /// Base Translation: Open Save Data
    /// </summary>
    public static readonly TranslationString BetterOption_SaveData = new("BetterOption.SaveData");

    /// <summary>
    /// Base Translation: Switch To Vanilla
    /// </summary>
    public static readonly TranslationString BetterOption_ToVanilla = new("BetterOption.ToVanilla");

    /// <summary>
    /// Base Translation: Better Settings
    /// </summary>
    public static readonly TranslationString BetterSetting = new("BetterSetting");

    /// <summary>
    /// Base Translation: Edit better settings for your lobby and gameplay.
    /// </summary>
    public static readonly TranslationString BetterSetting_Description = new("BetterSetting.Description");

    /// <summary>
    /// Base Translation: Set To:
    /// </summary>
    public static readonly TranslationString BetterSetting_SetTo = new("BetterSetting.SetTo");

    /// <summary>
    /// Base Translation: &lt;color=#07B400&gt;System Settings&lt;/color&gt;
    /// </summary>
    public static readonly TranslationString BetterSetting_MainHeader_System = new("BetterSetting.MainHeader.System");

    /// <summary>
    /// Base Translation: &lt;color=#4f92ff&gt;Anti-Cheat Settings&lt;/color&gt;
    /// </summary>
    public static readonly TranslationString BetterSetting_MainHeader_AntiCheat = new("BetterSetting.MainHeader.AntiCheat");

    /// <summary>
    /// Base Translation: &lt;color=#4f92ff&gt;Role Algorithm Settings&lt;/color&gt;
    /// </summary>
    public static readonly TranslationString BetterSetting_MainHeader_RoleAlgorithm = new("BetterSetting.MainHeader.RoleAlgorithm");

    /// <summary>
    /// Base Translation: &lt;color=#d7d700&gt;Gameplay Settings&lt;/color&gt;
    /// </summary>
    public static readonly TranslationString BetterSetting_MainHeader_Gameplay = new("BetterSetting.MainHeader.Gameplay");

    /// <summary>
    /// Base Translation: &lt;color=#d7d700&gt;Hide &amp; Seek Settings&lt;/color&gt;
    /// </summary>
    public static readonly TranslationString BetterSetting_MainHeader_HideNSeek = new("BetterSetting.MainHeader.HideNSeek");

    /// <summary>
    /// Base Translation: &lt;color=#4f92ff&gt;Host Only&lt;/color&gt;
    /// </summary>
    public static readonly TranslationString BetterSetting_TextHeader_HostOnly = new("BetterSetting.TextHeader.HostOnly");

    /// <summary>
    /// Base Translation: &lt;color=#4f92ff&gt;Detections&lt;/color&gt;
    /// </summary>
    public static readonly TranslationString BetterSetting_TextHeader_Detections = new("BetterSetting.TextHeader.Detections");

    /// <summary>
    /// Base Translation: Kick cooldown
    /// </summary>
    public static readonly TranslationString BetterSetting_Setting_KickCooldown = new("BetterSetting.Setting.KickCooldown");

    /// <summary>
    /// Base Translation: When a player is caught cheating
    /// </summary>
    public static readonly TranslationString BetterSetting_Setting_WhenCheating = new("BetterSetting.Setting.WhenCheating");

    /// <summary>
    /// Base Translation: Notify
    /// </summary>
    public static readonly TranslationString BetterSetting_Setting_WhenCheating_Notify = new("BetterSetting.Setting.WhenCheating.Notify");

    /// <summary>
    /// Base Translation: Kick
    /// </summary>
    public static readonly TranslationString BetterSetting_Setting_WhenCheating_Kick = new("BetterSetting.Setting.WhenCheating.Kick");

    /// <summary>
    /// Base Translation: Ban
    /// </summary>
    public static readonly TranslationString BetterSetting_Setting_WhenCheating_Ban = new("BetterSetting.Setting.WhenCheating.Ban");

    /// <summary>
    /// Base Translation: Detected invalid friendCodes
    /// </summary>
    public static readonly TranslationString BetterSetting_Setting_InvalidFriendCode = new("BetterSetting.Setting.InvalidFriendCode");

    /// <summary>
    /// Base Translation: Cancel invalid sabotages
    /// </summary>
    public static readonly TranslationString BetterSetting_Setting_CancelInvalidSabotage = new("BetterSetting.Setting.CancelInvalidSabotage");

    /// <summary>
    /// Base Translation: Use ban player list
    /// </summary>
    public static readonly TranslationString BetterSetting_Setting_UseBanPlayerList = new("BetterSetting.Setting.UseBanPlayerList");

    /// <summary>
    /// Base Translation: Use ban name list
    /// </summary>
    public static readonly TranslationString BetterSetting_Setting_UseBanNameList = new("BetterSetting.Setting.UseBanNameList");

    /// <summary>
    /// Base Translation: Use ban chat list
    /// </summary>
    public static readonly TranslationString BetterSetting_Setting_UseBanChatList = new("BetterSetting.Setting.UseBanChatList");

    /// <summary>
    /// Base Translation: Only in lobby
    /// </summary>
    public static readonly TranslationString BetterSetting_Setting_UseBanChatListOnlyLobby = new("BetterSetting.Setting.UseBanChatListOnlyLobby");

    /// <summary>
    /// Base Translation: Ban
    /// </summary>
    public static readonly TranslationString BetterSetting_Setting_UseBanChatListBan = new("BetterSetting.Setting.UseBanChatListBan");

    /// <summary>
    /// Base Translation: Show role in name for clients
    /// </summary>
    public static readonly TranslationString BetterSetting_Setting_ShowRoleForClients = new("BetterSetting.Setting.ShowRoleForClients");

    /// <summary>
    /// Base Translation: Censor detection reason
    /// </summary>
    public static readonly TranslationString BetterSetting_Setting_CensorDetectionReason = new("BetterSetting.Setting.CensorDetectionReason");

    /// <summary>
    /// Base Translation: Detected player levels
    /// </summary>
    public static readonly TranslationString BetterSetting_Setting_DetectedLevel = new("BetterSetting.Setting.DetectedLevel");

    /// <summary>
    /// Base Translation: Detected player level above
    /// </summary>
    public static readonly TranslationString BetterSetting_Setting_DetectedLevelAbove = new("BetterSetting.Setting.DetectedLevelAbove");

    /// <summary>
    /// Base Translation: Kick player levels
    /// </summary>
    public static readonly TranslationString BetterSetting_Setting_KickLevel = new("BetterSetting.Setting.KickLevel");

    /// <summary>
    /// Base Translation: Kick player level below
    /// </summary>
    public static readonly TranslationString BetterSetting_Setting_KickLevelBelow = new("BetterSetting.Setting.KickLevelBelow");

    /// <summary>
    /// Base Translation: Detect cheat clients
    /// </summary>
    public static readonly TranslationString BetterSetting_Setting_DetectCheatClients = new("BetterSetting.Setting.DetectCheatClients");

    /// <summary>
    /// Base Translation: Detect invalid Rpcs
    /// </summary>
    public static readonly TranslationString BetterSetting_Setting_DetectInvalidRpcs = new("BetterSetting.Setting.DetectInvalidRpcs");

    /// <summary>
    /// Base Translation: Rpc Rate Limiting
    /// </summary>
    public static readonly TranslationString BetterSetting_Setting_RpcRateLimiting = new("BetterSetting.Setting.RpcRateLimiting");

    /// <summary>
    /// Base Translation: Rate Limit
    /// </summary>
    public static readonly TranslationString BetterSetting_Setting_RateLimit = new("BetterSetting.Setting.RateLimit");

    /// <summary>
    /// Base Translation: Randomizer
    /// </summary>
    public static readonly TranslationString BetterSetting_Setting_RoleRandomizer = new("BetterSetting.Setting.RoleRandomizer");

    /// <summary>
    /// Base Translation: Desync roles to other clients
    /// </summary>
    public static readonly TranslationString BetterSetting_Setting_DesyncRoles = new("BetterSetting.Setting.DesyncRoles");

    /// <summary>
    /// Base Translation: Disable sabotages for dead
    /// </summary>
    public static readonly TranslationString BetterSetting_Setting_DisableSabotagesForDead = new("BetterSetting.Setting.DisableSabotagesForDead");

    /// <summary>
    /// Base Translation: Disable sabotages
    /// </summary>
    public static readonly TranslationString BetterSetting_Setting_DisableSabotages = new("BetterSetting.Setting.DisableSabotages");

    /// <summary>
    /// Base Translation: Remove pet on death
    /// </summary>
    public static readonly TranslationString BetterSetting_Setting_RemovePetOnDeath = new("BetterSetting.Setting.RemovePetOnDeath");

    /// <summary>
    /// Base Translation: # Seekers
    /// </summary>
    public static readonly TranslationString BetterSetting_Setting_HideAndSeekImpNum = new("BetterSetting.Setting.HideAndSeekImpNum");

    /// <summary>
    /// Base Translation: Seeker
    /// </summary>
    public static readonly TranslationString BetterSetting_TempSetting_HideAndSeekImpNum = new("BetterSetting.TempSetting.HideAndSeekImpNum");

    /// <summary>
    /// Base Translation: Anti-Cheat
    /// </summary>
    public static readonly TranslationString AntiCheat = new("AntiCheat");

    /// <summary>
    /// Base Translation: Banned
    /// </summary>
    public static readonly TranslationString AntiCheat_Ban = new("AntiCheat.Ban");

    /// <summary>
    /// Base Translation: Kicked
    /// </summary>
    public static readonly TranslationString AntiCheat_Kick = new("AntiCheat.Kick");

    /// <summary>
    /// Base Translation: Player
    /// </summary>
    public static readonly TranslationString AntiCheat_PlayerDetected = new("AntiCheat.PlayerDetected");

    /// <summary>
    /// Base Translation: Anti-Cheat has been temporarily disabled on modded protocol!
    /// </summary>
    public static readonly TranslationString AntiCheat_DisabledModdedProtocol = new("AntiCheat.DisabledModdedProtocol");

    /// <summary>
    /// Base Translation: Has been detected doing an unauthorized action
    /// </summary>
    public static readonly TranslationString AntiCheat_UnauthorizedAction = new("AntiCheat.UnauthorizedAction");

    /// <summary>
    /// Base Translation: {0} by &lt;color=#4f92ff&gt;Anti-Cheat&lt;/color&gt;
    /// </summary>
    public static readonly TranslationString AntiCheat_ByAntiCheat = new("AntiCheat.ByAntiCheat");

    /// <summary>
    /// Base Translation: {0}! Reason: &lt;color=#fc0000&gt;{1}&lt;/color&gt;
    /// </summary>
    public static readonly TranslationString AntiCheat_KickMessage = new("AntiCheat.KickMessage");

    /// <summary>
    /// Base Translation: has been {0} due to being on the ban player list!
    /// </summary>
    public static readonly TranslationString AntiCheat_BanPlayerListMessage = new("AntiCheat.BanPlayerListMessage");

    /// <summary>
    /// Base Translation: has been {0} due to their name being on the ban name list!
    /// </summary>
    public static readonly TranslationString AntiCheat_BanNameListMessage = new("AntiCheat.BanNameListMessage");

    /// <summary>
    /// Base Translation: Has been detected with a cheat
    /// </summary>
    public static readonly TranslationString AntiCheat_HasBeenDetectedWithCheat = new("AntiCheat.HasBeenDetectedWithCheat");

    /// <summary>
    /// Base Translation: Has been detected with a cheat client
    /// </summary>
    public static readonly TranslationString AntiCheat_HasBeenDetectedWithCheatClient = new("AntiCheat.HasBeenDetectedWithCheatClient");

    /// <summary>
    /// Base Translation: Invalid Action RPC: {0}
    /// </summary>
    public static readonly TranslationString AntiCheat_InvalidAction = new("AntiCheat.InvalidAction");

    /// <summary>
    /// Base Translation: Invalid Action RPC: {0}
    /// </summary>
    public static readonly TranslationString AntiCheat_InvalidActionRPC = new("AntiCheat.InvalidActionRPC");

    /// <summary>
    /// Base Translation: Invalid Host RPC: {0}
    /// </summary>
    public static readonly TranslationString AntiCheat_InvalidHostRPC = new("AntiCheat.InvalidHostRPC");

    /// <summary>
    /// Base Translation: Invalid Set RPC: {0}
    /// </summary>
    public static readonly TranslationString AntiCheat_InvalidSetRPC = new("AntiCheat.InvalidSetRPC");

    /// <summary>
    /// Base Translation: Invalid Lobby RPC: {0}
    /// </summary>
    public static readonly TranslationString AntiCheat_InvalidLobbyRPC = new("AntiCheat.InvalidLobbyRPC");

    /// <summary>
    /// Base Translation: Invalid Level: {0}
    /// </summary>
    public static readonly TranslationString AntiCheat_InvalidLevelRPC = new("AntiCheat.InvalidLevelRPC");

    /// <summary>
    /// Base Translation: Attempted To Ban Exploit
    /// </summary>
    public static readonly TranslationString AntiCheat_TryBanExploit = new("AntiCheat.TryBanExploit");

    /// <summary>
    /// Base Translation: Sicko Menu
    /// </summary>
    public static readonly TranslationString AntiCheat_Cheat_Sicko = new("AntiCheat.Cheat.Sicko");

    /// <summary>
    /// Base Translation: AUM
    /// </summary>
    public static readonly TranslationString AntiCheat_Cheat_AUM = new("AntiCheat.Cheat.AUM");

    /// <summary>
    /// Base Translation: AUM Chat
    /// </summary>
    public static readonly TranslationString AntiCheat_Cheat_AUMChat = new("AntiCheat.Cheat.AUMChat");

    /// <summary>
    /// Base Translation: Kill Network
    /// </summary>
    public static readonly TranslationString AntiCheat_Cheat_KN = new("AntiCheat.Cheat.KN");

    /// <summary>
    /// Base Translation: KN Chat
    /// </summary>
    public static readonly TranslationString AntiCheat_Cheat_KNChat = new("AntiCheat.Cheat.KNChat");

    /// <summary>
    /// Base Translation: Mod Menu Crew
    /// </summary>
    public static readonly TranslationString AntiCheat_Cheat_MMC = new("AntiCheat.Cheat.MMC");

    /// <summary>
    /// Base Translation: MMC Chat
    /// </summary>
    public static readonly TranslationString AntiCheat_Cheat_MMCChat = new("AntiCheat.Cheat.MMCChat");

    /// <summary>
    /// Base Translation: Invalid Friend Code
    /// </summary>
    public static readonly TranslationString AntiCheat_Reason_InvalidFriendCode = new("AntiCheat.Reason.InvalidFriendCode");

    /// <summary>
    /// Base Translation: Platform Spoofer
    /// </summary>
    public static readonly TranslationString AntiCheat_Reason_PlatformSpoofer = new("AntiCheat.Reason.PlatformSpoofer");

    /// <summary>
    /// Base Translation: RPC Spam
    /// </summary>
    public static readonly TranslationString AntiCheat_Reason_RPCSentPS = new("AntiCheat.Reason.RPCSentPS");

    /// <summary>
    /// Base Translation: Vote Kick Exploit
    /// </summary>
    public static readonly TranslationString AntiCheat_Reason_VoteKick = new("AntiCheat.Reason.VoteKick");

    /// <summary>
    /// Base Translation: Failed To Initialize Player
    /// </summary>
    public static readonly TranslationString AntiCheat_Reason_Initialize = new("AntiCheat.Reason.Initialize");

    /// <summary>
    /// Base Translation: Known Sicko User
    /// </summary>
    public static readonly TranslationString AntiCheat_Reason_SickoMenuUser = new("AntiCheat.Reason.SickoMenuUser");

    /// <summary>
    /// Base Translation: Known AUM User
    /// </summary>
    public static readonly TranslationString AntiCheat_Reason_AUMUser = new("AntiCheat.Reason.AUMUser");

    /// <summary>
    /// Base Translation: Known KN User
    /// </summary>
    public static readonly TranslationString AntiCheat_Reason_KNUser = new("AntiCheat.Reason.KNUser");

    /// <summary>
    /// Base Translation: Known MMC User
    /// </summary>
    public static readonly TranslationString AntiCheat_Reason_MMCUser = new("AntiCheat.Reason.MMCUser");

    /// <summary>
    /// Base Translation: Known Cheater
    /// </summary>
    public static readonly TranslationString AntiCheat_Reason_KnownCheater = new("AntiCheat.Reason.KnownCheater");

    /// <summary>
    /// Base Translation: Game Summary
    /// </summary>
    public static readonly TranslationString GameSummary = new("GameSummary");

    /// <summary>
    /// Base Translation: Hiders
    /// </summary>
    public static readonly TranslationString Game_Summary_Hiders = new("Game.Summary.Hiders");

    /// <summary>
    /// Base Translation: Seekers
    /// </summary>
    public static readonly TranslationString Game_Summary_Seekers = new("Game.Summary.Seekers");

    /// <summary>
    /// Base Translation: Won
    /// </summary>
    public static readonly TranslationString Game_Summary_Won = new("Game.Summary.Won");

    /// <summary>
    /// Base Translation: By
    /// </summary>
    public static readonly TranslationString Game_Summary_By = new("Game.Summary.By");

    /// <summary>
    /// Base Translation: Tasks Completion
    /// </summary>
    public static readonly TranslationString Game_Summary_Result_TasksCompletion = new("Game.Summary.Result.TasksCompletion");

    /// <summary>
    /// Base Translation: Imposters Voted Out
    /// </summary>
    public static readonly TranslationString Game_Summary_Result_ImpostersVotedOut = new("Game.Summary.Result.ImpostersVotedOut");

    /// <summary>
    /// Base Translation: Impostors Disconnected
    /// </summary>
    public static readonly TranslationString Game_Summary_Result_ImpostorsDisconnected = new("Game.Summary.Result.ImpostorsDisconnected");

    /// <summary>
    /// Base Translation: Crew Outnumbered
    /// </summary>
    public static readonly TranslationString Game_Summary_Result_CrewOutnumbered = new("Game.Summary.Result.CrewOutnumbered");

    /// <summary>
    /// Base Translation: Sabotage
    /// </summary>
    public static readonly TranslationString Game_Summary_Result_Sabotage = new("Game.Summary.Result.Sabotage");

    /// <summary>
    /// Base Translation: Cremates Disconnected
    /// </summary>
    public static readonly TranslationString Game_Summary_Result_CrematesDisconnected = new("Game.Summary.Result.CrematesDisconnected");

    /// <summary>
    /// Base Translation: Time Out
    /// </summary>
    public static readonly TranslationString Game_Summary_Result_TimeOut = new("Game.Summary.Result.TimeOut");

    /// <summary>
    /// Base Translation: No Survivors
    /// </summary>
    public static readonly TranslationString Game_Summary_Result_NoSurvivors = new("Game.Summary.Result.NoSurvivors");
}