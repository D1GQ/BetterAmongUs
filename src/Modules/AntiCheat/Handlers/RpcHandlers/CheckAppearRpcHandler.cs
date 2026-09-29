using AmongUs.GameOptions;
using BetterAmongUs.Attributes;
using BetterAmongUs.Enums;
using BetterAmongUs.Generated;
using BetterAmongUs.Utilities;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat.Handlers.RpcHandlers;

[RegisterRpcHandler(RpcCalls.CheckAppear)]
internal sealed class CheckAppearHandler : IRpcHandler<PlayerControl>
{
    public AntiCheatFlags CheckRpc(PlayerControl innerNetObject, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
        if (!GameState.IsHost)
        {
            return AntiCheatFlags.Cancel;
        }

        bool shouldAnimate = reader.ReadBoolean();
        if (innerNetObject.Is(RoleTypes.Phantom)
            && innerNetObject.IsAlive() && innerNetObject.IsImpostorTeam()
            && !innerNetObject.inMovingPlat
            && !innerNetObject.onLadder
            && !innerNetObject.MyPhysics.Animations.IsPlayingAnyLadderAnimation())
        {
            if (!innerNetObject.IsInVent() && shouldAnimate == false)
            {
                // LogRpcInfo($"Phantom attempted to appear without animation while not in vent.");
                return AntiCheatFlags.Cancel | AntiCheatFlags.Medium;
            }
        }

        return AntiCheatFlags.None;
    }

    public void HandleRpc(PlayerControl innerNetObject, MessageReader reader)
    {
    }
}
