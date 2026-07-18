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
internal sealed class ModMenuCrewChatHandler : RPCHandler
{
    internal override byte CallId => unchecked((byte)CustomRPC.ModMenuCrewChat);

    internal override void HandleCheatRpcCheck(PlayerControl? sender, MessageReader reader)
    {
        try
        {
            var tag = reader.ReadByte();
            var senderId = reader.ReadPackedInt32();
            var senderName = reader.ReadString();
            var content = reader.ReadString();
            var timestamp = reader.ReadUInt64();
            var type = reader.ReadByte();

            if (sender.PlayerId != senderId)
                return;

            var betterData = sender.ExtendedData();
            var alreadyContainsMessage = betterData.AntiCheatInfo.MCCChats.Count > 0 && betterData.AntiCheatInfo.MCCChats.Last() == content;
            if (!alreadyContainsMessage)
            {
                Utils.AddChatPrivate($"{content}", overrideName: $"<b>{TranslationStrings.AntiCheat_Cheat_MMCChat.LocalizedString.ToColor(Colors.MMCHexColor)} - {sender.GetPlayerNameAndColor()}</b>");
                betterData.AntiCheatInfo.MCCChats.Add(content);
            }

            if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_Anticheat))
                return;

            if (!BAUConfigs.AntiCheat.Value || !BetterGameSettings.DetectCheatClients.GetBool())
                return;

            var isEmpty = string.IsNullOrEmpty(senderName) && string.IsNullOrEmpty(content);
            if (!isEmpty && !BetterDataManager.Files.BetterDataFile.MMCData.Any(info => info.CheckPlayerData(sender.Data)))
            {
                sender.ReportPlayer(ReportReasons.Cheating_Hacking);
                BetterDataManager.Files.BetterDataFile.MMCData.Add(new(sender?.ExtendedData().RealName ?? sender.Data.PlayerName, sender.GetHashPuid(), sender.Data.FriendCode, "ModMenuCrew Chat RPC"));
                BetterDataManager.Files.BetterDataFile.Save();
                BetterNotificationManager.NotifyCheat(sender, TranslationStrings.AntiCheat_Cheat_MMCChat.LocalizedString, TranslationStrings.AntiCheat_HasBeenDetectedWithCheatClient.LocalizedString);
            }
        }
        catch
        {
            if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_Anticheat))
                return;

            if (!BAUConfigs.AntiCheat.Value || !BetterGameSettings.DetectCheatClients.GetBool())
                return;

            if (!BetterDataManager.Files.BetterDataFile.MMCData.Any(info => info.CheckPlayerData(sender.Data)))
            {
                sender.ReportPlayer(ReportReasons.Cheating_Hacking);
                BetterDataManager.Files.BetterDataFile.MMCData.Add(new(sender?.ExtendedData().RealName ?? sender.Data.PlayerName, sender.GetHashPuid(), sender.Data.FriendCode, "ModMenuCrew Chat RPC"));
                BetterDataManager.Files.BetterDataFile.Save();
                BetterNotificationManager.NotifyCheat(sender, TranslationStrings.AntiCheat_Cheat_MMCChat.LocalizedString, TranslationStrings.AntiCheat_HasBeenDetectedWithCheatClient.LocalizedString);
            }
        }
    }
}