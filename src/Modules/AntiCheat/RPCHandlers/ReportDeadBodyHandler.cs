using BetterAmongUs.Attributes;
using BetterAmongUs.Generated;
using BetterAmongUs.Managers;
using BetterAmongUs.Patches.Gameplay.UI;
using BetterAmongUs.Utilities;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class ReportDeadBodyHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.ReportDeadBody;

    internal override bool HandleAntiCheatCancel(PlayerControl? sender, MessageReader reader)
    {
        if (!GameState.IsInGamePlay || !BAUPlugin.AllPlayerControls.All(pc => pc.roleAssigned))
        {
            if (BetterNotificationManager.NotifyCheat(sender, TranslationStrings.AntiCheat_InvalidActionRPC.Format(Enum.GetName((RpcCalls)CallId)), forceBan: true))
            {
                LogRpcInfo($"Report dead body blocked: Game not in play or roles not assigned");
            }

            return CancelAsHost;
        }

        if (GameState.IsMeeting && MeetingHudPatch.timeOpen > 5f || GameState.IsHideNSeek || sender.IsInVent() || sender.shapeshifting
            || sender.inMovingPlat || sender.onLadder || sender.MyPhysics.Animations.IsPlayingAnyLadderAnimation())
        {
            if (BetterNotificationManager.NotifyCheat(sender, TranslationStrings.AntiCheat_InvalidActionRPC.Format(Enum.GetName((RpcCalls)CallId))))
            {
                string issue = GetReportBlockedIssue(sender);
                LogRpcInfo($"Report blocked: {issue}");
            }

            return CancelAsHost;
        }

        var deadPlayerInfo = reader.ReadPlayerDataId();
        bool isBodyReport = deadPlayerInfo != null;

        if (isBodyReport)
        {
            if (!deadPlayerInfo.IsDead || deadPlayerInfo == sender.Data)
            {
                if (BetterNotificationManager.NotifyCheat(sender, TranslationStrings.AntiCheat_InvalidActionRPC.Format(Enum.GetName((RpcCalls)CallId))))
                {
                    string issue = GetBodyReportIssue(deadPlayerInfo, sender);
                    LogRpcInfo($"Invalid body report: {issue}");
                }

                return CancelAsHost;
            }
        }
        else
        {
            if (sender.RemainingEmergencies <= 0)
            {
                if (BetterNotificationManager.NotifyCheat(sender, TranslationStrings.AntiCheat_InvalidActionRPC.Format(Enum.GetName((RpcCalls)CallId))))
                {
                    LogRpcInfo($"Emergency meeting: No meetings remaining ({sender.RemainingEmergencies} left)");
                }

                return CancelAsHost;
            }
        }

        return true;
    }

    private static string GetReportBlockedIssue(PlayerControl sender)
    {
        if (GameState.IsMeeting && MeetingHudPatch.timeOpen > 5f) return "Meeting already in progress";
        if (GameState.IsHideNSeek) return "Hide and Seek mode";
        if (sender.IsInVent()) return "Sender in vent";
        if (sender.shapeshifting) return "Sender shapeshifting";
        if (sender.inMovingPlat) return "Sender in moving platform";
        if (sender.onLadder) return "Sender on ladder";
        if (sender.MyPhysics.Animations.IsPlayingAnyLadderAnimation()) return "Sender in ladder animation";

        return "Unknown report blocked issue";
    }

    private static string GetBodyReportIssue(NetworkedPlayerInfo deadPlayerInfo, PlayerControl sender)
    {
        if (!deadPlayerInfo.IsDead) return "Reported player is not dead";
        if (deadPlayerInfo == sender.Data) return "Cannot report yourself";

        return "Unknown body report issue";
    }
}
