using BepInEx.Unity.IL2CPP.Utils;
using BetterAmongUs.Data;
using BetterAmongUs.Data.Config;
using BetterAmongUs.Enums;
using BetterAmongUs.Helpers;
using BetterAmongUs.Mono;
using BetterAmongUs.Network;
using Hazel;
using Il2CppInterop.Runtime.Attributes;
using System.Collections;
using UnityEngine;

namespace BetterAmongUs.Modules;

/// <summary>
/// Handles the secure handshake process between BetterAmongUs clients using Diffie-Hellman key exchange.
/// </summary>
internal sealed class HandshakeHandler
{
    [HideFromIl2Cpp]
    internal HandshakeHandler(ExtendedPlayerInfo extendedData)
    {
        _extendedData = extendedData;
    }

    private readonly ExtendedPlayerInfo _extendedData;

    /// <summary>
    /// Initiates the wait period before sending the secret to another player.
    /// </summary>
    internal void WaitSendSecretToPlayer()
    {
        _extendedData.StartCoroutine(CoWaitSendSecretToPlayer());
    }

    /// <summary>
    /// Coroutine that waits for player initialization before sending the secret.
    /// </summary>
    private IEnumerator CoWaitSendSecretToPlayer()
    {
        if (!BAUConfigs.SendBetterRpc.Value) yield break;

        while (_extendedData._Data == null || _extendedData._Data.Object == null || PlayerControl.LocalPlayer == null)
        {
            if (GameState.IsFreePlay) yield break;
            yield return null;
        }

        yield return new WaitForSeconds(1f);
        SendSecretToPlayer();
    }

    /// <summary>
    /// Resends the secret to the player if not already verified.
    /// </summary>
    internal void ResendSecretToPlayer()
    {
        if (!BAUConfigs.SendBetterRpc.Value)
            return;

        if (HasSendSharedSecret && _extendedData.IsVerifiedBetterUser)
            return;

        HasSendSharedSecret = false;
        SendSecretToPlayer();
    }

    /// <summary>
    /// Sends the local client's public key and temporary key to another player.
    /// </summary>
    // Local client sends to client
    private void SendSecretToPlayer()
    {
        if (_extendedData._Data.Object.IsLocalPlayer())
            return;

        if (HasSendSharedSecret)
            return;

        HasSendSharedSecret = true;

        RPC.SendCustomRpcPacked(CustomRPC.SendSecretToPlayer, writer =>
        {
            writer.Write(SharedSecret.CryptoAvailable);
            writer.WriteBytes(SharedSecret.GetPublicKey());
            writer.Write(SharedSecret.GetTempKey());
        }, _extendedData._Data.ClientId);
    }

    /// <summary>
    /// Handles receiving a secret from another player and generates a shared secret.
    /// </summary>
    /// <param name="reader">MessageReader containing the sender's public key and temporary key.</param>
    // Client receives from local client
    internal void HandleSecretFromSender(MessageReader reader)
    {
        if (_extendedData._Data?.Object?.IsLocalPlayer() == true)
            return;

        bool senderSupportsCrypto = reader.ReadBoolean();
        byte[] sendersPublicKey = reader.ReadBytes();
        int tempKey = reader.ReadInt32();

        // Logger.Log($"Received public key ({sendersPublicKey.Length} bytes) from {_Data.PlayerName}");

        SharedSecret.UseFallback = !senderSupportsCrypto;
        SharedSecret.SetRemoteTempKey(tempKey);

        byte[] secret = SharedSecret.GenerateSharedSecret(sendersPublicKey);
        if (secret.Length == 0)
        {
            // Logger.Error("Failed to generate shared secret!");
            return;
        }

        _extendedData.IsBetterUser = true;

        TryHandlePendingVerificationData();
        SendSecretHashToSender(tempKey, _extendedData._Data.ClientId);
        ResendSecretToPlayer();
    }

    /// <summary>
    /// Sends the hash of the generated shared secret back to the original sender for verification.
    /// </summary>
    /// <param name="tempKey">The temporary key received from the sender.</param>
    /// <param name="senderClientId">The client ID of the sender.</param>
    // Client sends back to local client
    private void SendSecretHashToSender(int tempKey, int senderClientId)
    {
        if (!BAUConfigs.SendBetterRpc.Value) return;

        int hash = SharedSecret.GetSharedSecretHash();
        // Logger.Log($"Sending secret hash: {hash} (tempKey: {tempKey})");

        RPC.SendCustomRpcPacked(CustomRPC.CheckSecretHashFromPlayer, writer =>
        {
            writer.Write(tempKey);
            writer.Write(hash);
        }, senderClientId);
    }

    /// <summary>
    /// Handles receiving a secret hash from another player for verification.
    /// </summary>
    /// <param name="reader">MessageReader containing the temporary key and hash.</param>
    internal void HandleSecretHashFromPlayer(MessageReader reader)
    {
        int tempKey = reader.ReadInt32();
        int receivedHash = reader.ReadInt32();

        _pendingVerificationData = (tempKey, receivedHash);
        TryHandlePendingVerificationData();
    }

    /// <summary>
    /// Attempts to verify pending handshake data if all required information is available.
    /// </summary>
    internal void TryHandlePendingVerificationData()
    {
        if (!_pendingVerificationData.HasValue)
            return;

        if (SharedSecret.GetSharedSecret().Length == 0)
            return;

        var (tempKey, receivedHash) = _pendingVerificationData.Value;

        // Logger.Log($"Received hash check: TempKey={data.tempKey} (ours={SharedSecret.GetTempKey()}), Hash={data.receivedHash} (ours={SharedSecret.GetSharedSecretHash()})");

        if (tempKey != SharedSecret.GetTempKey())
        {
            // Logger.Warning($"Invalid tempKey from {extendedData._Data?.PlayerName}");
            return;
        }

        _extendedData.IsBetterUser = true;

        if (receivedHash == SharedSecret.GetSharedSecretHash())
        {
            _extendedData.IsVerifiedBetterUser = true;
            // Logger.Log($"Verified player: {extendedData._Data?.PlayerName}");
        }
        else
        {
            // Logger.Warning($"Hash mismatch from {extendedData._Data?.PlayerName}");
        }

        _pendingVerificationData = null;
    }

    private (int tempKey, int receivedHash)? _pendingVerificationData = null;
    private bool HasSendSharedSecret { get; set; }

    /// <summary>
    /// Gets the SharedSecretExchange instance for secure key exchange.
    /// </summary>
    internal SharedSecretExchange SharedSecret { get; set; } = new();
}
