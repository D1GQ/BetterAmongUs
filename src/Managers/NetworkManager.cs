using AmongUs.InnerNet.GameDataMessages;
using BepInEx.Unity.IL2CPP.Utils;
using BetterAmongUs.Enums;
using BetterAmongUs.Helpers;
using BetterAmongUs.Modules;
using BetterAmongUs.Modules.AntiCheat;
using BetterAmongUs.Mono;
using BetterAmongUs.Network;
using BetterAmongUs.Structs;
using HarmonyLib;
using Hazel;
using InnerNet;
using System.Collections;
using UnityEngine;

namespace BetterAmongUs.Managers;

/// <summary>
/// Manages network communication, RPC handling, and anti-cheat measures for BetterAmongUs.
/// </summary>
internal static class NetworkManager
{
    /// <summary>
    /// Gets the InnerNetClient instance from AmongUsClient.
    /// </summary>
    internal static InnerNetClient? InnerNetClient => AmongUsClient.Instance;

    /// <summary>
    /// Sends a message to the game server.
    /// </summary>
    /// <param name="writer">The MessageWriter containing the data to send.</param>
    internal static void SendToServer(MessageWriter writer)
    {
        if (InnerNetClient == null)
            return;

        try
        {
            StreamlineMessage(writer, writer.SendOption);
        }
        catch (Exception ex)
        {
            Logger_.Error(ex, "NetworkManager");
        }
        finally
        {
            if (InnerNetClient.connection != null)
            {
                SendErrors sendErrors = InnerNetClient.connection.Send(writer);
                if (sendErrors != SendErrors.None && !GameState.IsFreePlay)
                {
                    InnerNetClient.EnqueueDisconnect(DisconnectReasons.Error, "Failed to send message: " + sendErrors.ToString());
                }
            }
            else
            {
                InnerNetClient?.EnqueueDisconnect(DisconnectReasons.Custom, "InnerNetClient.connection is null");
            }
        }
    }

    /// <summary>
    /// Processes and forwards messages from a MessageWriter.
    /// </summary>
    /// <param name="writer">The MessageWriter containing messages to process.</param>
    /// <param name="sendOption">The send option for the messages.</param>
    internal static void StreamlineMessage(MessageWriter writer, SendOption sendOption)
    {
        if (!InnerNetClient.InOnlineScene)
        {
            return;
        }

        MessageReader[] allReaders = writer.ToReaders();

        foreach (MessageReader reader in allReaders)
        {
            if (reader.Tag == 5 || reader.Tag == 6)
            {
                ReadData(reader, sendOption);
                reader.Recycle();
                continue;
            }
        }
    }

    /// <summary>
    /// Reads data from a MessageReader and processes it based on the send option.
    /// </summary>
    /// <param name="reader">The MessageReader containing data to read.</param>
    /// <param name="sendOption">The send option for the data.</param>
    internal static void ReadData(MessageReader reader, SendOption sendOption)
    {
        var typeFlag = reader.Tag;
        int GameId = reader.ReadInt32();
        int ClientId = -1;

        if (typeFlag == 6)
        {
            ClientId = reader.ReadPackedInt32();
        }

        MessageReader[] allDataReaders = reader.ToReadersNewBuffer();

        foreach (MessageReader dataReader in allDataReaders)
        {
            if (dataReader.Tag == 2)
            {
                var data = ReadRpc(MessageReader.Get(dataReader), typeFlag, ClientId);
                HandleInnerNetObject(data.Sender, (byte)data.CalledRpc, data.Reader);
                continue;
            }
        }
    }

    /// <summary>
    /// Reads RPC data from a MessageReader.
    /// </summary>
    /// <param name="reader">The MessageReader containing RPC data.</param>
    /// <param name="flag">The message flag.</param>
    /// <param name="targetId">The target client ID.</param>
    /// <returns>An RPCData structure containing the parsed RPC information.</returns>
    internal static RPCData ReadRpc(MessageReader reader, byte flag, int targetId)
    {
        var netId = reader.ReadPackedUInt32();
        byte calledId = reader.ReadByte();

        return new RPCData(AmongUsClient.Instance.FindObjectByNetId<InnerNetObject>(netId), (SendOption)flag, targetId, (RpcCalls)calledId, reader);
    }

    /// <summary>
    /// Handles GameData messages from the server.
    /// </summary>
    /// <param name="parentReader">The MessageReader containing GameData messages.</param>
    public static void HandleGameData(MessageReader parentReader)
    {
        try
        {
            while (parentReader.Position < parentReader.Length)
            {
                MessageReader messageReader = parentReader.ReadMessageAsNewBuffer();
                int currentMessageNumber = InnerNetClient.msgNum++;
                var oldReader = MessageReader.Get(messageReader);
                InnerNetClient.StartCoroutine(HandleGameDataInner(messageReader, currentMessageNumber));
                RPCHandler.HandleRPC(parentReader.Tag, null, oldReader, HandlerFlag.HandleGameDataTag);
                oldReader.Recycle();
            }
        }
        finally
        {
            parentReader.Recycle();
        }
    }

    /// <summary>
    /// Internal coroutine to handle different types of GameData messages.
    /// </summary>
    /// <param name="reader">The MessageReader containing the GameData.</param>
    /// <param name="msgNum">The message number for tracking.</param>
    /// <returns>An IEnumerator for the coroutine.</returns>
    private static IEnumerator HandleGameDataInner(MessageReader reader, int msgNum)
    {
        int attemptCount = 0;
        reader.Position = 0;
        byte tag = reader.Tag;

        switch ((GameDataTypes)tag)
        {
            case GameDataTypes.DataFlag: // Object deserialization
                yield return HandleObjectDeserialization(reader, msgNum, attemptCount);
                break;

            case GameDataTypes.RpcFlag: // RPC handling
                yield return HandleRpcCall(reader, msgNum, attemptCount);
                break;

            case GameDataTypes.SpawnFlag: // Spawn handling
                InnerNetClient.StartCoroutine(InnerNetClient.CoHandleSpawn(reader));
                break;

            case GameDataTypes.DespawnFlag: // Object destruction
                yield return HandleObjectDestruction(reader);
                break;

            case GameDataTypes.ReadyFlag: // Client ready status
                yield return HandleClientReady(reader);
                break;

            case GameDataTypes.SceneChangeFlag: // Scene change
                HandleSceneChange(reader);
                break;

            case GameDataTypes.XboxDeclareXuid: // Special case (ulong parsing) (DO NOT USE)
                yield return HandleXboxDeclareXuid(reader);
                break;

            default: // Invalid tags
                HandleInvalidTag(reader);
                break;
        }

        yield break;
    }

    /// <summary>
    /// Handles object deserialization from a MessageReader.
    /// </summary>
    private static IEnumerator HandleObjectDeserialization(MessageReader reader, int msgNum, int initialAttemptCount)
    {
        int attemptCount = initialAttemptCount;
        try
        {
            InnerNetObject innerNetObject;
            while (true)
            {
                uint netId = reader.ReadPackedUInt32();

                if (InnerNetClient.allObjects.AllObjectsFast.TryGetValue(netId, out innerNetObject))
                {
                    innerNetObject.Deserialize(reader, false);
                    break;
                }

                if (InnerNetClient.DestroyedObjects.Contains(netId))
                {
                    break;
                }

                Debug.LogWarning("Stored data for " + netId.ToString());
                attemptCount++;

                if (attemptCount > 10)
                {
                    yield break;
                }

                reader.Position = 0;
                yield return Effects.Wait(0.1f);
            }
        }
        finally
        {
            reader.Recycle();
        }
    }

    /// <summary>
    /// Handles RPC calls from a MessageReader.
    /// </summary>
    private static IEnumerator HandleRpcCall(MessageReader reader, int msgNum, int initialAttemptCount)
    {
        int attemptCount = initialAttemptCount;
        try
        {
            while (true)
            {
                uint netId;
                byte rpcCall;

                try
                {
                    netId = reader.ReadPackedUInt32();
                    rpcCall = reader.ReadByte();
                }
                catch
                {
                    throw;
                }

                if (InnerNetClient.allObjects.AllObjectsFast.TryGetValue(netId, out InnerNetObject innerNetObject))
                {
                    if (!HandleInnerNetObject(innerNetObject, rpcCall, reader))
                    {
                        break;
                    }

                    if (innerNetObject is PlayerControl player && player != null)
                    {
                        if (rpcCall == (byte)RpcCalls.SetNamePlateStr)
                        {
                            RPC.HandleCustomRPCPacked(player, reader);
                        }
                    }

                    if (Enum.IsDefined(typeof(RpcCalls), rpcCall))
                    {
                        innerNetObject?.HandleRpc(rpcCall, reader);
                    }
                    else
                    {
                        if (innerNetObject is PlayerControl player2 && player2 != null)
                        {
                            RPC.HandleCustomRPCLegacy(player2, rpcCall, reader);
                        }
                    }

                    break;
                }

                if (netId == 4294967295U || InnerNetClient.DestroyedObjects.Contains(netId))
                {
                    break;
                }

                Debug.LogWarning($"Stored Msg {msgNum} RPC {(RpcCalls)rpcCall} for {netId}");
                attemptCount++;

                if (attemptCount > 10)
                {
                    yield break;
                }

                reader.Position = 0;
                yield return Effects.Wait(0.1f);
            }
        }
        finally
        {
            reader.Recycle();
        }
    }

    /// <summary>
    /// Handles object destruction messages.
    /// </summary>
    private static IEnumerator HandleObjectDestruction(MessageReader reader)
    {
        try
        {
            uint netId = reader.ReadPackedUInt32();
            InnerNetClient.DestroyedObjects.Add(netId);

            InnerNetObject innerNetObject = InnerNetClient.FindObjectByNetId<InnerNetObject>(netId);
            if (innerNetObject && !innerNetObject.AmOwner)
            {
                InnerNetClient.RemoveNetObject(innerNetObject);
                innerNetObject.gameObject.DestroyObj();
            }
        }
        finally
        {
            reader.Recycle();
        }
        yield break;
    }

    /// <summary>
    /// Handles client ready status messages.
    /// </summary>
    private static IEnumerator HandleClientReady(MessageReader reader)
    {
        try
        {
            ClientData clientData = InnerNetClient.FindClientById(reader.ReadPackedInt32());
            if (clientData != null)
            {
                clientData.IsReady = true;
            }
        }
        finally
        {
            reader.Recycle();
        }
        yield break;
    }

    /// <summary>
    /// Handles scene change messages.
    /// </summary>
    private static void HandleSceneChange(MessageReader reader)
    {
        int clientId = reader.ReadPackedInt32();
        ClientData clientData = InnerNetClient.FindClientById(clientId);
        string sceneName = reader.ReadString();

        if (clientData != null && !string.IsNullOrWhiteSpace(sceneName))
        {
            InnerNetClient.StartCoroutine(InnerNetClient.CoOnPlayerChangedScene(clientData, sceneName));
        }
        else
        {
            Debug.Log($"Couldn't find client {clientId} to change scene to {sceneName}");
            reader.Recycle();
        }
    }

    /// <summary>
    /// Handles Xbox XUID declaration messages (currently disabled).
    /// </summary>
    private static IEnumerator HandleXboxDeclareXuid(MessageReader reader)
    {
        /*
        try
        {
            string data = reader.ReadString();
            if (ulong.TryParse(data, out ulong parsedValue))
            {
            }
            yield break;
        }
        finally
        {
            reader.Recycle();
        }
        */
        reader.Recycle();
        yield break;
    }

    /// <summary>
    /// Handles invalid message tags.
    /// </summary>
    private static void HandleInvalidTag(MessageReader reader)
    {
        Logger_.Warning($"Bad tag {reader.Tag} at {reader.Offset}+{reader.Position}={reader.Length}: " + string.Join(" ", reader.Buffer.Take(128)), "NetworkManager");
        reader.Recycle();
    }

    /// <summary>
    /// Handles InnerNetObject RPCs with anti-cheat checks.
    /// </summary>
    private static bool HandleInnerNetObject(InnerNetObject netObj, byte callId, MessageReader reader)
    {
        if (netObj is PlayerControl player)
        {
            if (!PlayerRpc(player, callId, reader))
            {
                return false;
            }
        }
        else if (netObj is PlayerPhysics physics)
        {
            if (!PlayerRpc(physics.myPlayer, callId, reader))
            {
                return false;
            }
        }
        else if (netObj is CustomNetworkTransform networkTransform)
        {
            if (!PlayerRpc(networkTransform.myPlayer, callId, reader))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Processes RPCs for a player with anti-cheat validation.
    /// </summary>
    private static bool PlayerRpc(PlayerControl player, byte callId, MessageReader reader)
    {
        if (player.BetterData() != null)
        {
            player.BetterData().AntiCheatInfo.RPCSentPS++;
            if (player.BetterData().AntiCheatInfo.RPCSentPS >= ExtendedAntiCheatInfo.MAX_RPC_SENT)
            {
                return false;
            }
        }

        BetterAntiCheat.HandleCheatRPCBeforeCheck(player, callId, reader);

        if (BetterAntiCheat.CheckCancelRPC(player, callId, reader) != true)
        {
            if (!player.IsLocalPlayer()) Logger_.LogCheat($"RPC canceled by Anti-Cheat: {Enum.GetName((RpcCalls)callId)}{Enum.GetName((CustomRPC)callId)} - {callId}");
            return false;
        }

        BetterAntiCheat.CheckRPC(player, callId, reader);
        BetterAntiCheat.HandleRPC(player, callId, reader);

        return true;
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.UpdateSystem), typeof(SystemTypes), typeof(PlayerControl), typeof(MessageReader))]
    internal static class MessageReaderUpdateSystemPatch
    {
        internal static bool Prefix([HarmonyArgument(0)] SystemTypes systemType, [HarmonyArgument(1)] PlayerControl player, [HarmonyArgument(2)] MessageReader reader)
        {
            player.BetterData().AntiCheatInfo.RPCSentPS++;
            if (player.BetterData().AntiCheatInfo.RPCSentPS >= ExtendedAntiCheatInfo.MAX_RPC_SENT)
            {
                return false;
            }

            if (BetterAntiCheat.RpcUpdateSystemCheck(player, systemType, reader) != true) return false;

            return true;
        }
    }
}