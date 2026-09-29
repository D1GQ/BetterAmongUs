using BetterAmongUs.Attributes;
using BetterAmongUs.Enums;
using BetterAmongUs.Generated;
using BetterAmongUs.MonoScripts.Extended;
using BetterAmongUs.Utilities;
using BetterAmongUs.Utilities.Extension;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat.Handlers.RpcHandlers;

[RegisterRpcHandler(RpcCalls.CompleteTask)]
internal sealed class CompleteTaskRpcHandler : IRpcHandler<PlayerControl>
{
    public AntiCheatFlags CheckRpc(PlayerControl innerNetObject, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
        var taskId = reader.ReadPackedUInt32();

        if (innerNetObject.IsImpostorTeam() || !innerNetObject.Data.Tasks.AnyIl2Cpp(task => task.Id == taskId)
            || innerNetObject.ExtendedData().AntiCheatInfo.LastTaskId == taskId || innerNetObject.ExtendedData().AntiCheatInfo.LastTaskId != taskId
            && innerNetObject.ExtendedData().AntiCheatInfo.TimeSinceLastTask < 1.25f)
        {
            /*
            if (BetterNotificationManager.NotifyCheat(innerNetObject, GetFormatActionText()))
            {
                string reason = GetCompleteTaskIssue(innerNetObject, taskId);
                LogRpcInfo($"Invalid task completion: {reason}");
            }
            */
            return AntiCheatFlags.High | AntiCheatFlags.Cancel;
        }

        return AntiCheatFlags.None;
    }

    public void HandleRpc(PlayerControl innerNetObject, MessageReader reader)
    {
        var taskId = reader.ReadPackedUInt32();

        innerNetObject.ExtendedData().AntiCheatInfo.TimeSinceLastTask = 0f;
        innerNetObject.ExtendedData().AntiCheatInfo.LastTaskId = taskId;
    }
}
