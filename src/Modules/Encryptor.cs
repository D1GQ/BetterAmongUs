using System.Security.Cryptography;
using UnityEngine;

namespace BetterAmongUs.Modules;

/// <summary>
/// Provides AES encryption and decryption utilities for sensitive data.
/// </summary>
internal static class Encryptor
{
    private static byte[] Key = [];
    private static byte[] IV = [];

    /// <summary>
    /// Initializes the encryption system by loading or generating AES keys.
    /// </summary>
    internal static void Initialize()
    {
        try
        {
            if (PlayerPrefs.HasKey("BAUEncryptionKey") && PlayerPrefs.HasKey("BAUEncryptionIV"))
            {
                string keyString = PlayerPrefs.GetString("BAUEncryptionKey");
                string ivString = PlayerPrefs.GetString("BAUEncryptionIV");

                if (string.IsNullOrEmpty(keyString) || string.IsNullOrEmpty(ivString))
                {
                    throw new Exception("Keys are empty");
                }

                Key = Convert.FromBase64String(keyString);
                IV = Convert.FromBase64String(ivString);

                if (Key.Length != 32 || IV.Length != 16)
                {
                    throw new Exception("Invalid key length");
                }
            }
            else
            {
                GenerateAndSaveKeys();
            }
        }
        catch
        {
            PlayerPrefs.DeleteKey("BAUEncryptionKey");
            PlayerPrefs.DeleteKey("BAUEncryptionIV");
            PlayerPrefs.Save();
            GenerateAndSaveKeys();
        }
    }

    /// <summary>
    /// Generates a new 256-bit AES key and 128-bit initialization vector (IV),
    /// then saves them to PlayerPrefs as Base64 strings.
    /// </summary>
    private static void GenerateAndSaveKeys()
    {
        using (Aes aes = Aes.Create())
        {
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.GenerateKey();
            aes.GenerateIV();
            Key = aes.Key;
            IV = aes.IV;
        }

        PlayerPrefs.SetString("BAUEncryptionKey", Convert.ToBase64String(Key));
        PlayerPrefs.SetString("BAUEncryptionIV", Convert.ToBase64String(IV));
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Encrypts a plain text string using AES encryption.
    /// </summary>
    /// <param name="input">The plain text string to encrypt.</param>
    /// <returns>A base64-encoded string containing the encrypted data.</returns>
    /// <exception cref="InvalidOperationException">Thrown if encryption is not initialized.</exception>
    internal static string Encrypt(string input)
    {
        if (Key == null || Key.Length == 0 || IV == null || IV.Length == 0)
        {
            throw new InvalidOperationException("Encryption not initialized. Call InitializeEncryption() first.");
        }

        using Aes aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;

        using MemoryStream memoryStream = new();
        using CryptoStream cryptoStream = new(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
        using (StreamWriter streamWriter = new(cryptoStream))
        {
            streamWriter.Write(input);
        }
        return Convert.ToBase64String(memoryStream.ToArray());
    }

    /// <summary>
    /// Decrypts an AES-encrypted base64 string back to plain text.
    /// </summary>
    /// <param name="input">The base64-encoded encrypted string.</param>
    /// <returns>The decrypted plain text string.</returns>
    /// <exception cref="InvalidOperationException">Thrown if encryption is not initialized.</exception>
    /// <exception cref="FormatException">Thrown if the input is not valid Base64.</exception>
    /// <exception cref="CryptographicException">Thrown if decryption fails due to invalid key or corrupted data.</exception>
    internal static string Decrypt(string input)
    {
        if (Key == null || Key.Length == 0 || IV == null || IV.Length == 0)
        {
            throw new InvalidOperationException("Encryption not initialized. Call InitializeEncryption() first.");
        }

        using Aes aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;

        using MemoryStream memoryStream = new(Convert.FromBase64String(input));
        using CryptoStream cryptoStream = new(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using StreamReader streamReader = new(cryptoStream);
        return streamReader.ReadToEnd();
    }
}