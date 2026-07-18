using BetterAmongUs.Attributes;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers;

[RegisterRPCHandler]
internal sealed class SendChatNoteHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.SendChatNote;
}
