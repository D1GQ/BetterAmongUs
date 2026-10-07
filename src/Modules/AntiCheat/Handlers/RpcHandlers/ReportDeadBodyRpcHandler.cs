using BetterAmongUs.Attributes;
using BetterAmongUs.Enums;
using BetterAmongUs.Generated;
using BetterAmongUs.Patches.Gameplay.UI;
using BetterAmongUs.Utilities;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat.Handlers.RpcHandlers;

[RegisterRpcHandler(RpcCalls.ReportDeadBody)]
internal sealed class ReportDeadBodyRpcHandler : IRpcHandler<PlayerControl>
{
    public AntiCheatFlags CheckRpc(PlayerControl innerNetObject, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
        if (!GameState.IsInGamePlay || !BAUPlugin.AllPlayerControls.All(pc => pc.roleAssigned))
        {
            /*
            if (BetterNotificationManager.NotifyCheat(innerNetObject, TranslationStrings.AntiCheat_InvalidActionRPC.Format(Enum.GetName((RpcCalls)CallId)), forceBan: true))
            {
                LogRpcInfo($"Report dead body blocked: Game not in play or roles not assigned");
            }
            */

            if (GameState.IsHost)
            {
                return AntiCheatFlags.High | AntiCheatFlags.Cancel;
            }
            else
            {
                return AntiCheatFlags.High;
            }
        }

        if (GameState.IsMeeting && MeetingHudPatch.timeOpen > 5f || GameState.IsHideNSeek || innerNetObject.IsInVent() || innerNetObject.shapeshifting
            || innerNetObject.inMovingPlat || innerNetObject.onLadder || innerNetObject.MyPhysics.Animations.IsPlayingAnyLadderAnimation())
        {
            /*
            if (BetterNotificationManager.NotifyCheat(innerNetObject, TranslationStrings.AntiCheat_InvalidActionRPC.Format(Enum.GetName((RpcCalls)CallId))))
            {
                string issue = GetReportBlockedIssue(innerNetObject);
                LogRpcInfo($"Report blocked: {issue}");
            }
            */

            if (GameState.IsHost)
            {
                return AntiCheatFlags.High | AntiCheatFlags.Cancel;
            }
            else
            {
                return AntiCheatFlags.High;
            }
        }

        var deadPlayerInfo = reader.ReadPlayerDataId();
        bool isBodyReport = deadPlayerInfo != null;

        if (isBodyReport)
        {
            if (!deadPlayerInfo.IsDead || deadPlayerInfo == innerNetObject.Data)
            {
                /*
                if (BetterNotificationManager.NotifyCheat(innerNetObject, TranslationStrings.AntiCheat_InvalidActionRPC.Format(Enum.GetName((RpcCalls)CallId))))
                {
                    string issue = GetBodyReportIssue(deadPlayerInfo, innerNetObject);
                    LogRpcInfo($"Invalid body report: {issue}");
                }
                */

                if (GameState.IsHost)
                {
                    return AntiCheatFlags.High | AntiCheatFlags.Cancel;
                }
                else
                {
                    return AntiCheatFlags.High;
                }
            }
        }
        else
        {
            if (innerNetObject.RemainingEmergencies <= 0)
            {
                /*
                if (BetterNotificationManager.NotifyCheat(innerNetObject, TranslationStrings.AntiCheat_InvalidActionRPC.Format(Enum.GetName((RpcCalls)CallId))))
                {
                    LogRpcInfo($"Emergency meeting: No meetings remaining ({innerNetObject.RemainingEmergencies} left)");
                }
                */

                if (GameState.IsHost)
                {
                    return AntiCheatFlags.High | AntiCheatFlags.Cancel;
                }
                else
                {
                    return AntiCheatFlags.High;
                }
            }
        }

        return AntiCheatFlags.None;
    }

    public void HandleRpc(PlayerControl innerNetObject, MessageReader reader)
    {
    }
}
