using AmongUs.GameOptions;
using BetterAmongUs.Attributes;
using BetterAmongUs.Utilities;
using Hazel;
using InnerNet;
using UnityEngine;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class CheckProtectHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.CheckProtect;

    internal override bool BetterHandle(PlayerControl? sender, MessageReader reader)
    {
        PlayerControl target = reader.ReadNetObject<PlayerControl>();
        if (target != null)
        {
            if (sender.Is(RoleTypes.GuardianAngel)
                && !sender.IsAlive()
                && !sender.IsImpostorTeam()
                && CheckRange(sender.GetCustomPosition(), target.GetCustomPosition(), 3f))
            {
                if (target.IsAlive())
                {
                    sender.RpcProtectPlayer(target, sender.Data.DefaultOutfit.ColorId);
                }
                else
                {
                    LogRpcInfo($"Invalid protect: Target not alive");
                }
            }
            else
            {
                string senderIssue = GetSenderIssue(sender, target);
                LogRpcInfo($"Invalid protect attempt: {senderIssue}");
            }
        }
        else
        {
            LogRpcInfo($"Protect target is null");
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
        if (!sender.Is(RoleTypes.GuardianAngel)) return "Sender not GuardianAngel role";
        if (sender.IsAlive()) return "Sender is alive (GA should be dead)";
        if (sender.IsImpostorTeam()) return "Sender is impostor team";
        if (!CheckRange(sender.GetCustomPosition(), target.GetCustomPosition(), 3f))
            return $"Out of range (distance: {Vector2.Distance(sender.GetCustomPosition(), target.GetCustomPosition())})";

        return "Unknown sender issue";
    }
}