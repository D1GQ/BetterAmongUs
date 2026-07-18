using AmongUs.GameOptions;
using BetterAmongUs.Attributes;
using BetterAmongUs.Utilities;
using BetterAmongUs.Managers;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class ShapeshiftHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.Shapeshift;

    internal override bool HandleAntiCheatCancel(PlayerControl? sender, MessageReader reader)
    {
        var target = reader.ReadNetObject<PlayerControl>();
        var flag = reader.ReadBoolean();

        if (!sender.Is(RoleTypes.Shapeshifter) || !sender.IsAlive())
        {
            if (BetterNotificationManager.NotifyCheat(sender, GetFormatActionText()))
            {
                string issue = GetShapeshiftRoleIssue(sender);
                LogRpcInfo($"Invalid shapeshift: {issue}");
            }
            return false;
        }
        else if (!flag && !GameState.IsMeeting && !GameState.IsExilling && !sender.IsInVent())
        {
            if (BetterNotificationManager.NotifyCheat(sender, GetFormatActionText()))
            {
                LogRpcInfo($"Shapeshifter attempted to shapeshift without animation while not in vent");
            }
            return false;
        }

        return true;
    }

    private static string GetShapeshiftRoleIssue(PlayerControl sender)
    {
        if (!sender.Is(RoleTypes.Shapeshifter)) return "Sender not Shapeshifter role";
        if (!sender.IsAlive()) return "Sender not alive";

        return "Unknown shapeshift role issue";
    }
}
