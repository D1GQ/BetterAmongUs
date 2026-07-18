using BetterAmongUs.Data;
using BetterAmongUs.Utilities;
using BetterAmongUs.Managers;
using BetterAmongUs.Modules;
using BetterAmongUs.Patches.Gameplay.UI.Settings;
using HarmonyLib;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Patches.Client;

[HarmonyPatch]
internal static class InnerNetClientPatch
{
    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.SendOrDisconnect))]
    [HarmonyPrefix]
    private static bool InnerNetClient_SendOrDisconnect_Prefix(InnerNetClient __instance, MessageWriter msg)
    {
        // Route all outgoing messages through custom NetworkManager
        // This allows BAU to intercept/modify network traffic
        NetworkManager.SendToServer(msg);
        return false;
    }

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HandleGameData))]
    [HarmonyPrefix]
    private static bool InnerNetClient_HandleGameDataInner_Prefix([HarmonyArgument(0)] MessageReader oldReader)
    {
        // Route all incoming game data through custom NetworkManager
        // This allows BAU to process/modify incoming network messages
        NetworkManager.HandleGameData(oldReader);
        return false;
    }

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.CanBan))]
    [HarmonyPrefix]
    private static bool InnerNetClient_CanBan_Prefix(ref bool __result)
    {
        // Only allow hosts to ban players in BAU
        __result = GameState.IsHost;
        return false;
    }

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.CanKick))]
    [HarmonyPrefix]
    private static bool InnerNetClient_CanKick_Prefix(ref bool __result)
    {
        // Allow kicking under specific conditions:
        // 1. Host can always kick
        // 2. Non-hosts can only kick during meetings or when player is being voted out
        __result = GameState.IsHost || (GameState.IsInGamePlay && (GameState.IsMeeting || GameState.IsExilling));

        return false;
    }

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.KickPlayer))]
    [HarmonyPrefix]
    private static void InnerNetClient_KickPlayer_Prefix(ref int clientId, ref bool ban)
    {
        // When banning a player, add them to BAU's custom ban list if enabled
        if (ban && BetterGameSettings.UseBanPlayerList.GetBool())
        {
            // Get player info from client ID
            NetworkedPlayerInfo info = Utils.PlayerFromClientId(clientId).Data;

            // Add player to ban list using both friend code and PUID
            BetterDataManager.AddToBanList(info.FriendCode, info.Puid);
        }
    }
}