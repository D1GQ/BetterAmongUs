using BetterAmongUs.Enums;
using BetterAmongUs.Generated;
using Hazel;
using InnerNet;

internal interface IRpcHandler
{
    AntiCheatFlags TryCheckRpc(InnerNetObject innerNetObject, MessageReader reader, ref TranslationStrings.TranslationString translationReason);
    void TryHandleRpc(InnerNetObject innerNetObject, MessageReader reader);
}

internal interface IRpcHandler<T> : IRpcHandler where T : InnerNetObject
{
    AntiCheatFlags IRpcHandler.TryCheckRpc(InnerNetObject innerNetObject, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
        if (innerNetObject.TryCast<T>() is T casted)
        {
            return CheckRpc(casted, reader, ref translationReason);
        }

        return AntiCheatFlags.None;
    }

    void IRpcHandler.TryHandleRpc(InnerNetObject innerNetObject, MessageReader reader)
    {
        if (innerNetObject.TryCast<T>() is T casted)
        {
            HandleRpc(casted, reader);
        }
    }

    AntiCheatFlags CheckRpc(T innerNetObject, MessageReader reader, ref TranslationStrings.TranslationString translationReason);
    void HandleRpc(T innerNetObject, MessageReader reader);
}