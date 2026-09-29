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

[RegisterRpcHandler(CustomRpc.AUM)]
internal sealed class AUMRpcHandler : IRpcHandler<PlayerControl>
{
    public AntiCheatFlags CheckRpc(PlayerControl innerNetObject, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
        return AntiCheatFlags.None;
    }

    public void HandleRpc(PlayerControl innerNetObject, MessageReader reader)
    {
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_Anticheat))
            return;

        if (!BAUConfigs.AntiCheat.Value || !BetterGameSettings.DetectCheatClients.GetBool())
            return;

        try
        {
            var aumid = reader.ReadByte();

            if (aumid == innerNetObject.PlayerId)
            {
                if (!BetterDataManager.Files.BetterDataFile.AUMData.Any(info => info.CheckPlayerData(innerNetObject.Data)))
                {
                    innerNetObject.ReportPlayer(ReportReasons.Cheating_Hacking);
                    BetterDataManager.Files.BetterDataFile.AUMData.Add(new(innerNetObject?.ExtendedData().RealName ?? innerNetObject.Data.PlayerName, innerNetObject.GetHashPuid(), innerNetObject.Data.FriendCode, "AUM RPC"));
                    BetterDataManager.Files.BetterDataFile.Save();
                    BetterNotificationManager.NotifyCheat(innerNetObject, TranslationStrings.AntiCheat_Cheat_AUM.LocalizedString, TranslationStrings.AntiCheat_HasBeenDetectedWithCheatClient.LocalizedString);
                }
            }
        }
        catch
        {
            if (!BetterDataManager.Files.BetterDataFile.AUMData.Any(info => info.CheckPlayerData(innerNetObject.Data)))
            {
                innerNetObject.ReportPlayer(ReportReasons.Cheating_Hacking);
                BetterDataManager.Files.BetterDataFile.AUMData.Add(new(innerNetObject?.ExtendedData().RealName ?? innerNetObject.Data.PlayerName, innerNetObject.GetHashPuid(), innerNetObject.Data.FriendCode, "AUM RPC"));
                BetterDataManager.Files.BetterDataFile.Save();
                BetterNotificationManager.NotifyCheat(innerNetObject, TranslationStrings.AntiCheat_Cheat_AUM.LocalizedString, TranslationStrings.AntiCheat_HasBeenDetectedWithCheatClient.LocalizedString);
            }
        }
    }
}
