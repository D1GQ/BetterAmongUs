using AmongUs.GameOptions;
using BetterAmongUs.Attributes;
using BetterAmongUs.Enums;
using BetterAmongUs.Generated;
using BetterAmongUs.Utilities;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat.Handlers.RpcHandlers;

[RegisterRpcHandler(RpcCalls.ExitVent)]
internal sealed class ExitVentRpcHandler : IRpcHandler<PlayerPhysics>
{
    public AntiCheatFlags CheckRpc(PlayerPhysics innerNetObject, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
        if (!innerNetObject.myPlayer.IsImpostorTeam() && !innerNetObject.myPlayer.Is(RoleTypes.Engineer))
        {
            /*
            if (BetterNotificationManager.NotifyCheat(sender, GetFormatActionText()))
            {
                LogRpcInfo($"Non-impostor and non-engineer attempted EnterVent RPC");
            }
            */
            return AntiCheatFlags.High | AntiCheatFlags.Cancel;
        }

        return AntiCheatFlags.None;
    }

    public void HandleRpc(PlayerPhysics innerNetObject, MessageReader reader)
    {
    }
}
