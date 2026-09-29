using AmongUs.GameOptions;
using BetterAmongUs.Attributes;
using BetterAmongUs.Enums;
using BetterAmongUs.Generated;
using BetterAmongUs.Utilities;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat.Handlers.RpcHandlers;

[RegisterRpcHandler(RpcCalls.Shapeshift)]
internal sealed class ShapeshiftRpcHandler : IRpcHandler<PlayerControl>
{
    public AntiCheatFlags CheckRpc(PlayerControl innerNetObject, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
        var target = reader.ReadNetObject<PlayerControl>();
        var flag = reader.ReadBoolean();

        if (!innerNetObject.Is(RoleTypes.Shapeshifter) || !innerNetObject.IsAlive())
        {
            /*
            if (BetterNotificationManager.NotifyCheat(innerNetObject, GetFormatActionText()))
            {
                string issue = GetShapeshiftRoleIssue(innerNetObject);
                LogRpcInfo($"Invalid shapeshift: {issue}");
            }
            return false;
            */

            return AntiCheatFlags.High | AntiCheatFlags.Cancel;
        }
        else if (!flag && !GameState.IsMeeting && !GameState.IsExilling && !innerNetObject.IsInVent())
        {
            /*
            if (BetterNotificationManager.NotifyCheat(innerNetObject, GetFormatActionText()))
            {
                LogRpcInfo($"Shapeshifter attempted to shapeshift without animation while not in vent");
            }
            return false;
            */

            return AntiCheatFlags.High | AntiCheatFlags.Cancel;
        }

        return AntiCheatFlags.None;
    }

    public void HandleRpc(PlayerControl innerNetObject, MessageReader reader)
    {
    }
}
