using BetterAmongUs.Generated;
using BetterAmongUs.Managers;
using BetterAmongUs.Modules;
using BetterAmongUs.Modules.Support;
using BetterAmongUs.Structs;
using BetterAmongUs.Utilities;
using HarmonyLib;

namespace BetterAmongUs.Patches.Gameplay.Anticheat;

[HarmonyPatch]
internal static class VoteBanSystemPatch
{
    private static readonly List<(int ClientId, TempClientData Voter)> _voteData = [];

    [HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.Awake))]
    [HarmonyPrefix]
    private static void VoteBanSystem_Awake_Prefix()
    {
        _voteData.Clear();
    }


    [HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.AddVote))]
    [HarmonyPrefix]
    private static bool VoteBanSystem_AddVote_Prefix(int srcClient, int clientId, ref bool __state)
    {
        __state = false;

        // Skip BAU anti-cheat if disabled by other mods
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_Anticheat)) return true;

        // If not host, allow vote and log it
        if (!GameState.IsHost)
        {
            __state = true;
            return true;
        }

        var client = Utils.ClientFromClientId(srcClient);
        if (client == null) return false;

        var tempClient = new TempClientData(client);

        // Allow host to vote without restrictions
        if (tempClient.Id == AmongUsClient.Instance.GetHost().Id)
        {
            return true;
        }

        void TryFlagPlayer()
        {
            var player = client.Character;
            if (player != null)
            {
                BetterNotificationManager.NotifyCheat(player, TranslationStrings.AntiCheat_Reason_VoteKick.LocalizedString, TranslationStrings.AntiCheat_HasBeenDetectedWithCheat.LocalizedString);
            }
        }

        // Prevent voting in lobby (anti-cheat measure)
        if (GameState.IsLobby)
        {
            TryFlagPlayer();
            return false;
        }

        // If client has no ID, allow vote but log it
        if (string.IsNullOrEmpty(tempClient.Puid) && string.IsNullOrEmpty(tempClient.FriendCode))
        {
            __state = true;
            return true;
        }

        // Check if this client has already voted for the same target
        foreach (var (targetClientId, existingVoter) in _voteData)
        {
            if (targetClientId != clientId)
                continue;

            // Detect duplicate votes by comparing hash or friend code
            bool isDuplicateVote = existingVoter.HashPuid == tempClient.HashPuid ||
                                  !string.IsNullOrEmpty(tempClient.FriendCode) &&
                                   existingVoter.FriendCode == tempClient.FriendCode;

            if (isDuplicateVote)
            {
                // Block duplicate vote attempt
                return false;
            }
        }

        // Record the vote
        _voteData.Add((clientId, tempClient));
        __state = true;
        return true;
    }

    [HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.AddVote))]
    [HarmonyPostfix]
    private static void VoteBanSystem_AddVote_Postfix(VoteBanSystem __instance, int srcClient, int clientId, bool __state)
    {
        // Skip logging if anti-cheat disabled
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_Anticheat))
            return;

        // Log the vote if it was allowed
        if (__state)
        {
            LogVote(__instance, srcClient, clientId);
        }
    }

    private static void LogVote(VoteBanSystem voteBanSystem, int srcClient, int clientId)
    {
        var src = Utils.ClientFromClientId(srcClient);
        var client = Utils.ClientFromClientId(clientId);

        int currentVotes = 0;
        int maxVotes = 0;

        if (voteBanSystem.Votes.TryGetValue(clientId, out var votes))
        {
            currentVotes = votes.Count(v => v != 0); // Count non-zero votes
            maxVotes = votes.Length; // Total possible votes
        }

        Logger_.InGame(
            $"{src.Character?.GetPlayerNameAndColor() ?? src.PlayerName} " +
            $"voted to kick {client.Character?.GetPlayerNameAndColor() ?? client.PlayerName} " +
            $"<#6F6F6F>(</color><#FFFFFF>{currentVotes}</color><#6F6F6F>/</color><#FFFFFF>{maxVotes}</color><#6F6F6F>)</color>"
        );
    }
}