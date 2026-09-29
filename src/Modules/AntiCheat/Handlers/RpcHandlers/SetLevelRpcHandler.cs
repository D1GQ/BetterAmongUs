using BetterAmongUs.Attributes;
using BetterAmongUs.Enums;
using BetterAmongUs.Generated;
using BetterAmongUs.MonoScripts.Extended;
using BetterAmongUs.Patches.Gameplay.UI.Settings;
using BetterAmongUs.Utilities;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat.Handlers.RpcHandlers;

[RegisterRpcHandler(RpcCalls.SetLevel)]
internal sealed class SetLevelRpcHandler : IRpcHandler<PlayerControl>
{
    public AntiCheatFlags CheckRpc(PlayerControl innerNetObject, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
        if (innerNetObject.DataIsCollected() && innerNetObject.ExtendedData().AntiCheatInfo.HasSetLevel && !GameState.IsLocalGame && GameState.IsVanillaServer)
        {
            /*
            if (BetterNotificationManager.NotifyCheat(sender, GetFormatSetText()))
            {
                LogRpcInfo($"Player attempted to set level multiple times");
            }
            */

            return AntiCheatFlags.High | AntiCheatFlags.Cancel;
        }

        return AntiCheatFlags.None;
    }

    public void HandleRpc(PlayerControl innerNetObject, MessageReader reader)
    {
        uint level = reader.ReadPackedUInt32() + 1;

        if (BetterGameSettings.DetectedLevel.GetBool() && level > BetterGameSettings.DetectedLevelAbove.GetInt())
        {
            /*
            if (BetterNotificationManager.NotifyCheat(sender, TranslationStrings.AntiCheat_InvalidLevelRPC.Format(level)))
            {
                LogRpcInfo($"Suspicious level set: {level} (max allowed: {BetterGameSettings.DetectedLevelAbove.GetInt()})");
            }
            */
        }

        innerNetObject.ExtendedData().AntiCheatInfo.HasSetLevel = true;
    }
}
