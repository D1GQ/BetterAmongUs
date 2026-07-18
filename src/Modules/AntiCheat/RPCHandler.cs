using BetterAmongUs.Attributes;
using BetterAmongUs.Enums;
using BetterAmongUs.Generated;
using BetterAmongUs.Modules.Support;
using BetterAmongUs.MonoScripts.Extended;
using Hazel;
using InnerNet;
using UnityEngine;

namespace BetterAmongUs.Modules.AntiCheat;

/// <summary>
/// Abstract base class for handling RPC (Remote Procedure Call) messages with anti-cheat functionality.
/// </summary>
internal abstract class RPCHandler
{
    /// <summary>
    /// Array of all registered RPC handler instances.
    /// </summary>
    internal static readonly RPCHandler?[] allHandlers = [.. RegisterRPCHandlerAttribute.Instances];

    /// <summary>
    /// Gets the InnerNetClient instance.
    /// </summary>
    internal InnerNetClient innerNetClient => AmongUsClient.Instance;

    /// <summary>
    /// Gets the RPC call ID that this handler responds to.
    /// </summary>
    internal virtual byte CallId => byte.MaxValue;

    /// <summary>
    /// Gets the GameData tag that this handler responds to.
    /// </summary>
    internal virtual byte GameDataTag => byte.MaxValue;

    /// <summary>
    /// Gets or sets whether this handler should process messages from the local player.
    /// </summary>
    internal virtual bool LocalHandling { get; set; }

    /// <summary>
    /// Gets whether to cancel RPC handling when not the host.
    /// </summary>
    protected static bool CancelAsHost => !GameState.IsHost;

    /// <summary>
    /// Checks if two positions are within a specified range.
    /// </summary>
    /// <param name="pos1">First position.</param>
    /// <param name="pos2">Second position.</param>
    /// <param name="range">Maximum allowed distance.</param>
    /// <returns>True if positions are within range, false otherwise.</returns>
    internal static bool CheckRange(Vector2 pos1, Vector2 pos2, float range) => Vector2.Distance(pos1, pos2) <= range;

    /// <summary>
    /// Handles normal RPC messages.
    /// </summary>
    /// <param name="sender">The player who sent the RPC.</param>
    /// <param name="reader">The MessageReader containing RPC data.</param>
    internal virtual void Handle(PlayerControl? sender, MessageReader reader) { }

    /// <summary>
    /// Handles GameData messages.
    /// </summary>
    /// <param name="reader">The MessageReader containing GameData.</param>
    internal virtual void HandleGameData(MessageReader reader) { }

    /// <summary>
    /// Handles RPC messages for cheat detection checks.
    /// </summary>
    /// <param name="sender">The player who sent the RPC.</param>
    /// <param name="reader">The MessageReader containing RPC data.</param>
    internal virtual void HandleCheatRpcCheck(PlayerControl? sender, MessageReader reader) { }

    /// <summary>
    /// Handles anti-cheat cancellation checks for RPC messages.
    /// </summary>
    /// <param name="sender">The player who sent the RPC.</param>
    /// <param name="reader">The MessageReader containing RPC data.</param>
    /// <returns>True if the RPC should be processed, false if it should be canceled.</returns>
    internal virtual bool HandleAntiCheatCancel(PlayerControl? sender, MessageReader reader) => true;

    /// <summary>
    /// Handles anti-cheat analysis for RPC messages.
    /// </summary>
    /// <param name="sender">The player who sent the RPC.</param>
    /// <param name="reader">The MessageReader containing RPC data.</param>
    internal virtual void HandleAntiCheat(PlayerControl? sender, MessageReader reader) { }

    /// <summary>
    /// Handles RPC messages with BetterAmongUs-specific logic.
    /// </summary>
    /// <param name="sender">The player who sent the RPC.</param>
    /// <param name="reader">The MessageReader containing RPC data.</param>
    /// <returns>True if the RPC should be processed, false if it should be canceled.</returns>
    internal virtual bool BetterHandle(PlayerControl? sender, MessageReader reader) => true;

    protected static PlayerControl? catchedSender;
    protected static HandlerFlag catchedHandlerFlag = HandlerFlag.Handle;

    /// <summary>
    /// Routes RPC messages to the appropriate handler based on the call ID and handler flag.
    /// </summary>
    /// <param name="calledId">The RPC call ID.</param>
    /// <param name="sender">The player who sent the RPC.</param>
    /// <param name="reader">The MessageReader containing RPC data.</param>
    /// <param name="handlerFlag">The type of handling to perform.</param>
    /// <returns>True if the RPC was handled successfully and should continue, false if canceled.</returns>
    internal static bool HandleRPC(byte calledId, PlayerControl? sender, MessageReader reader, HandlerFlag handlerFlag)
    {
        catchedSender = sender;
        catchedHandlerFlag = handlerFlag;
        bool cancel = false;

        foreach (var handler in allHandlers)
        {
            if (handlerFlag != HandlerFlag.HandleGameDataTag && calledId == handler.CallId)
            {
                if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_RPCHandler + handler.GetType().Name)
                    || BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_RPCHandler + handler.GetType().Name + ":" + Enum.GetName(handlerFlag)))
                {
                    break;
                }

                try
                {
                    switch (handlerFlag)
                    {
                        case HandlerFlag.Handle:
                            handler.Handle(sender, reader);
                            break;
                        case HandlerFlag.AntiCheatCancel:
                            cancel = !handler.HandleAntiCheatCancel(sender, reader);
                            break;
                        case HandlerFlag.AntiCheat:
                            handler.HandleAntiCheat(sender, reader);
                            break;
                        case HandlerFlag.CheatRpcCheck:
                            handler.HandleCheatRpcCheck(sender, reader);
                            break;
                        case HandlerFlag.BetterHost:
                            cancel = !handler.BetterHandle(sender, reader);
                            break;
                    }

                    if (!cancel) break;
                }
                catch
                {
                }
            }
            else if (handlerFlag == HandlerFlag.HandleGameDataTag && calledId == handler.GameDataTag)
            {
                if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_RPCHandler + handler.GetType().Name)
                    || BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_RPCHandler + handler.GetType().Name + ":" + Enum.GetName(handlerFlag)))
                {
                    break;
                }

                try
                {
                    handler.HandleGameData(reader);
                }
                catch (Exception ex)
                {
                    Logger_.Error(ex);
                }
            }
        }

        return !cancel;
    }

    /// <summary>
    /// Logs RPC information for debugging and anti-cheat tracking.
    /// </summary>
    /// <param name="info">Additional information about the RPC.</param>
    /// <param name="player">Optional player to log information for.</param>
    internal void LogRpcInfo(string info, PlayerControl? player = null)
    {
        string Name = Enum.GetName((RpcCalls)CallId) ?? Enum.GetName((CustomRPC)CallId) ?? $"Unregistered({CallId})";
        Name = $"[{Enum.GetName(catchedHandlerFlag)}] > " + Name;
        Logger_.LogCheat($"{catchedSender?.ExtendedData()?.RealName ?? player.ExtendedData()?.RealName ?? string.Empty} {Name}: {info}");
    }

    /// <summary>
    /// Gets formatted text for invalid action RPC notifications.
    /// </summary>
    /// <returns>A formatted string for displaying invalid action RPC messages.</returns>
    internal string GetFormatActionText()
    {
        string Name = Enum.GetName((RpcCalls)CallId) ?? Enum.GetName((CustomRPC)CallId) ?? $"Unregistered({CallId})";
        return TranslationStrings.AntiCheat_InvalidActionRPC.Format(Name);
    }

    /// <summary>
    /// Gets formatted text for invalid set RPC notifications.
    /// </summary>
    /// <returns>A formatted string for displaying invalid set RPC messages.</returns>
    internal string GetFormatSetText()
    {
        string Name = Enum.GetName((RpcCalls)CallId) ?? Enum.GetName((CustomRPC)CallId) ?? $"Unregistered({CallId})";
        return TranslationStrings.AntiCheat_InvalidSetRPC.Format(Name);
    }
}