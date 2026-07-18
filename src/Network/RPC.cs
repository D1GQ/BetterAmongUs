using BetterAmongUs.Enums;
using BetterAmongUs.MonoScripts.Extended;
using BetterAmongUs.Utilities;
using Hazel;

namespace BetterAmongUs.Network;

/// <summary>
/// Handles custom RPC (Remote Procedure Call) messages for BetterAmongUs.
/// </summary>
internal static class RPC
{
    /// <summary>
    /// The base rpc call used for custom RPC messages.
    /// </summary>
    internal const RpcCalls CUSTOM_RPC_CALL = RpcCalls.CancelPet;


    /// <summary>
    /// Sends a custom RPC message packed within a vanilla SetNamePlateStr RPC call.
    /// This method is used to maintain compatibility with vanilla Among Us servers
    /// while allowing custom RPC communication.
    /// </summary>
    /// <param name="customRPC">The custom RPC type to send.</param>
    /// <param name="action">A delegate that writes the custom RPC payload to the message writer.</param>
    /// <param name="targetClientId">The specific client ID to target, or -1 to broadcast to all clients.</param>
    internal static void SendCustomRpcPacked(CustomRPC customRPC, Action<MessageWriter> action, int targetClientId = -1)
    {
        AmongUsClient.Instance.SendRpcImmediately(PlayerControl.LocalPlayer.MyPhysics.NetId, CUSTOM_RPC_CALL, SendOption.Reliable, writer =>
        {
            writer.Write(ModInfo.Constants.BAU_CUSTOM_RPC_FLAG); // Flag to check if its a rpc packed into SetNamePlateStr
            writer.Write((byte)customRPC);
            action(writer);
        }, targetClientId);
    }

    /// <summary>
    /// Handles incoming custom RPC messages by extracting and processing them
    /// from the packed SetNamePlateStr RPC call.
    /// </summary>
    /// <param name="player">The player who sent the RPC message.</param>
    /// <param name="oldReader">The message reader containing the RPC data.</param>
    internal static bool HandleCustomRPCPacked(PlayerControl player, MessageReader oldReader)
    {
        if (player == null || player.IsLocalPlayer() || player.Data == null)
            return false;

        MessageReader reader = MessageReader.Get(oldReader);

        if (IsPackedCustomRpc(reader))
        {
            CustomRPC customRPC = (CustomRPC)reader.ReadByte();
            switch (customRPC)
            {
                case CustomRPC.SendSecretToPlayer:
                    {
                        player.ExtendedData().HandshakeHandler.HandleSecretFromSender(reader);
                    }
                    break;
                case CustomRPC.CheckSecretHashFromPlayer:
                    {
                        player.ExtendedData().HandshakeHandler.HandleSecretHashFromPlayer(reader);
                    }
                    break;
            }

            reader.Recycle();
            return true;
        }

        reader.Recycle();
        return false;
    }

    /// <summary>
    /// Processes custom RPC messages received from other players.
    /// </summary>
    /// <param name="player">The player who sent the RPC.</param>
    /// <param name="callId">The ID of the RPC call.</param>
    /// <param name="oldReader">The message reader containing the RPC data.</param>
    /// <remarks>
    /// Handles both defined custom RPCs and protects against unknown RPCs in modded lobbies.
    /// </remarks>
    internal static void HandleCustomRPCLegacy(PlayerControl player, byte callId, MessageReader oldReader)
    {
        if (player == null || player.IsLocalPlayer() || player.Data == null || Enum.IsDefined(typeof(RpcCalls), callId))
            return;

        if (Enum.IsDefined(typeof(CustomRPC), (int)unchecked(callId)))
        {
            MessageReader reader = MessageReader.Get(oldReader);

            switch (callId)
            {
                case (byte)CustomRPC.SendSecretToPlayer:
                    {
                        player.ExtendedData().HandshakeHandler.HandleSecretFromSender(reader);
                    }
                    break;
                case (byte)CustomRPC.CheckSecretHashFromPlayer:
                    {
                        player.ExtendedData().HandshakeHandler.HandleSecretHashFromPlayer(reader);
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Determines whether a MessageReader contains a packed custom RPC message.
    /// </summary>
    /// <param name="reader">The MessageReader to check for custom RPC content.</param>
    /// <returns>
    /// <c>true</c> if the reader contains a custom RPC flag and custom RPC data;
    /// otherwise, <c>false</c>.
    /// </returns>
    internal static bool IsPackedCustomRpc(MessageReader reader)
    {
        if (reader.BytesRemaining > 0)
        {
            try
            {
                if (reader.ReadString() == ModInfo.Constants.BAU_CUSTOM_RPC_FLAG)
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        return false;
    }
}