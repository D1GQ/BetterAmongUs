using BetterAmongUs.Attributes;
using BetterAmongUs.Enums;
using BetterAmongUs.Generated;
using BetterAmongUs.MonoScripts.Extended;
using BetterAmongUs.Utilities;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat.Handlers.RpcHandlers;

[RegisterRpcHandler(RpcCalls.SetName)]
internal sealed class SetNameRpcHandler : IRpcHandler<PlayerControl>
{
    public AntiCheatFlags CheckRpc(PlayerControl innerNetObject, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
        if (GameState.IsHost)
        {
            return AntiCheatFlags.None;
        }

        if (innerNetObject.DataIsCollected() && innerNetObject.ExtendedData().AntiCheatInfo.HasSetName && !GameState.IsLocalGame && GameState.IsVanillaServer)
        {
            /*
            if (BetterNotificationManager.NotifyCheat(innerNetObject, GetFormatSetText()))
            {
                Utils.AddChatPrivate($"{innerNetObject.GetPlayerNameAndColor()} Has tried to change their name to '{name}' but has been undone!");
                BAUPlugin.Logger.LogCheat($"{innerNetObject.ExtendedData().RealName} Has tried to change their name to '{name}' but has been undone!");
                LogRpcInfo($"Player attempted to change name multiple times: '{name}'");
            }
            */

            return AntiCheatFlags.High | AntiCheatFlags.Cancel;
        }

        return AntiCheatFlags.None;
    }

    public void HandleRpc(PlayerControl innerNetObject, MessageReader reader)
    {
        innerNetObject.ExtendedData().AntiCheatInfo.HasSetName = true;
    }
}
