using BetterAmongUs.Attributes;
using BetterAmongUs.Data;
using BetterAmongUs.Data.Config;
using BetterAmongUs.Enums;
using BetterAmongUs.Generated;
using BetterAmongUs.Managers;
using BetterAmongUs.Modules.Support;
using BetterAmongUs.MonoScripts.Extended;
using BetterAmongUs.Patches.Gameplay.UI.Settings;
using BetterAmongUs.Utilities;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers.Cheats;

[RegisterRPCHandler]
internal sealed class AUMChatHandler : RPCHandler
{
    internal override byte CallId => unchecked((byte)CustomRPC.AUMChat);

    internal override void HandleCheatRpcCheck(PlayerControl? sender, MessageReader reader)
    {
        try
        {
            var nameString = reader.ReadString();
            var msgString = reader.ReadString();
            var colorId = reader.ReadInt32();

            var betterData = sender.ExtendedData();
            var alreadyContainsMessage = betterData.AntiCheatInfo.AUMChats.Count > 0 && betterData.AntiCheatInfo.AUMChats.Last() == msgString;
            if (!alreadyContainsMessage)
            {
                Utils.AddChatPrivate($"{msgString}", overrideName: $"<b>{TranslationStrings.AntiCheat_Cheat_AUMChat.LocalizedString.ToColor(Colors.AUMHexColor)} - {sender.GetPlayerNameAndColor()}</b>");
                betterData.AntiCheatInfo.AUMChats.Add(msgString);
            }

            Logger_.Log($"{sender.Data.PlayerName} -> {msgString}", "AUMChatLog");

            if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_Anticheat))
                return;

            if (!BAUConfigs.AntiCheat.Value || !BetterGameSettings.DetectCheatClients.GetBool())
                return;

            var isEmpty = string.IsNullOrEmpty(nameString) && string.IsNullOrEmpty(msgString);

            if (!isEmpty && !BetterDataManager.Files.BetterDataFile.AUMData.Any(info => info.CheckPlayerData(sender.Data)))
            {
                sender.ReportPlayer(ReportReasons.Cheating_Hacking);
                BetterDataManager.Files.BetterDataFile.AUMData.Add(new(betterData.RealName ?? sender.Data.PlayerName, sender.GetHashPuid(), sender.Data.FriendCode, "AUM Chat RPC"));
                BetterDataManager.Files.BetterDataFile.Save();
                BetterNotificationManager.NotifyCheat(sender, TranslationStrings.AntiCheat_Cheat_AUMChat.LocalizedString, TranslationStrings.AntiCheat_HasBeenDetectedWithCheatClient.LocalizedString);
            }
        }
        catch
        {
            if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_Anticheat))
                return;

            if (!BAUConfigs.AntiCheat.Value || !BetterGameSettings.DetectCheatClients.GetBool())
                return;

            if (!BetterDataManager.Files.BetterDataFile.AUMData.Any(info => info.CheckPlayerData(sender.Data)))
            {
                sender.ReportPlayer(ReportReasons.Cheating_Hacking);
                BetterDataManager.Files.BetterDataFile.AUMData.Add(new(sender?.ExtendedData().RealName ?? sender.Data.PlayerName, sender.GetHashPuid(), sender.Data.FriendCode, "AUM Chat RPC"));
                BetterDataManager.Files.BetterDataFile.Save();
                BetterNotificationManager.NotifyCheat(sender, TranslationStrings.AntiCheat_Cheat_AUMChat.LocalizedString, TranslationStrings.AntiCheat_HasBeenDetectedWithCheatClient.LocalizedString);
            }
        }
    }
}