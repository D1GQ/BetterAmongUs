using BetterAmongUs.Attributes;
using BetterAmongUs.Utilities;
using Hazel;
using InnerNet;
using UnityEngine;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class CheckMurderHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.CheckMurder;

    internal override bool BetterHandle(PlayerControl? sender, MessageReader reader)
    {
        PlayerControl target = reader.ReadNetObject<PlayerControl>();

        if (target != null)
        {
            if (sender.IsAlive()
                && sender.IsImpostorTeam()
                && !sender.inMovingPlat
                && !sender.IsInVent()
                && !sender.IsInVanish()
                && !sender.shapeshifting
                && !sender.onLadder
                && !sender.MyPhysics.Animations.IsPlayingAnyLadderAnimation()
                && CheckRange(sender.GetCustomPosition(), target.GetCustomPosition(), 3f))
            {
                if (target.IsAlive()
                    && !target.IsImpostorTeam()
                    && !target.inMovingPlat
                    && !target.IsInVent()
                    && !target.onLadder
                    && !target.MyPhysics.Animations.IsPlayingAnyLadderAnimation())
                {
                    sender.RpcMurderPlayer(target, true);
                }
                else
                {
                    string targetIssue = GetTargetIssue(target);
                    LogRpcInfo($"Invalid murder target: {targetIssue}");
                }
            }
            else
            {
                string senderIssue = GetSenderIssue(sender, target);
                LogRpcInfo($"Invalid murder attempt: {senderIssue}");
            }
        }
        else
        {
            LogRpcInfo($"Murder target is null");
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

    private static string GetSenderIssue(PlayerControl sender, PlayerControl target)
    {
        if (!sender.IsAlive()) return "Sender not alive";
        if (!sender.IsImpostorTeam()) return "Sender not impostor team";
        if (sender.inMovingPlat) return "Sender in moving platform";
        if (sender.IsInVent()) return "Sender in vent";
        if (sender.IsInVanish()) return "Sender vanished";
        if (sender.shapeshifting) return "Sender shapeshifting";
        if (sender.onLadder) return "Sender on ladder";
        if (sender.MyPhysics.Animations.IsPlayingAnyLadderAnimation()) return "Sender in ladder animation";
        if (!CheckRange(sender.GetCustomPosition(), target.GetCustomPosition(), 3f))
            return $"Out of range (distance: {Vector2.Distance(sender.GetCustomPosition(), target.GetCustomPosition())})";

        return "Unknown sender issue";
    }

    private string GetTargetIssue(PlayerControl target)
    {
        if (!target.IsAlive()) return "Target not alive";
        if (target.IsImpostorTeam()) return "Target is impostor team";
        if (target.inMovingPlat) return "Target in moving platform";
        if (target.IsInVent()) return "Target in vent";
        if (target.onLadder) return "Target on ladder";
        if (target.MyPhysics.Animations.IsPlayingAnyLadderAnimation()) return "Target in ladder animation";

        return "Unknown target issue";
    }
}