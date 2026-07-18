using BetterAmongUs.Attributes;
using BetterAmongUs.Utilities;
using BetterAmongUs.Managers;
using Hazel;
using BetterAmongUs.MonoScripts.Extended;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class CheckNameHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.CheckName;

    internal override bool HandleAntiCheatCancel(PlayerControl? sender, MessageReader reader)
    {
        if (!GameState.IsHost)
        {
            return false;
        }

        var name = reader.ReadString();

        if (sender.DataIsCollected() == true && sender.ExtendedData().AntiCheatInfo.HasSetName && !GameState.IsLocalGame && GameState.IsVanillaServer)
        {
            if (BetterNotificationManager.NotifyCheat(sender, GetFormatSetText()))
            {
                Utils.AddChatPrivate($"{sender.GetPlayerNameAndColor()} Has tried to change their name to '{name}' but has been undone!");
                Logger_.LogCheat($"{sender.ExtendedData().RealName} Has tried to change their name to '{name}' but has been undone!");
                LogRpcInfo($"{sender.DataIsCollected() == true} && {!GameState.IsLocalGame} && {GameState.IsVanillaServer}");
            }

            return false;
        }

        sender.ExtendedData().AntiCheatInfo.HasSetName = true;

        return true;
    }
}