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
internal sealed class SickoHandler : RPCHandler
{
    internal override byte CallId => unchecked((byte)CustomRPC.Sicko);

    internal override void HandleCheatRpcCheck(PlayerControl? sender, MessageReader reader)
    {
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_Anticheat))
            return;

        if (!BAUConfigs.AntiCheat.Value || !BetterGameSettings.DetectCheatClients.GetBool())
            return;

        if (reader.BytesRemaining == 0 && !BetterDataManager.Files.BetterDataFile.SickoData.Any(info => info.CheckPlayerData(sender.Data)))
        {
            sender.ReportPlayer(ReportReasons.Cheating_Hacking);
            BetterDataManager.Files.BetterDataFile.SickoData.Add(new(sender?.ExtendedData().RealName ?? sender.Data.PlayerName, sender.GetHashPuid(), sender.Data.FriendCode, "Sicko RPC"));
            BetterDataManager.Files.BetterDataFile.Save();
            BetterNotificationManager.NotifyCheat(sender, TranslationStrings.AntiCheat_Cheat_Sicko.LocalizedString, TranslationStrings.AntiCheat_HasBeenDetectedWithCheatClient.LocalizedString);
        }
    }
}