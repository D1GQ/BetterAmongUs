using BetterAmongUs.Attributes;
using BetterAmongUs.Enums;
using BetterAmongUs.Generated;
using BetterAmongUs.MonoScripts.Extended;
using BetterAmongUs.Utilities;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat.Handlers.RpcHandlers;

[RegisterRpcHandler(RpcCalls.CheckName)]
internal sealed class CheckNameRpcHandler : IRpcHandler<PlayerControl>
{
    public AntiCheatFlags CheckRpc(PlayerControl innerNetObject, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
        if (!GameState.IsHost)
        {
            return AntiCheatFlags.Cancel;
        }

        var name = reader.ReadString();

        if (innerNetObject.DataIsCollected() == true && innerNetObject.ExtendedData().AntiCheatInfo.HasSetName && !GameState.IsLocalGame && GameState.IsVanillaServer)
        {
            /*
            if (BetterNotificationManager.NotifyCheat(innerNetObject, GetFormatSetText()))
            {
                Utils.AddChatPrivate($"{innerNetObject.GetPlayerNameAndColor()} Has tried to change their name to '{name}' but has been undone!");
                BAUPlugin.Logger.LogCheat($"{innerNetObject.ExtendedData().RealName} Has tried to change their name to '{name}' but has been undone!");
                LogRpcInfo($"{innerNetObject.DataIsCollected() == true} && {!GameState.IsLocalGame} && {GameState.IsVanillaServer}");
            }
            */

            return AntiCheatFlags.Cancel | AntiCheatFlags.High;
        }

        return AntiCheatFlags.None;
    }

    public void HandleRpc(PlayerControl innerNetObject, MessageReader reader)
    {
        innerNetObject.ExtendedData().AntiCheatInfo.HasSetName = true;
    }
}
