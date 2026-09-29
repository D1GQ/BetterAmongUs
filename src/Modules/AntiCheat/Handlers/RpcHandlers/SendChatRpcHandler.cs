using AmongUs.Data;
using BetterAmongUs.Attributes;
using BetterAmongUs.Data;
using BetterAmongUs.Enums;
using BetterAmongUs.Generated;
using BetterAmongUs.Patches.Gameplay.UI.Settings;
using BetterAmongUs.Utilities;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat.Handlers.RpcHandlers;

[RegisterRpcHandler(RpcCalls.SendChat)]
internal sealed class SendChatRpcHandler : IRpcHandler<PlayerControl>
{
    public AntiCheatFlags CheckRpc(PlayerControl innerNetObject, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
        if (innerNetObject.IsAlive() && GameState.IsInGamePlay && !GameState.IsMeeting && !GameState.IsExilling
            || DataManager.Settings.Multiplayer.ChatMode == InnerNet.QuickChatModes.QuickChatOnly)
        {
            /*
            if (BetterNotificationManager.NotifyCheat(sender, GetFormatActionText()))
            {
                string issue = GetChatIssue(sender);
                LogRpcInfo($"Invalid chat attempt: {issue}");
            }
            */

            return AntiCheatFlags.High;
        }

        return AntiCheatFlags.None;
    }

    public void HandleRpc(PlayerControl innerNetObject, MessageReader reader)
    {
        var text = reader.ReadString();

        if (BetterGameSettings.UseBanChatList.GetBool() && (!BetterGameSettings.UseBanChatListOnlyLobby.GetBool() || GameState.IsLobby))
        {
            if (TextFileHandler.CompareStringRegexMatches(BetterDataManager.Files.banChatListFilePath, text))
            {
                var ban = BetterGameSettings.UseBanChatListBan.GetBool();
                innerNetObject.Kick(ban, $"has been {(ban ? "banned" : "kicked")} due to\nchat message matching a banned pattern!");
            }
        }
    }
}
