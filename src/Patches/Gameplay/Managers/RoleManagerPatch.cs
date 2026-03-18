using AmongUs.GameOptions;
using BetterAmongUs.Helpers;
using BetterAmongUs.Modules;
using BetterAmongUs.Modules.Support;
using BetterAmongUs.Mono;
using BetterAmongUs.Patches.Gameplay.UI.Settings;
using HarmonyLib;
using Hazel;

namespace BetterAmongUs.Patches.Gameplay.Managers;

[HarmonyPatch]
internal static class RoleManagerPatch
{
    internal static Dictionary<string, int> ImpostorMultiplier = []; // HashPuid, Multiplier
    private static readonly Random random = new();

    // Check if client is verified Better Among Us user
    private static Func<InnerNet.ClientData, bool> SendTo(PlayerControl target)
    {
        return (clientData) =>
        {
            if (clientData.Id == target.GetClientId())
                return false;

            var betterData = clientData.BetterData();
            if (betterData == null)
                return true;

            return !betterData.IsVerifiedBetterUser;
        };
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SetRole))]
    [HarmonyPrefix]
    private static void RoleManager_SetRole_Prefix(RoleManager __instance, PlayerControl targetPlayer, RoleTypes roleType)
    {
        // Store the original role when player dies (for ghost role purposes)
        if (roleType.IsGhostRole())
        {
            if (!targetPlayer.Data.RoleType.IsGhostRole())
            {
                targetPlayer.BetterData().RoleInfo.DeadDisplayRole = targetPlayer.Data.RoleType;
            }
        }
    }

    // Better role assignment algorithm (replaces vanilla role assignment)
    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    [HarmonyPrefix]
    private static bool RoleManager_SelectRoles_Prefix()
    {
        // Skip BAU role assignment if other mods disabled it
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_BetterRoleAlgorithm))
        {
            return true;
        }

        // Use different algorithms for different game modes
        if (!GameState.IsHideNSeek)
        {
            RegularBetterRoleAssignment();
        }
        else
        {
            HideAndSeekBetterRoleAssignment();
        }

        // Return false to prevent vanilla role assignment from running
        return false;
    }

    internal static void RegularBetterRoleAssignment()
    {
        Logger_.LogHeader($"Better Role Assignment Has Started", "RoleManager");

        // Initialize impostor multiplier tracking for all players
        foreach (var addplayer in BAUPlugin.AllPlayerControls.Where(pc => !ImpostorMultiplier.ContainsKey(Utils.GetHashPuid(pc))))
            ImpostorMultiplier[Utils.GetHashPuid(addplayer)] = 0;

        int NumImpostors = GameOptionsManager.Instance.CurrentGameOptions.NumImpostors;
        int NumPlayers = BAUPlugin.AllPlayerControls.Count;

        // Apply player count limits to impostor numbers
        var impostorLimits = new Dictionary<int, int>
        {
            { 3, 1 },
            { 5, 2 },
            { 7, 3 }
        };

        foreach (var limit in impostorLimits)
        {
            if (NumPlayers <= limit.Key)
            {
                NumImpostors = Math.Min(NumImpostors, limit.Value);
                break;
            }
        }

        List<PlayerControl> Impostors = [];
        List<PlayerControl> Crewmates = [];

        // Track available roles and their counts
        Dictionary<RoleTypes, int> ImpostorRoles = new() // Role, Amount
        {
            { RoleTypes.Shapeshifter, 0 },
            { RoleTypes.Phantom, 0 },
            { RoleTypes.Viper, 0 }
        };

        Dictionary<RoleTypes, int> CrewmateRoles = new() // Role, Amount
        {
            { RoleTypes.Engineer, 0 },
            { RoleTypes.Scientist, 0 },
            { RoleTypes.Tracker, 0 },
            { RoleTypes.Noisemaker, 0 },
            { RoleTypes.Detective, 0 }
        };

        List<RoleTypes> Roles = [.. ImpostorRoles.Keys, .. CrewmateRoles.Keys];

        // Get role counts from game options
        foreach (RoleTypes role in Roles)
        {
            if (role.IsImpostorRole())
                ImpostorRoles[role] = GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(role);
            else
                CrewmateRoles[role] = GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(role);
        }

        // Shuffle players for random role assignment
        List<PlayerControl> players = [.. BAUPlugin.AllPlayerControls.Where(player => !Impostors.Contains(player) && !Crewmates.Contains(player) && player.roleAssigned == false)];

        int n = players.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            PlayerControl value = players[k];
            players[k] = players[n];
            players[n] = value;
        }

        // Assign roles to each player
        foreach (PlayerControl pc in players)
        {
            if (pc == null || pc.roleAssigned == true) continue;

            // Check if player should be impostor based on multiplier and available slots
            if (Impostors.Count < NumImpostors && RNG() > ImpostorMultiplier[Utils.GetHashPuid(pc)])
            {
                var impRoles = ImpostorRoles.Shuffle();
                foreach (var kvp in impRoles)
                {
                    // Assign special impostor role based on chance and availability
                    if (RNG() <= GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(kvp.Key) && kvp.Value > 0)
                    {
                        ImpostorMultiplier[Utils.GetHashPuid(pc)] += 15; // Increase chance of being crewmate next game
                        ImpostorRoles[kvp.Key]--;
                        Impostors.Add(pc);
                        pc.RpcSetRole(kvp.Key);

                        // Desync role to hide special role from non-BAU players
                        if (BetterGameSettings.DesyncRoles.GetBool())
                        {
                            if (kvp.Key is not (RoleTypes.Phantom or RoleTypes.Viper))
                            {
                                AmongUsClient.Instance.SendRpcImmediatelyDesync(pc.NetId, RpcCalls.SetRole, SendOption.None, SendTo(pc), writer =>
                                {
                                    writer.Write((ushort)RoleTypes.Impostor);
                                    writer.Write(false);
                                });
                            }
                        }

                        Logger_.LogPrivate($"Assigned {kvp.Key.GetRoleName()} role to {pc.Data.PlayerName}", "RoleManager");
                        break;
                    }
                }

                // If no special role assigned, give regular impostor
                if (!Impostors.Contains(pc))
                {
                    ImpostorMultiplier[Utils.GetHashPuid(pc)] += 15;
                    Impostors.Add(pc);
                    pc.RpcSetRole(RoleTypes.Impostor);
                    Logger_.LogPrivate($"Assigned {RoleTypes.Impostor.GetRoleName()} role to {pc.Data.PlayerName}", "RoleManager");
                }
            }
            else // Assign crewmate role
            {
                var crewRoles = CrewmateRoles.Shuffle();
                foreach (var kvp in crewRoles)
                {
                    // Assign special crewmate role based on chance and availability
                    if (RNG() <= GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(kvp.Key) && kvp.Value > 0)
                    {
                        ImpostorMultiplier[Utils.GetHashPuid(pc)] = 0; // Reset impostor chance
                        CrewmateRoles[kvp.Key]--;
                        Crewmates.Add(pc);
                        pc.RpcSetRole(kvp.Key);

                        // Desync role to hide special role from non-BAU players
                        if (BetterGameSettings.DesyncRoles.GetBool())
                        {
                            if (kvp.Key is not RoleTypes.Noisemaker)
                            {
                                AmongUsClient.Instance.SendRpcImmediatelyDesync(pc.NetId, RpcCalls.SetRole, SendOption.None, SendTo(pc), writer =>
                                {
                                    writer.Write((ushort)RoleTypes.Crewmate);
                                    writer.Write(false);
                                });
                            }
                        }

                        Logger_.LogPrivate($"Assigned {kvp.Key.GetRoleName()} role to {pc.Data.PlayerName}", "RoleManager");
                        break;
                    }
                }

                // If no special role assigned, give regular crewmate
                if (!Crewmates.Contains(pc))
                {
                    ImpostorMultiplier[Utils.GetHashPuid(pc)] = 0;
                    Crewmates.Add(pc);
                    pc.RpcSetRole(RoleTypes.Crewmate);
                    Logger_.LogPrivate($"Assigned {RoleTypes.Crewmate.GetRoleName()} role to {pc.Data.PlayerName}", "RoleManager");
                }
            }
        }

        Logger_.LogHeader($"Better Role Assignment Has Finished", "RoleManager");
    }

    internal static void HideAndSeekBetterRoleAssignment()
    {
        Logger_.LogHeader($"Better Role Assignment Has Started", "RoleManager");

        // Get impostor count from BAU settings (defaults to 1)
        int NumImpostors = BetterGameSettings.HideAndSeekImpNum?.GetInt() ?? 1;

        if (NumImpostors > BAUPlugin.AllPlayerControls.Count)
            NumImpostors = BAUPlugin.AllPlayerControls.Count;

        List<NetworkedPlayerInfo> Impostors = [];
        List<NetworkedPlayerInfo> Crewmates = [];
        List<NetworkedPlayerInfo> CrewAndImps() => [.. Impostors, .. Crewmates];

        // Get predefined impostors from settings (host can set specific players as impostors)
        int[] betterImpostorSettings =
        [
            GameOptionsManager.Instance.currentHideNSeekGameOptions.ImpostorPlayerID,
            BetterGameSettingsTemp.HideAndSeekImp2?.GetInt() ?? -1,
            BetterGameSettingsTemp.HideAndSeekImp3?.GetInt() ?? -1,
            BetterGameSettingsTemp.HideAndSeekImp4?.GetInt() ?? -1,
            BetterGameSettingsTemp.HideAndSeekImp5?.GetInt() ?? -1
        ];

        for (int i = 0; i < NumImpostors; i++)
        {
            int tempSetImpostor = betterImpostorSettings[i];

            if (tempSetImpostor >= 0)
            {
                var player = Utils.PlayerFromPlayerId(tempSetImpostor);
                if (player != null)
                {
                    if (Impostors.Count < NumImpostors)
                    {
                        Impostors.Add(player.Data);
                        Logger_.LogPrivate($"Settings Assigned {RoleTypes.Impostor.GetRoleName()} role to {player.Data.PlayerName}", "RoleManager");
                    }
                }
            }
        }

        // Shuffle remaining players for random role assignment
        List<PlayerControl> players = [.. BAUPlugin.AllPlayerControls.Where(player => !CrewAndImps().Contains(player.Data))];

        int n = players.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            PlayerControl value = players[k];
            players[k] = players[n];
            players[n] = value;
        }

        // Assign roles to remaining players
        foreach (PlayerControl pc in players)
        {
            if (pc == null || pc.roleAssigned == true) continue;

            if (Impostors.Count < NumImpostors)
            {
                Impostors.Add(pc.Data);
                Logger_.LogPrivate($"Assigned {RoleTypes.Impostor.GetRoleName()} role to {pc.Data.PlayerName}", "RoleManager");
            }
            else
            {
                Crewmates.Add(pc.Data);
                Logger_.LogPrivate($"Assigned {RoleTypes.Engineer.GetRoleName()} role to {pc.Data.PlayerName}", "RoleManager");
            }
        }

        // Apply roles to all players
        foreach (var data in Impostors)
        {
            var player = Utils.PlayerFromPlayerId(data.PlayerId);
            player.RpcSetRole(RoleTypes.Impostor);
        }

        foreach (var data in Crewmates)
        {
            var player = Utils.PlayerFromPlayerId(data.PlayerId);
            player.RpcSetRole(RoleTypes.Engineer);
        }

        Logger_.LogHeader($"Better Role Assignment Has Finished", "RoleManager");
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.AssignRoleOnDeath))]
    [HarmonyPrefix]
    internal static bool RoleManager_AssignRoleOnDeath_Prefix(PlayerControl player)
    {
        // Track available ghost roles
        Dictionary<RoleTypes, int> GhostRoles = new() // Role, Amount
        {
            { RoleTypes.GuardianAngel, 0 },
        };

        List<RoleTypes> Roles = [.. GhostRoles.Keys];

        foreach (RoleTypes role in Roles)
        {
            GhostRoles[role] = GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(role);
        }

        // Deduct roles already assigned to dead players
        foreach (var allDeadPlayers in BAUPlugin.AllPlayerControls.Where(pc => !pc.IsAlive()))
        {
            for (int i = 0; i < Roles.Count; i++)
            {
                if (allDeadPlayers.Is(Roles[i]))
                {
                    GhostRoles[Roles[i]]--;
                }
            }
        }

        var ghostRoles = GhostRoles.Shuffle();

        // Assign ghost role based on chance and availability
        foreach (var kvp in ghostRoles)
        {
            if (player.IsImpostorTeam() && kvp.Key is RoleTypes.GuardianAngel) continue;

            if (kvp.Value > 0 && RNG() <= GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(kvp.Key))
            {
                player.RpcSetRole(kvp.Key);

                // Desync ghost role to hide it from non-BAU players
                if (BetterGameSettings.DesyncRoles.GetBool())
                {
                    AmongUsClient.Instance.SendRpcImmediatelyDesync(player.NetId, RpcCalls.SetRole, SendOption.None, SendTo(player), writer =>
                    {
                        writer.Write((ushort)player.Data.Role.DefaultGhostRole);
                        writer.Write(false);
                    });
                }

                return false;
            }
        }

        // Assign default ghost role if no special role available
        player.RpcSetRole(player.Data.Role.DefaultGhostRole);

        return false;
    }

    internal static int RNG()
    {
        // Choose random number generator based on settings
        switch (BetterGameSettings.RoleRandomizer.GetStringValue())
        {
            case 1:
                return UnityEngine.Random.Range(0, 100); // Unity RNG

            default:
                Random Random = new Random();
                return Random.Next(0, 100); // .NET RNG
        }
    }
}