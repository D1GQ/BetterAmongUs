using BetterAmongUs.Attributes;
using BetterAmongUs.Utilities;
using BetterAmongUs.Managers;
using Hazel;
using BetterAmongUs.Utilities.Extension;
using BetterAmongUs.MonoScripts.Extended;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class CompleteTaskHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.CompleteTask;

    internal override void HandleAntiCheat(PlayerControl? sender, MessageReader reader)
    {
        var taskId = reader.ReadPackedUInt32();

        if (sender.IsImpostorTeam() || !sender.Data.Tasks.AnyIl2Cpp(task => task.Id == taskId)
            || sender.ExtendedData().AntiCheatInfo.LastTaskId == taskId || sender.ExtendedData().AntiCheatInfo.LastTaskId != taskId
            && sender.ExtendedData().AntiCheatInfo.TimeSinceLastTask < 1.25f)
        {
            if (BetterNotificationManager.NotifyCheat(sender, GetFormatActionText()))
            {
                string reason = GetCompleteTaskIssue(sender, taskId);
                LogRpcInfo($"Invalid task completion: {reason}");
            }
        }

        sender.ExtendedData().AntiCheatInfo.TimeSinceLastTask = 0f;
        sender.ExtendedData().AntiCheatInfo.LastTaskId = taskId;
    }

    private static string GetCompleteTaskIssue(PlayerControl sender, uint taskId)
    {
        if (sender.IsImpostorTeam()) return "Impostor completing tasks";
        if (!sender.Data.Tasks.AnyIl2Cpp(task => task.Id == taskId)) return $"Task ID {taskId} not assigned to player";
        if (sender.ExtendedData().AntiCheatInfo.LastTaskId == taskId) return $"Task ID {taskId} already completed";
        if (sender.ExtendedData().AntiCheatInfo.TimeSinceLastTask < 1.25f)
            return $"Tasks too fast (time since last: {sender.ExtendedData().AntiCheatInfo.TimeSinceLastTask}s)";

        return "Unknown task completion issue";
    }
}
