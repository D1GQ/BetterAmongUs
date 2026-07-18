using BetterAmongUs.Modules;
using System.Security.Cryptography;

namespace BetterAmongUs.Data;

/// <summary>
/// Handles secure key exchange using Elliptic Curve Diffie-Hellman (ECDH) for establishing shared secrets.
/// </summary>
internal sealed class SharedSecretExchange
{
    private readonly ECDiffieHellman? dh;
    private byte[] publicKey;
    private int tempKey;
    private byte[] sharedSecret = [];
    private readonly bool cryptoDisabled;

    /// <summary>
    /// Gets a value indicating whether cryptographic operations are available.
    /// </summary>
    internal bool CryptoAvailable => !cryptoDisabled;

    /// <summary>
    /// Gets or sets a value indicating whether to use the fallback key exchange mechanism.
    /// </summary>
    internal bool UseFallback { get; set; }

    private int? remoteTempKey;

    /// <summary>
    /// Initializes a new instance of the SharedSecretExchange class with a new ECDH key pair.
    /// </summary>
    internal SharedSecretExchange()
    {
        try
        {
            dh = ECDiffieHellman.Create();
            dh.GenerateKey(ECCurve.NamedCurves.nistP256);
            publicKey = dh.ExportSubjectPublicKeyInfo();
        }
        catch (Exception ex)
        {
            Logger_.Error("ECDH unavailable, using fallback handshake: " + ex.Message, logConsole: false);
            cryptoDisabled = true;
            dh = null;
            publicKey = [];
        }

        tempKey = GenerateSecureTempKey();
    }

    /// <summary>
    /// Generates a cryptographically secure random temporary key.
    /// </summary>
    /// <returns>A random positive integer used as a temporary key.</returns>
    private static int GenerateSecureTempKey()
    {
        byte[] buffer = new byte[4];
        RandomNumberGenerator.Fill(buffer);
        return Math.Abs(BitConverter.ToInt32(buffer, 0));
    }

    /// <summary>
    /// Sets the temporary key received from the remote party.
    /// </summary>
    /// <param name="key">The temporary key from the remote party.</param>
    internal void SetRemoteTempKey(int key)
    {
        remoteTempKey = key;
    }

    /// <summary>
    /// Gets the public key for key exchange.
    /// </summary>
    /// <returns>The public key in SubjectPublicKeyInfo format, or an empty array if cryptography is disabled.</returns>
    internal byte[] GetPublicKey()
    {
        if (cryptoDisabled) return [];
        return publicKey;
    }

    /// <summary>
    /// Gets the temporary key used for initial communication.
    /// </summary>
    /// <returns>A random integer used as a temporary key.</returns>
    internal int GetTempKey()
    {
        return tempKey;
    }

    /// <summary>
    /// Gets the established shared secret.
    /// </summary>
    /// <returns>The shared secret byte array.</returns>
    internal byte[] GetSharedSecret()
    {
        return sharedSecret;
    }

    /// <summary>
    /// Generates a shared secret using another party's public key.
    /// </summary>
    /// <param name="otherPartyPublicKey">The other party's public key in SubjectPublicKeyInfo format.</param>
    /// <returns>The shared secret byte array, or an empty array if generation fails.</returns>
    internal byte[] GenerateSharedSecret(byte[] otherPartyPublicKey)
    {
        if (sharedSecret.Length > 0) return sharedSecret;

        if (cryptoDisabled || UseFallback)
        {
            if (!remoteTempKey.HasValue)
                return [];

            sharedSecret = DeriveFallbackSecret(tempKey, remoteTempKey.Value);
            return sharedSecret;
        }

        try
        {
            using var otherPartyDH = ECDiffieHellman.Create();

            if (otherPartyPublicKey == null || otherPartyPublicKey.Length == 0)
                return [];

            otherPartyDH.ImportSubjectPublicKeyInfo(otherPartyPublicKey, out _);
            sharedSecret = dh.DeriveKeyMaterial(otherPartyDH.PublicKey);

            dh.Dispose();
            return sharedSecret;
        }
        catch (Exception ex)
        {
            Logger_.Error("Error generating shared secret: " + ex.Message, logConsole: false);
            return [];
        }
    }

    /// <summary>
    /// Gets a numeric hash of the shared secret for verification purposes.
    /// </summary>
    /// <returns>A 32-bit integer hash of the shared secret, or 0 if no shared secret exists.</returns>
    internal int GetSharedSecretHash()
    {
        if (sharedSecret.Length == 0) return 0;

        using SHA256 sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(sharedSecret);

        int numericHash = (hashBytes[0] << 24) |
                          (hashBytes[1] << 16) |
                          (hashBytes[2] << 8) |
                          hashBytes[3];

        return Math.Abs(numericHash);
    }

    /// <summary>
    /// Derives a fallback shared secret using temporary keys when ECDH is unavailable.
    /// </summary>
    /// <param name="local">The local temporary key.</param>
    /// <param name="remote">The remote temporary key.</param>
    /// <returns>A 256-bit hash derived from the ordered temporary keys.</returns>
    private static byte[] DeriveFallbackSecret(int local, int remote)
    {
        using SHA256 sha256 = SHA256.Create();

        int a = local < remote ? local : remote;
        int b = local < remote ? remote : local;

        byte[] buffer = new byte[8];

        Buffer.BlockCopy(BitConverter.GetBytes(a), 0, buffer, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(b), 0, buffer, 4, 4);

        return sha256.ComputeHash(buffer);
    }

    /// <summary>
    /// Gets a value indicating whether the key exchange data has been cleared for security.
    /// </summary>
    internal bool HasBeenCleared { get; private set; }

    /// <summary>
    /// Clears all sensitive key exchange data for security purposes.
    /// </summary>
    internal void ClearData()
    {
        if (HasBeenCleared)
            return;

        HasBeenCleared = true;
        publicKey = [];
        tempKey = 0;
        sharedSecret = [];
    }
}
