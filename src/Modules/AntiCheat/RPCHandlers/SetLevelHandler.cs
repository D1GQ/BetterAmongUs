using BetterAmongUs.Attributes;
using BetterAmongUs.Generated;
using BetterAmongUs.Managers;
using BetterAmongUs.MonoScripts.Extended;
using BetterAmongUs.Patches.Gameplay.UI.Settings;
using BetterAmongUs.Utilities;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class SetLevelHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SetLevel;

    internal override bool HandleAntiCheatCancel(PlayerControl? sender, MessageReader reader)
    {
        if (sender.DataIsCollected() == true && sender.ExtendedData().AntiCheatInfo.HasSetLevel && !GameState.IsLocalGame && GameState.IsVanillaServer)
        {
            if (BetterNotificationManager.NotifyCheat(sender, GetFormatSetText()))
            {
                LogRpcInfo($"Player attempted to set level multiple times");
            }

            return false;
        }

        sender.ExtendedData().AntiCheatInfo.HasSetLevel = true;

        return true;
    }

    internal override void HandleAntiCheat(PlayerControl? sender, MessageReader reader)
    {
        uint level = reader.ReadPackedUInt32() + 1;

        if (BetterGameSettings.DetectedLevel.GetBool() && level > BetterGameSettings.DetectedLevelAbove.GetInt())
        {
            if (BetterNotificationManager.NotifyCheat(sender, TranslationStrings.AntiCheat_InvalidLevelRPC.Format(level)))
            {
                LogRpcInfo($"Suspicious level set: {level} (max allowed: {BetterGameSettings.DetectedLevelAbove.GetInt()})");
            }
        }
    }
}
