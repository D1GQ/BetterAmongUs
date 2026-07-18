using BetterAmongUs.Attributes;
using BetterAmongUs.Utilities;
using BetterAmongUs.Managers;
using Hazel;
using BetterAmongUs.MonoScripts.Extended;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class SetNameHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SetName;

    internal override bool HandleAntiCheatCancel(PlayerControl? sender, MessageReader reader)
    {
        if (GameState.IsHost) return true;

        _ = reader.ReadUInt32();
        var name = reader.ReadString();

        if (sender.DataIsCollected() == true && sender.ExtendedData().AntiCheatInfo.HasSetName && !GameState.IsLocalGame && GameState.IsVanillaServer)
        {
            if (BetterNotificationManager.NotifyCheat(sender, GetFormatSetText()))
            {
                Utils.AddChatPrivate($"{sender.GetPlayerNameAndColor()} Has tried to change their name to '{name}' but has been undone!");
                Logger_.LogCheat($"{sender.ExtendedData().RealName} Has tried to change their name to '{name}' but has been undone!");
                LogRpcInfo($"Player attempted to change name multiple times: '{name}'");
            }

            return false;
        }

        sender.ExtendedData().AntiCheatInfo.HasSetName = true;

        return true;
    }
}
