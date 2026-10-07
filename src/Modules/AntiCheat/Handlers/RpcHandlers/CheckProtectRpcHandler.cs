using AmongUs.GameOptions;
using BetterAmongUs.Attributes;
using BetterAmongUs.Enums;
using BetterAmongUs.Generated;
using BetterAmongUs.Utilities;
using Hazel;
using InnerNet;
using UnityEngine;

namespace BetterAmongUs.Modules.AntiCheat.Handlers.RpcHandlers;

[RegisterRpcHandler(RpcCalls.CheckProtect)]
internal sealed class CheckProtectRpcHandler : IRpcHandler<PlayerControl>
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
            if (!target.IsAlive())
            {
                return AntiCheatFlags.Cancel;
            }
        }

        if (!innerNetObject.Is(RoleTypes.GuardianAngel)
            || innerNetObject.IsAlive()
            || innerNetObject.IsImpostorTeam()
            || Vector2.Distance(innerNetObject.GetCustomPosition(), target.GetCustomPosition()) > 4f)
        {
            return AntiCheatFlags.Cancel;
        }

        return AntiCheatFlags.None;
    }

    public void HandleRpc(PlayerControl innerNetObject, MessageReader reader)
    {
    }
}
