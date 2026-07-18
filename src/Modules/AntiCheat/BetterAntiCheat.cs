using BetterAmongUs.Attributes;
using BetterAmongUs.Data;
using BetterAmongUs.Data.Config;
using BetterAmongUs.Enums;
using BetterAmongUs.Generated;
using BetterAmongUs.Managers;
using BetterAmongUs.Modules.AntiCheat.RPCHandlers.NetObjectHandlers;
using BetterAmongUs.Modules.Support;
using BetterAmongUs.MonoScripts.Extended;
using BetterAmongUs.Patches.Gameplay.UI.Settings;
using BetterAmongUs.Utilities;
using Hazel;

namespace BetterAmongUs.Modules.AntiCheat;

/// <summary>
/// Provides anti-cheat functionality for detecting and handling cheating behaviors.
/// </summary>
internal static class BetterAntiCheat
{
    /// <summary>
    /// Gets whether anti-cheat is enabled.
    /// </summary>
    internal static bool IsEnabled
    {
        get
        {
            if (GameState.IsModdedProtocol)
                return false;

            if (PlayerControl.LocalPlayer == null)
                return false;

            if (PlayerControl.LocalPlayer.Data == null)
                return false;

            return PlayerControl.LocalPlayer.Data.IsIncomplete == false;
        }
    }

    /// <summary>
    /// Updates anti-cheat checks for all players in the game.
    /// </summary>
    internal static void Update()
    {
        if (GameState.IsHost && GameState.IsInGame)
        {
            foreach (var player in BAUPlugin.AllPlayerControls)
            {
                if (BetterDataManager.Files.BetterDataFile.SickoData.Any(info => info.CheckPlayerData(player.Data)))
                {
                    string reason = TranslationStrings.AntiCheat_Reason_SickoMenuUser.LocalizedString;
                    string kickMessage = TranslationStrings.AntiCheat_KickMessage.Format(TranslationStrings.AntiCheat_ByAntiCheat, reason);
                    player.Kick(true, kickMessage, true);
                }
                else if (BetterDataManager.Files.BetterDataFile.AUMData.Any(info => info.CheckPlayerData(player.Data)))
                {
                    string reason = TranslationStrings.AntiCheat_Reason_AUMUser.LocalizedString;
                    string kickMessage = TranslationStrings.AntiCheat_KickMessage.Format(TranslationStrings.AntiCheat_ByAntiCheat, reason);
                    player.Kick(true, kickMessage, true);
                }
                else if (BetterDataManager.Files.BetterDataFile.KNData.Any(info => info.CheckPlayerData(player.Data)))
                {
                    string reason = TranslationStrings.AntiCheat_Reason_KNUser.LocalizedString;
                    string kickMessage = TranslationStrings.AntiCheat_KickMessage.Format(TranslationStrings.AntiCheat_ByAntiCheat, reason);
                    player.Kick(true, kickMessage, true);
                }
                else if (BetterDataManager.Files.BetterDataFile.MMCData.Any(info => info.CheckPlayerData(player.Data)))
                {
                    string reason = TranslationStrings.AntiCheat_Reason_MMCUser.LocalizedString;
                    string kickMessage = TranslationStrings.AntiCheat_KickMessage.Format(TranslationStrings.AntiCheat_ByAntiCheat, reason);
                    player.Kick(true, kickMessage, true);
                }
                else if (BetterDataManager.Files.BetterDataFile.CheatData.Any(info => info.CheckPlayerData(player.Data)))
                {
                    string reason = TranslationStrings.AntiCheat_Reason_KnownCheater.LocalizedString;
                    string kickMessage = TranslationStrings.AntiCheat_KickMessage.Format(TranslationStrings.AntiCheat_ByAntiCheat, reason);
                    player.Kick(true, kickMessage, true);
                }
            }
        }
    }

    /// <summary>
    /// Handles RPCs before anti-cheat detection checks.
    /// </summary>
    /// <param name="player">The player who sent the RPC.</param>
    /// <param name="callId">The RPC call ID.</param>
    /// <param name="oldReader">The MessageReader containing RPC data.</param>
    internal static void HandleCheatRPCBeforeCheck(PlayerControl player, byte callId, MessageReader oldReader)
    {
        if (!IsEnabled)
            return;

        MessageReader reader = MessageReader.Get(oldReader);
        RPCHandler.HandleRPC(callId, player, reader, HandlerFlag.CheatRpcCheck);
        reader.Recycle();
    }

    /// <summary>
    /// Checks and notifies for invalid RPCs.
    /// </summary>
    /// <param name="player">The player who sent the RPC.</param>
    /// <param name="callId">The RPC call ID.</param>
    /// <param name="oldReader">The MessageReader containing RPC data.</param>
    internal static void CheckRPC(PlayerControl player, byte callId, MessageReader oldReader)
    {
        if (!IsEnabled || !BAUConfigs.AntiCheat.Value)
            return;

        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_Anticheat) ||
            !BetterGameSettings.DetectInvalidRpcs.GetBool())
            return;

        if (player == null)
            return;

        if (player.Data == null)
            return;

        if (player.IsLocalPlayer() && player.IsHost())
            return;

        MessageReader reader = MessageReader.Get(oldReader);
        RPCHandler.HandleRPC(callId, player, reader, HandlerFlag.AntiCheat);
        reader.Recycle();
    }

    /// <summary>
    /// Checks, notifies, and cancels invalid RPCs.
    /// </summary>
    /// <param name="player">The player who sent the RPC.</param>
    /// <param name="callId">The RPC call ID.</param>
    /// <param name="oldReader">The MessageReader containing RPC data.</param>
    /// <returns>True if the RPC should be processed, false if it should be canceled.</returns>
    internal static bool CheckCancelRPC(PlayerControl player, byte callId, MessageReader oldReader)
    {
        try
        {
            if (player == null || player.Data == null) return true;
            if (!IsEnabled || !BAUConfigs.AntiCheat.Value || BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_Anticheat) || !BetterGameSettings.DetectInvalidRpcs.GetBool()) return true;
            if (player.IsLocalPlayer() && player.IsHost()) return true;

            MessageReader reader = MessageReader.Get(oldReader);

            if (TrustedRPCs(callId) != true && !player.IsHost())
            {
                BetterNotificationManager.NotifyCheat(player, $"Unregistered RPC received: {callId}");
                reader.Recycle();
                return false;
            }

            if (RPCHandler.HandleRPC(callId, player, reader, HandlerFlag.AntiCheatCancel) == false)
            {
                reader.Recycle();
                return false;
            }

            if (!player.IsHost())
            {
                if (callId is (byte)RpcCalls.SetTasks
                or (byte)RpcCalls.ExtendLobbyTimer
                or (byte)RpcCalls.CloseMeeting)
                {
                    if (BetterNotificationManager.NotifyCheat(player, TranslationStrings.AntiCheat_InvalidHostRPC.Format(Enum.GetName((RpcCalls)callId))))
                    {
                        Logger_.LogCheat($"{player.ExtendedData().RealName} {Enum.GetName((RpcCalls)callId)}: {!player.IsHost()}");
                    }

                    reader.Recycle();
                    return false;
                }
            }

            if (GameState.IsInGamePlay)
            {
                if (callId is (byte)RpcCalls.SetColor
                    or (byte)RpcCalls.SetHatStr
                    or (byte)RpcCalls.SetSkinStr
                    or (byte)RpcCalls.SetVisorStr
                    or (byte)RpcCalls.SetPetStr
                    or (byte)RpcCalls.SetNamePlateStr)
                {
                    if (BetterNotificationManager.NotifyCheat(player, TranslationStrings.AntiCheat_InvalidSetRPC.Format(Enum.GetName((RpcCalls)callId))))
                    {
                        Logger_.LogCheat($"{player.ExtendedData().RealName} {Enum.GetName((RpcCalls)callId)}: {GameState.IsInGamePlay}");
                    }

                    reader.Recycle();
                    return false;
                }
            }

            if (GameState.IsInGame && GameState.IsLobby)
            {
                if (callId is (byte)RpcCalls.StartMeeting
                    or (byte)RpcCalls.ReportDeadBody
                    or (byte)RpcCalls.SendChatNote
                    or (byte)RpcCalls.CloseMeeting
                    or (byte)RpcCalls.Exiled
                    or (byte)RpcCalls.CastVote
                    or (byte)RpcCalls.ClearVote
                    or (byte)RpcCalls.SetRole
                    or (byte)RpcCalls.ClimbLadder
                    or (byte)RpcCalls.UsePlatform
                    or (byte)RpcCalls.UseZipline
                    or (byte)RpcCalls.CompleteTask
                    or (byte)RpcCalls.BootFromVent
                    or (byte)RpcCalls.EnterVent
                    or (byte)RpcCalls.ExitVent
                    or (byte)RpcCalls.CloseDoorsOfType
                    or (byte)RpcCalls.CheckMurder
                    or (byte)RpcCalls.MurderPlayer
                    or (byte)RpcCalls.CheckShapeshift
                    or (byte)RpcCalls.Shapeshift
                    or (byte)RpcCalls.RejectShapeshift
                    or (byte)RpcCalls.CheckProtect
                    or (byte)RpcCalls.ProtectPlayer
                    or (byte)RpcCalls.CheckAppear
                    or (byte)RpcCalls.StartAppear
                    or (byte)RpcCalls.CheckVanish
                    or (byte)RpcCalls.StartVanish)
                {
                    if (BetterNotificationManager.NotifyCheat(player, TranslationStrings.AntiCheat_InvalidLobbyRPC.Format(Enum.GetName((RpcCalls)callId))))
                    {
                        Logger_.LogCheat($"{player.ExtendedData().RealName} {Enum.GetName((RpcCalls)callId)}: {GameState.IsInGame} && {GameState.IsLobby}");
                    }

                    reader.Recycle();
                    return false;
                }
            }

            reader.Recycle();

            return true;
        }
        catch (Exception ex)
        {
            Logger_.Error(ex);
            return true;
        }
    }

    /// <summary>
    /// Handles RPCs received from players.
    /// </summary>
    /// <param name="player">The player who sent the RPC.</param>
    /// <param name="callId">The RPC call ID.</param>
    /// <param name="oldReader">The MessageReader containing RPC data.</param>
    internal static void HandleRPC(PlayerControl player, byte callId, MessageReader oldReader)
    {
        if (player == null || player.Data == null || player.IsLocalPlayer())
            return;

        MessageReader reader = MessageReader.Get(oldReader);
        RPCHandler.HandleRPC(callId, player, reader, HandlerFlag.Handle);
        reader.Recycle();
    }

    /// <summary>
    /// Checks game states when sabotaging systems.
    /// </summary>
    /// <param name="player">The player attempting the sabotage.</param>
    /// <param name="systemType">The system type being updated.</param>
    /// <param name="oldReader">The MessageReader containing system update data.</param>
    /// <returns>True if the sabotage should be allowed, false otherwise.</returns>
    internal static bool RpcUpdateSystemCheck(PlayerControl player, SystemTypes systemType, MessageReader oldReader)
    {
        MessageReader reader = MessageReader.Get(oldReader);
        RegisterRPCHandlerAttribute.GetInstance<UpdateSystemHandler>().CatchedSystemType = systemType;
        if (!RPCHandler.HandleRPC((byte)RpcCalls.UpdateSystem, player, reader, HandlerFlag.AntiCheatCancel))
        {
            reader.Recycle();
            return false;
        }
        reader.Recycle();

        MessageReader reader2 = MessageReader.Get(oldReader);
        if (!RPCHandler.HandleRPC((byte)RpcCalls.UpdateSystem, player, reader2, HandlerFlag.BetterHost))
        {
            reader2.Recycle();
            return false;
        }
        reader2.Recycle();

        return true;
    }

    /// <summary>
    /// Checks if an RPC ID is from a known/trusted RPC enumeration.
    /// </summary>
    /// <param name="RPCId">The RPC ID to check.</param>
    /// <returns>True if the RPC ID is from a known enumeration, false otherwise.</returns>
    private static bool TrustedRPCs(int RPCId)
    {
        foreach (RpcCalls rpc in Enum.GetValues(typeof(RpcCalls)))
            if ((byte)rpc == RPCId || unchecked((byte)rpc) == RPCId || unchecked((byte)(short)rpc) == RPCId)
                return true;
        foreach (CustomRPC rpc in Enum.GetValues(typeof(CustomRPC)))
            if ((byte)rpc == RPCId || unchecked((byte)rpc) == RPCId || unchecked((byte)(short)rpc) == RPCId)
                return true;

        return false;
    }
}