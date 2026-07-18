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
internal sealed class ModMenuCrewHandler : RPCHandler
{
    internal override byte CallId => unchecked((byte)CustomRPC.ModMenuCrew);

    internal override void HandleCheatRpcCheck(PlayerControl? sender, MessageReader reader)
    {
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_Anticheat))
            return;

        if (!BAUConfigs.AntiCheat.Value || !BetterGameSettings.DetectCheatClients.GetBool())
            return;

        try
        {
            var mccSignature = reader.ReadString();
            var playerId = reader.ReadByte();
            var version = reader.ReadString();

            if (sender.PlayerId != playerId)
                return;

            if (!BetterDataManager.Files.BetterDataFile.MMCData.Any(info => info.CheckPlayerData(sender.Data)))
            {
                sender.ReportPlayer(ReportReasons.Cheating_Hacking);
                BetterDataManager.Files.BetterDataFile.MMCData.Add(new(sender?.ExtendedData().RealName ?? sender.Data.PlayerName, sender.GetHashPuid(), sender.Data.FriendCode, "ModMenuCrew RPC"));
                BetterDataManager.Files.BetterDataFile.Save();
                BetterNotificationManager.NotifyCheat(sender, TranslationStrings.AntiCheat_Cheat_MMC.LocalizedString, TranslationStrings.AntiCheat_HasBeenDetectedWithCheatClient.LocalizedString);
            }
        }
        catch
        {
            if (!BetterDataManager.Files.BetterDataFile.MMCData.Any(info => info.CheckPlayerData(sender.Data)))
            {
                sender.ReportPlayer(ReportReasons.Cheating_Hacking);
                BetterDataManager.Files.BetterDataFile.MMCData.Add(new(sender?.ExtendedData().RealName ?? sender.Data.PlayerName, sender.GetHashPuid(), sender.Data.FriendCode, "ModMenuCrew RPC"));
                BetterDataManager.Files.BetterDataFile.Save();
                BetterNotificationManager.NotifyCheat(sender, TranslationStrings.AntiCheat_Cheat_MMC.LocalizedString, TranslationStrings.AntiCheat_HasBeenDetectedWithCheatClient.LocalizedString);
            }
        }
    }
}