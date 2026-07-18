using BetterAmongUs.Attributes;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class SetScannerHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SetScanner;

    internal override void HandleAntiCheat(PlayerControl? sender, MessageReader reader)
    {
        /*
        if (sender.IsImpostorTeam() || !sender.Data.Tasks.ToArray().Any(task => task.TypeId == (byte)TaskTypes.SubmitScan))
        {
            BetterNotificationManager.NotifyCheat(sender, GetFormatActionText());
            LogRpcInfo($"{sender.IsImpostorTeam()} || {!sender.Data.Tasks.ToArray().Any(task => task.TypeId == (byte)TaskTypes.SubmitScan)}");
        }
        */
    }
}
