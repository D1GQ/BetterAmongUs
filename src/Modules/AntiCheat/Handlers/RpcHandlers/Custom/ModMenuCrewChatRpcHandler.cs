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

namespace BetterAmongUs.Modules.AntiCheat.Handlers.RpcHandlers.Custom;

[RegisterRpcHandler(CustomRpc.ModMenuCrewChat)]
internal sealed class ModMenuCrewChatRpcHandler : IRpcHandler<PlayerControl>
{
    public AntiCheatFlags CheckRpc(PlayerControl innerNetObject, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
        return AntiCheatFlags.None;
    }

    public void HandleRpc(PlayerControl innerNetObject, MessageReader reader)
    {
        try
        {
            var tag = reader.ReadByte();
            var senderId = reader.ReadPackedInt32();
            var senderName = reader.ReadString();
            var content = reader.ReadString();
            var timestamp = reader.ReadUInt64();
            var type = reader.ReadByte();

            if (innerNetObject.PlayerId != senderId)
                return;

            var betterData = innerNetObject.ExtendedData();
            var alreadyContainsMessage = betterData.AntiCheatInfo.MCCChats.Count > 0 && betterData.AntiCheatInfo.MCCChats.Last() == content;
            if (!alreadyContainsMessage)
            {
                Utils.AddChatPrivate($"{content}", overrideName: $"<b>{TranslationStrings.AntiCheat_Cheat_MMCChat.LocalizedString.ToColor(Colors.MMCHexColor)} - {innerNetObject.GetPlayerNameAndColor()}</b>");
                betterData.AntiCheatInfo.MCCChats.Add(content);
            }

            if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_Anticheat))
                return;

            if (!BAUConfigs.AntiCheat.Value || !BetterGameSettings.DetectCheatClients.GetBool())
                return;

            var isEmpty = string.IsNullOrEmpty(senderName) && string.IsNullOrEmpty(content);
            if (!isEmpty && !BetterDataManager.Files.BetterDataFile.MMCData.Any(info => info.CheckPlayerData(innerNetObject.Data)))
            {
                innerNetObject.ReportPlayer(ReportReasons.Cheating_Hacking);
                BetterDataManager.Files.BetterDataFile.MMCData.Add(new(innerNetObject?.ExtendedData().RealName ?? innerNetObject.Data.PlayerName, innerNetObject.GetHashPuid(), innerNetObject.Data.FriendCode, "ModMenuCrew Chat RPC"));
                BetterDataManager.Files.BetterDataFile.Save();
                BetterNotificationManager.NotifyCheat(innerNetObject, TranslationStrings.AntiCheat_Cheat_MMCChat.LocalizedString, TranslationStrings.AntiCheat_HasBeenDetectedWithCheatClient.LocalizedString);
            }
        }
        catch
        {
            if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_Anticheat))
                return;

            if (!BAUConfigs.AntiCheat.Value || !BetterGameSettings.DetectCheatClients.GetBool())
                return;

            if (!BetterDataManager.Files.BetterDataFile.MMCData.Any(info => info.CheckPlayerData(innerNetObject.Data)))
            {
                innerNetObject.ReportPlayer(ReportReasons.Cheating_Hacking);
                BetterDataManager.Files.BetterDataFile.MMCData.Add(new(innerNetObject?.ExtendedData().RealName ?? innerNetObject.Data.PlayerName, innerNetObject.GetHashPuid(), innerNetObject.Data.FriendCode, "ModMenuCrew Chat RPC"));
                BetterDataManager.Files.BetterDataFile.Save();
                BetterNotificationManager.NotifyCheat(innerNetObject, TranslationStrings.AntiCheat_Cheat_MMCChat.LocalizedString, TranslationStrings.AntiCheat_HasBeenDetectedWithCheatClient.LocalizedString);
            }
        }
    }
}
