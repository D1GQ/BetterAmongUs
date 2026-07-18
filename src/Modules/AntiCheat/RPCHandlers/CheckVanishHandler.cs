using AmongUs.GameOptions;
using BetterAmongUs.Attributes;
using BetterAmongUs.Utilities;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class CheckVanishHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.CheckVanish;

    internal override bool BetterHandle(PlayerControl? player, MessageReader reader)
    {
        if (player.Is(RoleTypes.Phantom)
            && player.IsAlive()
            && player.IsImpostorTeam()
            && !player.IsInVent()
            && !player.inMovingPlat
            && !player.onLadder
            && !player.MyPhysics.Animations.IsPlayingAnyLadderAnimation())
        {

            if (AmongUsClient.Instance.AmClient)
            {
                player.SetRoleInvisibility(true, true, true);
            }
            player.RpcVanish();
        }
        else
        {
            string issue = GetVanishIssue(player);
            LogRpcInfo($"Invalid vanish attempt: {issue}");
        }

        return false;
    }

    internal override bool HandleAntiCheatCancel(PlayerControl? sender, MessageReader reader)
    {
        if (!GameState.IsHost)
        {
            return false;
        }

        return true;
    }

    private static string GetVanishIssue(PlayerControl player)
    {
        if (!player.Is(RoleTypes.Phantom)) return "Player not Phantom role";
        if (!player.IsAlive()) return "Player not alive";
        if (!player.IsImpostorTeam()) return "Player not impostor team";
        if (player.IsInVent()) return "Player in vent";
        if (player.inMovingPlat) return "Player in moving platform";
        if (player.onLadder) return "Player on ladder";
        if (player.MyPhysics.Animations.IsPlayingAnyLadderAnimation()) return "Player in ladder animation";

        return "Unknown vanish issue";
    }
}
