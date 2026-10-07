using BetterAmongUs.Attributes;
using BetterAmongUs.Enums;
using BetterAmongUs.Generated;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat.Handlers.RpcHandlers;

[RegisterRpcHandler(RpcCalls.Pet)]
internal sealed class PetRpcHandler : IRpcHandler<PlayerPhysics>
{
    public AntiCheatFlags CheckRpc(PlayerPhysics innerNetObject, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
        return AntiCheatFlags.None;
    }

    public void HandleRpc(PlayerPhysics innerNetObject, MessageReader reader)
    {
        if (innerNetObject.myPlayer == null)
            return;

        if (innerNetObject.myPlayer.CurrentOutfit == null)
            return;

        if (innerNetObject.myPlayer.CurrentOutfit.PetId == PetData.EmptyId)
        {
            /*
            if (BetterNotificationManager.NotifyCheat(innerNetObject.myPlayer, GetFormatActionText()))
            {
                LogRpcInfo($"Player with empty pet attempted Pet RPC");
            }
            */
        }
    }
}
