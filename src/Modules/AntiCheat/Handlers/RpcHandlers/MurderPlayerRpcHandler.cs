using AmongUs.GameOptions;
using BetterAmongUs.Attributes;
using BetterAmongUs.Enums;
using BetterAmongUs.Generated;
using BetterAmongUs.MonoScripts.Extended;
using BetterAmongUs.Utilities;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Modules.AntiCheat.Handlers.RpcHandlers;

[RegisterRpcHandler(RpcCalls.MurderPlayer)]
internal sealed class MurderPlayerRpcHandler : IRpcHandler<PlayerControl>
{
    public AntiCheatFlags CheckRpc(PlayerControl innerNetObject, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
        PlayerControl target = reader.ReadNetObject<PlayerControl>();

        if (target != null)
        {
            if (target.IsLocalPlayer() && GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown) > 2.5f)
            {
                target.ExtendedData().AntiCheatInfo.TimesAttemptedKilled++;

                if (target.ExtendedData().AntiCheatInfo.TimesAttemptedKilled >= 10 && !target.IsAlive())
                {
                    /*
                    if (BetterNotificationManager.NotifyCheat(player, TranslationStrings.AntiCheat_InvalidAction.Format(TranslationStrings.AntiCheat_TryBanExploit)))
                    {
                        LogRpcInfo($"Ban exploit detected: Player attempted to kill dead target {target.ExtendedData().AntiCheatInfo.TimesAttemptedKilled} times");
                    }
                    */

                    return AntiCheatFlags.High | AntiCheatFlags.Cancel;
                }

                // Cancel murder on client if not alive
                if (!target.IsAlive())
                {
                    // LogRpcInfo($"Murder blocked: Target {target.ExtendedData()?.RealName} is not alive");
                    return AntiCheatFlags.Medium;
                }
            }

            if (!innerNetObject.IsImpostorTeam() || !innerNetObject.IsAlive() || innerNetObject.IsInVanish() || target.IsImpostorTeam())
            {
                /*
                if (BetterNotificationManager.NotifyCheat(innerNetObject, GetFormatActionText()))
                {
                    string issue = GetMurderIssue(innerNetObject, target);
                    LogRpcInfo($"Invalid murder: {issue}");
                }
                */

                return AntiCheatFlags.Medium;
            }
        }

        return AntiCheatFlags.None;
    }

    public void HandleRpc(PlayerControl innerNetObject, MessageReader reader)
    {
    }
}
