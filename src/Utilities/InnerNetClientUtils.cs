using AmongUs.InnerNet.GameDataMessages;
using BetterAmongUs.Utilities.Extension;
using Hazel;
using InnerNet;

namespace BetterAmongUs.Utilities;

/// <summary>
/// Provides helper methods for working with InnerNet messaging, RPC handling, and message serialization.
/// </summary>
internal static class InnerNetClientUtils
{
    /// <summary>
    /// Broadcasts an RPC message to all clients with optional reliability.
    /// </summary>
    /// <param name="rpcMessage">The RPC message to broadcast.</param>
    /// <param name="reliable">Whether to use reliable or unreliable transmission.</param>
    internal static void BroadcastRpc(this BaseRpcMessage rpcMessage, bool reliable = true)
    {
        if (rpcMessage.TryCast<IGameDataMessage>(out var data))
        {
            if (reliable)
                AmongUsClient.Instance.reliableMessageQueue.Enqueue(data);
            else
                AmongUsClient.Instance.unreliableMessageQueue.Enqueue(data);
        }
    }

    /// <summary>
    /// Broadcasts a game data message to all clients using reliable transmission.
    /// </summary>
    /// <param name="rpcMessage">The game data message to broadcast.</param>
    internal static void BroadcastData(this BaseGameDataMessage rpcMessage)
    {
        if (rpcMessage.TryCast<IGameDataMessage>(out var data))
        {
            AmongUsClient.Instance.reliableMessageQueue.Enqueue(data);
        }
    }

    /// <summary>
    /// Writes a player's ID to a MessageWriter, using 255 for null players.
    /// </summary>
    /// <param name="writer">The MessageWriter to write to.</param>
    /// <param name="player">The player whose ID to write.</param>
    internal static void WritePlayerId(this MessageWriter writer, PlayerControl player) => writer.Write(player?.PlayerId ?? 255);

    /// <summary>
    /// Reads a player ID from a MessageReader and returns the corresponding PlayerControl.
    /// </summary>
    /// <param name="reader">The MessageReader to read from.</param>
    /// <returns>The PlayerControl or null if not found.</returns>
    internal static PlayerControl? ReadPlayerId(this MessageReader reader) => Utils.PlayerFromPlayerId(reader.ReadByte());

    /// <summary>
    /// Writes a NetworkedPlayerInfo's player ID to a MessageWriter, using 255 for null data.
    /// </summary>
    /// <param name="writer">The MessageWriter to write to.</param>
    /// <param name="data">The NetworkedPlayerInfo whose ID to write.</param>
    internal static void WritePlayerDataId(this MessageWriter writer, NetworkedPlayerInfo data) => writer.Write(data?.PlayerId ?? 255);

    /// <summary>
    /// Reads a player ID from a MessageReader and returns the corresponding NetworkedPlayerInfo.
    /// </summary>
    /// <param name="reader">The MessageReader to read from.</param>
    /// <returns>The NetworkedPlayerInfo or null if not found.</returns>
    internal static NetworkedPlayerInfo? ReadPlayerDataId(this MessageReader reader) => Utils.PlayerDataFromPlayerId(reader.ReadByte());

    /// <summary>
    /// Writes a DeadBody's parent ID to a MessageWriter, using 255 for null bodies.
    /// </summary>
    /// <param name="writer">The MessageWriter to write to.</param>
    /// <param name="body">The DeadBody whose parent ID to write.</param>
    internal static void WriteDeadBodyId(this MessageWriter writer, DeadBody body) => writer.Write(body?.ParentId ?? 255);

    /// <summary>
    /// Reads a DeadBody ID from a MessageReader and returns the corresponding DeadBody.
    /// </summary>
    /// <param name="reader">The MessageReader to read from.</param>
    /// <returns>The DeadBody or null if not found.</returns>
    internal static DeadBody? ReadDeadBodyId(this MessageReader reader) => BAUPlugin.AllDeadBodys.FirstOrDefault(deadbody => deadbody.ParentId == reader.ReadByte());

    /// <summary>
    /// Writes a Vent's ID to a MessageWriter, using -1 for null vents.
    /// </summary>
    /// <param name="writer">The MessageWriter to write to.</param>
    /// <param name="vent">The Vent whose ID to write.</param>
    internal static void WriteVentId(this MessageWriter writer, Vent vent) => writer.Write(vent?.Id ?? -1);

    /// <summary>
    /// Reads a Vent ID from a MessageReader and returns the corresponding Vent.
    /// </summary>
    /// <param name="reader">The MessageReader to read from.</param>
    /// <returns>The Vent or null if not found.</returns>
    internal static Vent? ReadVentId(this MessageReader reader) => BAUPlugin.AllVents.FirstOrDefault(vent => vent.Id == reader.ReadInt32());

    /// <summary>
    /// Writes an array of bytes to a MessageWriter in a packed format, combining two bytes into one to save space.
    /// </summary>
    /// <param name="writer">The MessageWriter to write to.</param>
    /// <param name="bytesEnumerable">The byte values to write.</param>
    internal static void WriteBytes(this MessageWriter writer, IEnumerable<byte> bytesEnumerable)
    {
        byte[] bytes = bytesEnumerable.ToArray();

        writer.Write(bytes.Length);
        writer.Write(bytes);
    }

    /// <summary>
    /// Reads an array of bytes from a MessageReader that were previously packed using WritePackedBytes.
    /// </summary>
    /// <param name="reader">The MessageReader to read from.</param>
    /// <returns>An array of bytes.</returns>
    internal static byte[] ReadBytes(this MessageReader reader)
    {
        int count = reader.ReadInt32();
        var bytes = reader.ReadBytes(count);
        return [.. bytes];
    }

    /// <summary>
    /// Converts a MessageWriter to a MessageReader.
    /// </summary>
    /// <param name="writer">The MessageWriter to convert.</param>
    /// <returns>A MessageReader containing the writer's data.</returns>
    internal static MessageReader ToReader(this MessageWriter writer) => MessageReader.Get(writer.ToByteArray(false));

    /// <summary>
    /// Converts a MessageWriter into multiple MessageReaders for each contained message.
    /// </summary>
    /// <param name="writer">The MessageWriter to convert.</param>
    /// <returns>An array of MessageReaders.</returns>
    internal static MessageReader[] MessagesToReaders(this MessageWriter writer)
    {
        var reader = writer.ToReader();
        List<MessageReader> readers = [];

        while (reader.Position < reader.Length)
        {
            readers.Add(reader.ReadMessage());
        }

        return [.. readers];
    }

    /// <summary>
    /// Converts a MessageReader into multiple MessageReaders for each contained message.
    /// </summary>
    /// <param name="reader">The MessageReader to convert.</param>
    /// <returns>An array of MessageReaders.</returns>
    internal static MessageReader[] MessagesToReaders(this MessageReader reader)
    {
        List<MessageReader> readers = [];

        while (reader.Position < reader.Length)
        {
            readers.Add(reader.ReadMessage());
        }

        return [.. readers];
    }

    /// <summary>
    /// Converts a MessageReader into multiple MessageReaders with new buffers for each message.
    /// </summary>
    /// <param name="reader">The MessageReader to convert.</param>
    /// <returns>An array of MessageReaders with new buffers.</returns>
    internal static MessageReader[] MessagesToReadersNewBuffer(this MessageReader reader)
    {
        List<MessageReader> readers = [];

        while (reader.Position < reader.Length)
        {
            readers.Add(reader.ReadMessageAsNewBuffer());
        }

        return [.. readers];
    }

    /// <summary>
    /// Sends an RPC message immediately to all clients that match the specified target criteria, bypassing standard
    /// synchronization mechanisms.
    /// </summary>
    /// <param name="client">The InnerNetClient instance used to send the RPC messages.</param>
    /// <param name="playerNetId">The network ID of the player on whose behalf the RPC is sent.</param>
    /// <param name="callId">The identifier for the RPC call to be executed.</param>
    /// <param name="option">The send option that determines how the message is delivered (e.g., reliability, ordering).</param>
    /// <param name="targetClients">A function that selects which clients should receive the RPC message. The RPC is sent only to clients for which
    /// this function returns <see langword="true"/>.</param>
    /// <param name="writeTo">An action that writes the RPC payload to the provided MessageWriter.</param>
    internal static void SendRpcImmediatelyDesync(this InnerNetClient client, uint playerNetId, RpcCalls callId, SendOption option, Func<ClientData, bool> targetClients, Action<MessageWriter> writeTo)
    {
        foreach (var allClients in AmongUsClient.Instance.allClients)
        {
            if (!targetClients(allClients)) continue;

            client.SendRpcImmediately(playerNetId, callId, option, writeTo, allClients.Id);
        }
    }

    /// <summary>
    /// Sends a remote procedure call (RPC) message immediately to the specified player, using the provided message
    /// writer action and send option.
    /// </summary>
    /// <param name="client">The InnerNetClient instance used to send the RPC message.</param>
    /// <param name="playerNetId">The network identifier of the target player to whom the RPC is sent.</param>
    /// <param name="callId">The identifier of the RPC call to invoke.</param>
    /// <param name="option">The send option that determines how the message is delivered (such as reliability or channel).</param>
    /// <param name="writeTo">An action that writes the RPC message content to the provided MessageWriter.</param>
    /// <param name="targetClientId">The client identifier of the specific target client. Use -1 to broadcast to all clients.</param>
    internal static void SendRpcImmediately(this InnerNetClient client, uint playerNetId, RpcCalls callId, SendOption option, Action<MessageWriter> writeTo, int targetClientId = -1)
    {
        var writer = client.StartRpcImmediately(playerNetId, (byte)callId, option, targetClientId);
        writeTo(writer);
        client.FinishRpcImmediately(writer);
    }
}