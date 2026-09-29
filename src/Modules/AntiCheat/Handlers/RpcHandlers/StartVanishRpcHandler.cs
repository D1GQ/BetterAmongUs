using AmongUs.GameOptions;
using BetterAmongUs.Attributes;
using BetterAmongUs.Enums;
using BetterAmongUs.Generated;
using BetterAmongUs.Utilities;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat.Handlers.RpcHandlers;

[RegisterRpcHandler(RpcCalls.StartVanish)]
internal sealed class StartVanishRpcHandler : IRpcHandler<PlayerControl>
{
    public AntiCheatFlags CheckRpc(PlayerControl innerNetObject, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
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

        if (innerNetObject.IsInVent())
        {
            /*
            if (BetterNotificationManager.NotifyCheat(sender, GetFormatActionText()))
            {
                LogRpcInfo($"Phantom attempted to vanish while in vent");
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
