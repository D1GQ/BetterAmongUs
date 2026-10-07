using AmongUs.GameOptions;
using BetterAmongUs.Attributes;
using BetterAmongUs.Enums;
using BetterAmongUs.Generated;
using BetterAmongUs.Utilities;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat.Handlers.RpcHandlers;

[RegisterRpcHandler(RpcCalls.StartAppear)]
internal sealed class StartAppearRpcHandler : IRpcHandler<PlayerControl>
{
    public AntiCheatFlags CheckRpc(PlayerControl innerNetObject, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
        bool shouldAnimate = reader.ReadBoolean();

        if (!innerNetObject.Is(RoleTypes.Phantom) || !innerNetObject.IsAlive())
        {
            /*
            if (BetterNotificationManager.NotifyCheat(innerNetObject, GetFormatActionText()))
            {
                string issue = GetRoleCheckIssue(innerNetObject);
                LogRpcInfo($"Invalid vanish role: {issue}");
            }
            */

            return AntiCheatFlags.High | AntiCheatFlags.Cancel;
        }

        if (!shouldAnimate && (!innerNetObject.IsInVent() && !GameState.IsMeeting && !GameState.IsExilling))
        {
            /*
            if (BetterNotificationManager.NotifyCheat(innerNetObject, GetFormatActionText()))
            {
                LogRpcInfo($"Phantom attempted to appear without animation while not in vent");
                innerNetObject.HandleServerAppear(true);
            }
            */

            return AntiCheatFlags.High | AntiCheatFlags.Cancel;
        }

        return AntiCheatFlags.None;
    }

    public void HandleRpc(PlayerControl innerNetObject, MessageReader reader)
    {
    }
}
