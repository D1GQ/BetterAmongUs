using AmongUs.GameOptions;
using BetterAmongUs.Attributes;
using BetterAmongUs.Enums;
using BetterAmongUs.Generated;
using BetterAmongUs.Utilities;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat.Handlers.RpcHandlers;

[RegisterRpcHandler(RpcCalls.CheckVanish)]
internal sealed class CheckVanishRpcHandler : IRpcHandler<PlayerControl>
{
    public AntiCheatFlags CheckRpc(PlayerControl innerNetObject, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
        if (!GameState.IsHost)
        {
            return AntiCheatFlags.Cancel;
        }

        if (!innerNetObject.Is(RoleTypes.Phantom)
            || !innerNetObject.IsAlive()
            || !innerNetObject.IsImpostorTeam()
            || innerNetObject.IsInVent()
            || innerNetObject.inMovingPlat
            || innerNetObject.onLadder
            || innerNetObject.MyPhysics.Animations.IsPlayingAnyLadderAnimation())
        {
            return AntiCheatFlags.Cancel;
        }

        return AntiCheatFlags.None;
    }

    public void HandleRpc(PlayerControl innerNetObject, MessageReader reader)
    {
    }
}
