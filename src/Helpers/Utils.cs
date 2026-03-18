using AmongUs.Data;
using BetterAmongUs.Modules;
using BetterAmongUs.Patches.Gameplay.UI.Chat;
using InnerNet;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BetterAmongUs.Helpers;

/// <summary>
/// Provides utility methods for string manipulation, network operations, player lookups, and game utilities.
/// </summary>
internal static class Utils
{
    internal static Dictionary<string, Sprite> CachedSprites = [];

    // String extensions and formatting
    /// <summary>
    /// Wraps a string in size HTML tags.
    /// </summary>
    /// <param name="str">The string to format.</param>
    /// <param name="size">The size percentage.</param>
    /// <returns>The formatted string with size tags.</returns>
    internal static string Size(this string str, float size) => $"<size={size}%>{str}</size>";

    /// <summary>
    /// Removes size HTML tags from a string.
    /// </summary>
    /// <param name="text">The text to clean.</param>
    /// <returns>The text without size tags.</returns>
    internal static string RemoveSizeHtmlText(string text)
    {
        text = Regex.Replace(text, "<size=[^>]*>", "", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "</size>", "", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "{[^}]*}", "");
        text = text.Replace("\n", " ").Replace("\r", " ").Trim();

        return text;
    }

    /// <summary>
    /// Formats information from a StringBuilder, cleaning HTML and formatting separators.
    /// </summary>
    /// <param name="source">The StringBuilder containing the text to format.</param>
    /// <returns>The formatted string.</returns>
    internal static string FormatInfo(StringBuilder source)
    {
        if (source.Length == 0) return string.Empty;

        var sb = new StringBuilder();
        foreach (var part in source.ToString().Split("+++"))
        {
            if (!string.IsNullOrEmpty(RemoveHtmlText(part)))
            {
                sb.Append(part).Append(" - ");
            }
        }
        return sb.ToString().TrimEnd(" - ".ToCharArray());
    }

    /// <summary>
    /// Removes all HTML tags and formatting from a string.
    /// </summary>
    /// <param name="text">The text to clean.</param>
    /// <returns>The plain text without HTML.</returns>
    internal static string RemoveHtmlText(string text)
    {
        text = Regex.Replace(text, "<[^>]*>", "");
        text = Regex.Replace(text, "{[^}]*}", "");
        text = text.Replace("\n", " ").Replace("\r", " ");
        text = text.Trim();

        return text;
    }

    /// <summary>
    /// Checks if a string contains HTML or special formatting.
    /// </summary>
    /// <param name="text">The text to check.</param>
    /// <returns>True if the text contains HTML or formatting, false otherwise.</returns>
    internal static bool IsHtmlText(string text)
    {
        return Regex.IsMatch(text, "<[^>]*>") ||
               Regex.IsMatch(text, "{[^}]*}") ||
               text.Contains("\n") ||
               text.Contains("\r");
    }

    // Player lookup methods
    /// <summary>
    /// Gets ClientData from a client ID.
    /// </summary>
    /// <param name="clientId">The client ID to look up.</param>
    /// <returns>The ClientData if found, null otherwise.</returns>
    internal static ClientData? ClientFromClientId(int clientId) =>
        AmongUsClient.Instance.allClients.FirstOrDefaultIl2Cpp(cd => cd.Id == clientId);

    /// <summary>
    /// Gets NetworkedPlayerInfo from a player ID.
    /// </summary>
    /// <param name="playerId">The player ID to look up.</param>
    /// <returns>The NetworkedPlayerInfo if found, null otherwise.</returns>
    internal static NetworkedPlayerInfo? PlayerDataFromPlayerId(int playerId) =>
        GameData.Instance.AllPlayers.FirstOrDefaultIl2Cpp(data => data.PlayerId == playerId);

    /// <summary>
    /// Gets NetworkedPlayerInfo from a client ID.
    /// </summary>
    /// <param name="clientId">The client ID to look up.</param>
    /// <returns>The NetworkedPlayerInfo if found, null otherwise.</returns>
    internal static NetworkedPlayerInfo? PlayerDataFromClientId(int clientId) =>
        GameData.Instance.AllPlayers.FirstOrDefaultIl2Cpp(data => data.ClientId == clientId);

    /// <summary>
    /// Gets NetworkedPlayerInfo from a friend code.
    /// </summary>
    /// <param name="friendCode">The friend code to look up.</param>
    /// <returns>The NetworkedPlayerInfo if found, null otherwise.</returns>
    internal static NetworkedPlayerInfo? PlayerDataFromFriendCode(string friendCode) =>
        GameData.Instance.AllPlayers.FirstOrDefaultIl2Cpp(data => data.FriendCode == friendCode);

    /// <summary>
    /// Gets PlayerControl from a player ID.
    /// </summary>
    /// <param name="playerId">The player ID to look up.</param>
    /// <returns>The PlayerControl if found, null otherwise.</returns>
    internal static PlayerControl? PlayerFromPlayerId(int playerId) =>
        BAUPlugin.AllPlayerControls.FirstOrDefault(player => player.PlayerId == playerId);

    /// <summary>
    /// Gets PlayerControl from a client ID.
    /// </summary>
    /// <param name="clientId">The client ID to look up.</param>
    /// <returns>The PlayerControl if found, null otherwise.</returns>
    internal static PlayerControl? PlayerFromClientId(int clientId) =>
        BAUPlugin.AllPlayerControls.FirstOrDefault(player => player.GetClientId() == clientId);

    /// <summary>
    /// Gets PlayerControl from a network ID.
    /// </summary>
    /// <param name="netId">The network ID to look up.</param>
    /// <returns>The PlayerControl if found, null otherwise.</returns>
    internal static PlayerControl? PlayerFromNetId(uint netId) =>
        BAUPlugin.AllPlayerControls.FirstOrDefault(player => player.NetId == netId);

    // Chat functionality
    /// <summary>
    /// Adds a private chat message with custom formatting options.
    /// </summary>
    /// <param name="text">The message text.</param>
    /// <param name="overrideName">Optional override for the sender name.</param>
    /// <param name="setRight">Whether to align the message to the right side.</param>
    internal static void AddChatPrivate(string text, string overrideName = "", bool setRight = false)
    {
        if (!GameState.IsInGame)
            return;

        var chat = HudManager.Instance.Chat;
        if (chat == null)
            return;

        var data = PlayerControl.LocalPlayer.Data;
        if (data == null)
            return;

        var pooledBubble = chat.GetPooledBubble();
        var messageName = $"<color=#ffffff><b>(<color=#00ff44>{Translator.GetString("SystemMessage")}</color>)</b>" + ChatPatch.COMMAND_POSTFIX_ID;

        if (!string.IsNullOrEmpty(overrideName))
            messageName = overrideName + ChatPatch.COMMAND_POSTFIX_ID;

        try
        {
            pooledBubble.transform.SetParent(chat.scroller.Inner);
            pooledBubble.transform.localScale = Vector3.one;
            pooledBubble.SetCosmetics(data);
            pooledBubble.gameObject.transform.Find("PoolablePlayer").gameObject.SetActive(false);
            pooledBubble.ColorBlindName.gameObject.SetActive(false);

            if (!setRight)
            {
                pooledBubble.SetLeft();
                pooledBubble.gameObject.transform.Find("NameText (TMP)").transform.localPosition += new Vector3(-0.7f, 0f);
                pooledBubble.gameObject.transform.Find("ChatText (TMP)").transform.localPosition += new Vector3(-0.7f, 0f);
            }
            else
            {
                pooledBubble.SetRight();
            }

            chat.SetChatBubbleName(pooledBubble, data, false, false, PlayerNameColor.Get(data), null);
            pooledBubble.SetText(text);
            pooledBubble.AlignChildren();
            chat.AlignAllBubbles();
            pooledBubble.NameText.text = messageName;

            if (!chat.IsOpenOrOpening && chat.notificationRoutine == null)
            {
                chat.notificationRoutine = chat.StartCoroutine(chat.BounceDot());
            }

            SoundManager.Instance.PlaySound(chat.messageSound, false, 1f, null).pitch = 0.5f + data.PlayerId / 15f;
        }
        catch
        {
        }
    }

    // System type checks
    /// <summary>
    /// Checks if a SystemTypes value represents a sabotage type.
    /// </summary>
    /// <param name="type">The SystemTypes value to check.</param>
    /// <returns>True if the system type is a sabotage, false otherwise.</returns>
    internal static bool SystemTypeIsSabotage(SystemTypes type) => type is
        SystemTypes.Reactor or SystemTypes.Laboratory or SystemTypes.Comms or
        SystemTypes.LifeSupp or SystemTypes.MushroomMixupSabotage or
        SystemTypes.HeliSabotage or SystemTypes.Electrical;

    /// <summary>
    /// Checks if an integer value represents a sabotage SystemTypes.
    /// </summary>
    /// <param name="typeNum">The integer value to check.</param>
    /// <returns>True if the value represents a sabotage system type.</returns>
    internal static bool SystemTypeIsSabotage(int typeNum) =>
        SystemTypeIsSabotage((SystemTypes)typeNum);

    // Hashing utilities
    /// <summary>
    /// Gets the hashed PUID of a player.
    /// </summary>
    /// <param name="player">The player to get the hashed PUID for.</param>
    /// <returns>The hashed PUID string.</returns>
    internal static string GetHashPuid(PlayerControl player)
    {
        if (player == null)
            return "";

        if (player.Data == null)
            return "";

        if (player.Data.Puid == null)
            return "";

        return GetHashStr(player.Data.Puid);
    }

    /// <summary>
    /// Computes a truncated SHA256 hash of a string.
    /// </summary>
    /// <param name="str">The string to hash.</param>
    /// <returns>A 9-character hash (first 5 + last 4 characters of SHA256).</returns>
    internal static string GetHashStr(this string str)
    {
        if (string.IsNullOrEmpty(str)) return "";

        using var sha256 = SHA256.Create();
        var sha256Bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(str));
        var sha256Hash = BitConverter.ToString(sha256Bytes).Replace("-", "").ToLower();
        return sha256Hash[..5] + sha256Hash[^4..];
    }

    /// <summary>
    /// Computes a 16-bit hash of a string using SHA256.
    /// </summary>
    /// <param name="input">The string to hash.</param>
    /// <returns>A 16-bit hash value.</returns>
    internal static ushort GetHashUInt16(string input)
    {
        if (string.IsNullOrEmpty(input)) return 0;
        return (ushort)(BitConverter.ToUInt16(SHA256.HashData(Encoding.UTF8.GetBytes(input)), 0) % 65536);
    }

    // Color utilities
    /// <summary>
    /// Gets the hexadecimal color code for a role team.
    /// </summary>
    /// <param name="team">The role team type.</param>
    /// <returns>The hex color string for the team.</returns>
    internal static string GetTeamHexColor(RoleTeamTypes team)
    {
        return team == RoleTeamTypes.Impostor ? "#f00202" : "#8cffff";
    }

    /// <summary>
    /// Converts a Color32 to a hexadecimal string.
    /// </summary>
    /// <param name="color">The Color32 to convert.</param>
    /// <returns>The hexadecimal color string.</returns>
    internal static string Color32ToHex(Color32 color) => $"#{color.r:X2}{color.g:X2}{color.b:X2}{255:X2}";

    /// <summary>
    /// Converts a hexadecimal string to a Color32.
    /// </summary>
    /// <param name="hex">The hexadecimal color string.</param>
    /// <returns>The Color32 representation.</returns>
    internal static Color HexToColor32(string hex)
    {
        if (hex.StartsWith("#"))
        {
            hex = hex[1..];
        }

        byte r = byte.Parse(hex[..2], System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

        return new Color32(r, g, b, 255);
    }

    /// <summary>
    /// Linearly interpolates between multiple colors based on a value within a range.
    /// </summary>
    /// <param name="colors">The array of colors to interpolate between.</param>
    /// <param name="lerpRange">The minimum and maximum range for interpolation.</param>
    /// <param name="t">The interpolation value within the range.</param>
    /// <param name="reverse">Whether to reverse the color array order.</param>
    /// <returns>The interpolated color.</returns>
    internal static Color LerpColor(Color[] colors, (float min, float max) lerpRange, float t, bool reverse = false)
    {
        float normalizedT = Mathf.InverseLerp(lerpRange.min, lerpRange.max, t);

        if (colors.Length == 1)
            return colors[0];

        if (reverse)
        {
            Array.Reverse(colors);
        }

        if (normalizedT <= 0f)
            return colors[0];
        if (normalizedT >= 1f)
            return colors[^1];

        float segmentSize = 1f / (colors.Length - 1);
        int segmentIndex = (int)(normalizedT / segmentSize);
        float segmentT = (normalizedT - segmentIndex * segmentSize) / segmentSize;

        return Color.Lerp(colors[segmentIndex], colors[segmentIndex + 1], segmentT);
    }

    // Network and UI operations
    /// <summary>
    /// Disconnects the player's account from online services.
    /// </summary>
    /// <param name="apiError">Whether this is due to an API error.</param>
    internal static void DisconnectAccountFromOnline(bool apiError = false)
    {
        if (GameState.IsInGame)
        {
            AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
        }

        DataManager.Player.Account.LoginStatus = EOSManager.AccountLoginStatus.Offline;
        DataManager.Player.Save();

        if (apiError)
        {
            ShowPopUp(Translator.GetString("DataBaseConnect.InitFailure"), true);
        }
    }

    /// <summary>
    /// Shows a settings change notification in the HUD.
    /// </summary>
    /// <param name="id">The notification ID for deduplication.</param>
    /// <param name="text">The notification text.</param>
    /// <param name="playSound">Whether to play the notification sound.</param>
    internal static void SettingsChangeNotifier(int id, string text, bool playSound = true)
    {
        var notifier = HudManager.Instance.Notifier;

        if (notifier.lastMessageKey == id && notifier.activeMessages.Count > 0)
        {
            notifier.activeMessages[^1].UpdateMessage(text);
        }
        else
        {
            notifier.lastMessageKey = id;
            var newMessage = UnityEngine.Object.Instantiate(
                notifier.notificationMessageOrigin,
                Vector3.zero,
                Quaternion.identity,
                notifier.transform
            );

            newMessage.transform.localPosition = new Vector3(0f, 0f, -2f);
            newMessage.SetUp(text, notifier.settingsChangeSprite, notifier.settingsChangeColor, (Action)(() =>
            {
                notifier.OnMessageDestroy(newMessage);
            }));

            notifier.ShiftMessages();
            notifier.AddMessageToQueue(newMessage);
        }

        if (playSound)
        {
            SoundManager.Instance.PlaySoundImmediate(notifier.settingsChangeSound, false, 1f, 1f, null);
        }
    }

    /// <summary>
    /// Disconnects the local player from the game with an optional reason message.
    /// </summary>
    /// <param name="reason">The reason for disconnection.</param>
    /// <param name="showReason">Whether to show the reason in a popup.</param>
    internal static void DisconnectSelf(string reason, bool showReason = true)
    {
        AmongUsClient.Instance.ExitGame(0);

        LateTask.Schedule(() =>
        {
            SceneChanger.ChangeScene("MainMenu");

            if (showReason)
            {
                LateTask.Schedule(() =>
                {
                    var lines = "<color=#ebbd34>----------------------------------------------------------------------------------------------</color>";
                    ShowPopUp($"{lines}\n\n\n<size=150%>{reason}</size>\n\n\n{lines}");
                }, 0.1f, "DisconnectSelf 2");
            }
        }, 0.2f, "DisconnectSelf 1");
    }

    /// <summary>
    /// Shows a popup message using the DisconnectPopup.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="enableWordWrapping">Whether to enable word wrapping.</param>
    internal static void ShowPopUp(string text, bool enableWordWrapping = false)
    {
        DisconnectPopup.Instance.gameObject.SetActive(true);
        DisconnectPopup.Instance._textArea.enableWordWrapping = enableWordWrapping;
        DisconnectPopup.Instance._textArea.text = text;
    }

    // Resource loading
    /// <summary>
    /// Loads a sprite from an embedded resource with caching.
    /// </summary>
    /// <param name="path">The resource path.</param>
    /// <param name="pixelsPerUnit">The pixels per unit for the sprite.</param>
    /// <returns>The loaded sprite, or null if loading fails.</returns>
    internal static Sprite? LoadSprite(string path, float pixelsPerUnit = 1f)
    {
        try
        {
            var cacheKey = path + pixelsPerUnit;
            if (CachedSprites.TryGetValue(cacheKey, out var sprite))
                return sprite;

            var texture = LoadTextureFromResources(path);
            if (texture == null)
                return null;

            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;

            return CachedSprites[cacheKey] = sprite;
        }
        catch (Exception ex)
        {
            Logger_.Error(ex);
            return null;
        }
    }

    /// <summary>
    /// Loads a Texture2D from an embedded resource.
    /// </summary>
    /// <param name="path">The resource path.</param>
    /// <returns>The loaded texture, or null if loading fails.</returns>
    internal static Texture2D? LoadTextureFromResources(string path)
    {
        try
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
            if (stream == null)
                return null;

            var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                if (!texture.LoadImage(ms.ToArray(), false))
                    return null;
            }

            return texture;
        }
        catch (Exception ex)
        {
            Logger_.Error(ex);
            return null;
        }
    }

    // Platform utilities
    /// <summary>
    /// Gets the platform name for a player.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="useTag">Whether to include the platform tag (PC, Console, Mobile).</param>
    /// <returns>The platform name string.</returns>
    internal static string GetPlatformName(PlayerControl player, bool useTag = false)
    {
        if (player == null)
            return string.Empty;

        var client = player.GetClient();
        if (client == null)
            return string.Empty;

        if (client.PlatformData == null)
            return string.Empty;

        return GetPlatformName(client.PlatformData.Platform, useTag);
    }

    /// <summary>
    /// Gets the platform name from a Platforms enum value.
    /// </summary>
    /// <param name="platform">The platform enum value.</param>
    /// <param name="useTag">Whether to include the platform tag.</param>
    /// <returns>The platform name string.</returns>
    internal static string GetPlatformName(Platforms platform, bool useTag = false)
    {
        var (platformName, tag) = platform switch
        {
            Platforms.StandaloneSteamPC => ("Steam", "PC"),
            Platforms.StandaloneEpicPC => ("Epic Games", "PC"),
            Platforms.StandaloneWin10 => ("Microsoft Store", "PC"),
            Platforms.StandaloneMac => ("Mac OS", "PC"),
            Platforms.StandaloneItch => ("Itch.io", "PC"),
            Platforms.Xbox => ("Xbox", "Console"),
            Platforms.Playstation => ("Playstation", "Console"),
            Platforms.Switch => ("Switch", "Console"),
            Platforms.Android => ("Android", "Mobile"),
            Platforms.IPhone => ("IPhone", "Mobile"),
            Platforms.Unknown => ("None", ""),
            _ => (string.Empty, string.Empty)
        };

        if (string.IsNullOrEmpty(platformName))
            return string.Empty;

        return useTag && !string.IsNullOrEmpty(tag) ? $"{tag}: {platformName}" : platformName;
    }
}