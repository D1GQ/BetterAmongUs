using AmongUs.GameOptions;
using BetterAmongUs.Attributes;
using BetterAmongUs.Enums;
using BetterAmongUs.Generated;
using BetterAmongUs.Utilities;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat.Handlers.RpcHandlers;

[RegisterRpcHandler(RpcCalls.CheckShapeshift)]
internal sealed class CheckShapeshiftRpcHandler : IRpcHandler<PlayerControl>
{
    public AntiCheatFlags CheckRpc(PlayerControl innerNetObject, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
        if (!GameState.IsHost)
        {
            return AntiCheatFlags.Cancel;
        }

        _ = reader.ReadNetObject<PlayerControl>();
        bool flag = reader.ReadBoolean();

        if (!innerNetObject.Is(RoleTypes.Shapeshifter)
            || !innerNetObject.IsAlive()
            || !innerNetObject.IsImpostorTeam()
            || innerNetObject.inMovingPlat
            || innerNetObject.shapeshifting
            || innerNetObject.onLadder
            || innerNetObject.MyPhysics.Animations.IsPlayingAnyLadderAnimation())
        {
            return AntiCheatFlags.Cancel;
        }

        if (!innerNetObject.IsInVent() && !GameState.IsMeeting && !GameState.IsExilling && flag == false)
        {
            return AntiCheatFlags.Cancel;
        }

        return AntiCheatFlags.None;
    }

    public void HandleRpc(PlayerControl innerNetObject, MessageReader reader)
    {
    }
}
