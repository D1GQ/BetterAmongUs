using BetterAmongUs.Attributes;
using BetterAmongUs.Enums;
using BetterAmongUs.Generated;
using BetterAmongUs.Utilities;
using Hazel;
using InnerNet;
using UnityEngine;

namespace BetterAmongUs.Modules.AntiCheat.Handlers.RpcHandlers;

[RegisterRpcHandler(RpcCalls.CheckMurder)]
internal sealed class CheckMurderRpcHandler : IRpcHandler<PlayerControl>
{
    public AntiCheatFlags CheckRpc(PlayerControl innerNetObject, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
        if (!GameState.IsHost)
        {
            return AntiCheatFlags.Cancel;
        }

        PlayerControl target = reader.ReadNetObject<PlayerControl>();

        if (target != null)
        {
            if (!target.IsAlive()
                || target.IsImpostorTeam()
                || target.inMovingPlat
                || target.IsInVent()
                || target.onLadder
                || target.MyPhysics.Animations.IsPlayingAnyLadderAnimation())
            {
                return AntiCheatFlags.Cancel;
            }
        }

        if (!innerNetObject.IsAlive()
            || !innerNetObject.IsImpostorTeam()
            || innerNetObject.inMovingPlat
            || innerNetObject.IsInVent()
            || innerNetObject.IsInVanish()
            || innerNetObject.shapeshifting
            || innerNetObject.onLadder
            || innerNetObject.MyPhysics.Animations.IsPlayingAnyLadderAnimation()
            || Vector2.Distance(innerNetObject.GetCustomPosition(), target.GetCustomPosition()) > 3f)
        {
            return AntiCheatFlags.Cancel;
        }

        return AntiCheatFlags.None;
    }

    public void HandleRpc(PlayerControl innerNetObject, MessageReader reader)
    {
    }
}
