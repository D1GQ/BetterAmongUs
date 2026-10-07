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

[RegisterRpcHandler(CustomRpc.AUMChat)]
internal sealed class AUMChatRpcHandler : IRpcHandler<PlayerControl>
{
    public AntiCheatFlags CheckRpc(PlayerControl innerNetObject, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
        return AntiCheatFlags.None;
    }

    public void HandleRpc(PlayerControl innerNetObject, MessageReader reader)
    {
        try
        {
            var nameString = reader.ReadString();
            var msgString = reader.ReadString();
            var colorId = reader.ReadInt32();

            var betterData = innerNetObject.ExtendedData();
            var alreadyContainsMessage = betterData.AntiCheatInfo.AUMChats.Count > 0 && betterData.AntiCheatInfo.AUMChats.Last() == msgString;
            if (!alreadyContainsMessage)
            {
                Utils.AddChatPrivate($"{msgString}", overrideName: $"<b>{TranslationStrings.AntiCheat_Cheat_AUMChat.LocalizedString.ToColor(Colors.AUMHexColor)} - {innerNetObject.GetPlayerNameAndColor()}</b>");
                betterData.AntiCheatInfo.AUMChats.Add(msgString);
            }

            BAUPlugin.Logger.Log($"{innerNetObject.Data.PlayerName} -> {msgString}", "AUMChatLog");

            if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_Anticheat))
                return;

            if (!BAUConfigs.AntiCheat.Value || !BetterGameSettings.DetectCheatClients.GetBool())
                return;

            var isEmpty = string.IsNullOrEmpty(nameString) && string.IsNullOrEmpty(msgString);

            if (!isEmpty && !BetterDataManager.Files.BetterDataFile.AUMData.Any(info => info.CheckPlayerData(innerNetObject.Data)))
            {
                innerNetObject.ReportPlayer(ReportReasons.Cheating_Hacking);
                BetterDataManager.Files.BetterDataFile.AUMData.Add(new(betterData.RealName ?? innerNetObject.Data.PlayerName, innerNetObject.GetHashPuid(), innerNetObject.Data.FriendCode, "AUM Chat RPC"));
                BetterDataManager.Files.BetterDataFile.Save();
                BetterNotificationManager.NotifyCheat(innerNetObject, TranslationStrings.AntiCheat_Cheat_AUMChat.LocalizedString, TranslationStrings.AntiCheat_HasBeenDetectedWithCheatClient.LocalizedString);
            }
        }
        catch
        {
            if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_Anticheat))
                return;

            if (!BAUConfigs.AntiCheat.Value || !BetterGameSettings.DetectCheatClients.GetBool())
                return;

            if (!BetterDataManager.Files.BetterDataFile.AUMData.Any(info => info.CheckPlayerData(innerNetObject.Data)))
            {
                innerNetObject.ReportPlayer(ReportReasons.Cheating_Hacking);
                BetterDataManager.Files.BetterDataFile.AUMData.Add(new(innerNetObject?.ExtendedData().RealName ?? innerNetObject.Data.PlayerName, innerNetObject.GetHashPuid(), innerNetObject.Data.FriendCode, "AUM Chat RPC"));
                BetterDataManager.Files.BetterDataFile.Save();
                BetterNotificationManager.NotifyCheat(innerNetObject, TranslationStrings.AntiCheat_Cheat_AUMChat.LocalizedString, TranslationStrings.AntiCheat_HasBeenDetectedWithCheatClient.LocalizedString);
            }
        }
    }
}
