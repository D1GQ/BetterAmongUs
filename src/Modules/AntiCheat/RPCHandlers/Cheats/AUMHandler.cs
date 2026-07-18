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
internal sealed class AUMHandler : RPCHandler
{
    internal override byte CallId => unchecked((byte)CustomRPC.AUM);

    internal override void HandleCheatRpcCheck(PlayerControl? sender, MessageReader reader)
    {
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_Anticheat))
            return;

        if (!BAUConfigs.AntiCheat.Value || !BetterGameSettings.DetectCheatClients.GetBool())
            return;

        try
        {
            var aumid = reader.ReadByte();

            if (aumid == sender.PlayerId)
            {
                if (!BetterDataManager.Files.BetterDataFile.AUMData.Any(info => info.CheckPlayerData(sender.Data)))
                {
                    sender.ReportPlayer(ReportReasons.Cheating_Hacking);
                    BetterDataManager.Files.BetterDataFile.AUMData.Add(new(sender?.ExtendedData().RealName ?? sender.Data.PlayerName, sender.GetHashPuid(), sender.Data.FriendCode, "AUM RPC"));
                    BetterDataManager.Files.BetterDataFile.Save();
                    BetterNotificationManager.NotifyCheat(sender, TranslationStrings.AntiCheat_Cheat_AUM.LocalizedString, TranslationStrings.AntiCheat_HasBeenDetectedWithCheatClient.LocalizedString);
                }
            }
        }
        catch
        {
            if (!BetterDataManager.Files.BetterDataFile.AUMData.Any(info => info.CheckPlayerData(sender.Data)))
            {
                sender.ReportPlayer(ReportReasons.Cheating_Hacking);
                BetterDataManager.Files.BetterDataFile.AUMData.Add(new(sender?.ExtendedData().RealName ?? sender.Data.PlayerName, sender.GetHashPuid(), sender.Data.FriendCode, "AUM RPC"));
                BetterDataManager.Files.BetterDataFile.Save();
                BetterNotificationManager.NotifyCheat(sender, TranslationStrings.AntiCheat_Cheat_AUM.LocalizedString, TranslationStrings.AntiCheat_HasBeenDetectedWithCheatClient.LocalizedString);
            }
        }
    }
}