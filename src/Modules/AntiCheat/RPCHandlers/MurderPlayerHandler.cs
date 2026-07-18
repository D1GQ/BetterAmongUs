using AmongUs.GameOptions;
using BetterAmongUs.Attributes;
using BetterAmongUs.Generated;
using BetterAmongUs.Managers;
using BetterAmongUs.MonoScripts.Extended;
using BetterAmongUs.Utilities;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class MurderPlayerHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.MurderPlayer;

    internal override bool HandleAntiCheatCancel(PlayerControl? player, MessageReader reader)
    {
        PlayerControl target = reader.ReadNetObject<PlayerControl>();

        if (target != null)
        {
            if (target.IsLocalPlayer() && GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown) > 2.5f)
            {
                target.ExtendedData().AntiCheatInfo.TimesAttemptedKilled++;

                if (target.ExtendedData().AntiCheatInfo.TimesAttemptedKilled >= 10 && !target.IsAlive())
                {
                    if (BetterNotificationManager.NotifyCheat(player, TranslationStrings.AntiCheat_InvalidAction.Format(TranslationStrings.AntiCheat_TryBanExploit)))
                    {
                        LogRpcInfo($"Ban exploit detected: Player attempted to kill dead target {target.ExtendedData().AntiCheatInfo.TimesAttemptedKilled} times");
                    }
                    return false;
                }

                // Cancel murder on client if not alive
                if (!target.IsAlive())
                {
                    LogRpcInfo($"Murder blocked: Target {target.ExtendedData()?.RealName} is not alive");
                    return false;
                }
            }
        }

        return true;
    }

    internal override void HandleAntiCheat(PlayerControl? sender, MessageReader reader)
    {
        PlayerControl target = reader.ReadNetObject<PlayerControl>();

        if (target != null)
        {
            if (!sender.IsImpostorTeam() || !sender.IsAlive() || sender.IsInVanish() || target.IsImpostorTeam())
            {
                if (BetterNotificationManager.NotifyCheat(sender, GetFormatActionText()))
                {
                    string issue = GetMurderIssue(sender, target);
                    LogRpcInfo($"Invalid murder: {issue}");
                }
            }
        }
    }

    private static string GetMurderIssue(PlayerControl sender, PlayerControl target)
    {
        if (!sender.IsImpostorTeam()) return "Sender not impostor team";
        if (!sender.IsAlive()) return "Sender not alive";
        if (sender.IsInVanish()) return "Sender is vanished";
        if (target.IsImpostorTeam()) return "Target is impostor team";

        return "Unknown murder issue";
    }
}
