using BetterAmongUs.Attributes;
using BetterAmongUs.Utilities;
using BetterAmongUs.Managers;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class StartAppearHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.StartAppear;

    internal override bool HandleAntiCheatCancel(PlayerControl? sender, MessageReader reader)
    {
        bool shouldAnimate = reader.ReadBoolean();
        if (RegisterRPCHandlerAttribute.GetInstance<StartVanishHandler>().RoleCheck(sender) == false)
        {
            return false;
        }

        if (!shouldAnimate && (!sender.IsInVent() && !GameState.IsMeeting && !GameState.IsExilling))
        {
            if (BetterNotificationManager.NotifyCheat(sender, GetFormatActionText()))
            {
                LogRpcInfo($"Phantom attempted to appear without animation while not in vent");
                sender.HandleServerAppear(true);
            }

            return false;
        }

        return true;
    }
}
