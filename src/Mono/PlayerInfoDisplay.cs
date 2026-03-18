using AmongUs.Data;
using AmongUs.GameOptions;
using BetterAmongUs.Data;
using BetterAmongUs.Data.Config;
using BetterAmongUs.Helpers;
using BetterAmongUs.Modules;
using BetterAmongUs.Patches.Gameplay.UI.Settings;
using BetterAmongUs.Structs;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Mono;

/// <summary>
/// Displays extended player information during gameplay.
/// </summary>
internal class PlayerInfoDisplay : MonoBehaviour
{
    protected PlayerControl? _player;
    protected TextMeshPro? _nameText;
    protected TextMeshPro? _infoText;
    protected TextMeshPro? _topText;
    protected TextMeshPro? _bottomText;

    private readonly StringBuilder _sbTag = new(256);
    private readonly StringBuilder _sbTagTop = new(256);
    private readonly StringBuilder _sbTagBottom = new(256);
    private string _lastTopText = "", _lastBottomText = "", _lastInfoText = "";
    private int _lastUpdateFrame;
    private const int UPDATE_COOLDOWN = 10;

    /// <summary>
    /// Cached regex pattern for friend code validation.
    /// </summary>
    private static readonly Regex _friendCodePattern = new(@"^[a-zA-Z0-9#]+$", RegexOptions.Compiled);

    private CachedTranslations _cachedTranslations = new();

    /// <summary>
    /// Cached color values for performance optimization.
    /// </summary>
    private static readonly Dictionary<string, Color32> _cachedColors = new()
    {
        ["#00f583"] = Utils.HexToColor32("#00f583"),
        ["#4f0000"] = Utils.HexToColor32("#4f0000"),
        ["#fc0000"] = Utils.HexToColor32("#fc0000"),
        ["#8731e7"] = Utils.HexToColor32("#8731e7")
    };

    /// <summary>
    /// Cached translations for performance optimization.
    /// </summary>
    private class CachedTranslations
    {
        internal readonly string Loading = Translator.GetString("Player.Loading");
        internal readonly string PlatformHidden = Translator.GetString("Player.PlatformHidden");
        internal readonly string NoFriendCode = Translator.GetString("Player.NoFriendCode");
        internal readonly string SickoUser = Translator.GetString("Player.SickoUser");
        internal readonly string AUMUser = Translator.GetString("Player.AUMUser");
        internal readonly string KNUser = Translator.GetString("Player.KNUser");
        internal readonly string KnownCheater = Translator.GetString("Player.KnownCheater");
        internal readonly string BetterUser = Translator.GetString("Player.BetterUser");
    }

    /// <summary>
    /// Initializes the player info display.
    /// </summary>
    /// <param name="player">The player to display info for.</param>
    internal void Init(PlayerControl player)
    {
        _player = player;

        var nameTextTransform = player.gameObject.transform.Find("Names/NameText_TMP");
        if (nameTextTransform == null)
            return;

        _nameText = nameTextTransform.GetComponent<TextMeshPro>();

        _infoText = InstantiatePlayerInfoText("InfoText_Info_TMP", new Vector3(0f, 0.25f), nameTextTransform);
        _topText = InstantiatePlayerInfoText("InfoText_T_TMP", new Vector3(0f, 0.15f), nameTextTransform);
        _bottomText = InstantiatePlayerInfoText("InfoText_B_TMP", new Vector3(0f, -0.15f), nameTextTransform);
        _infoText.fontSize = 1.3f;
        _topText.fontSize = 1.3f;
        _bottomText.fontSize = 1.3f;
    }

    /// <summary>
    /// Instantiates a player info text object.
    /// </summary>
    /// <param name="name">The name of the text object.</param>
    /// <param name="positionOffset">The position offset from the parent.</param>
    /// <param name="parent">The parent transform.</param>
    /// <returns>The created TextMeshPro component.</returns>
    protected TextMeshPro InstantiatePlayerInfoText(string name, Vector3 positionOffset, Transform parent)
    {
        var newTextObject = Instantiate(_nameText, parent);
        newTextObject.name = name;
        newTextObject.transform.DestroyChildren();
        newTextObject.transform.position += positionOffset;

        var textMesh = newTextObject.GetComponent<TextMeshPro>();
        textMesh.text = string.Empty;
        newTextObject.gameObject.SetActive(true);

        return textMesh;
    }

    /// <summary>
    /// Resets all text displays to empty.
    /// </summary>
    private void ResetText()
    {
        if (_infoText != null)
        {
            _infoText.SetText(string.Empty);
        }

        if (_topText != null)
        {
            _topText.SetText(string.Empty);
        }

        if (_bottomText != null)
        {
            _bottomText.SetText(string.Empty);
        }
    }

    /// <summary>
    /// LateUpdate override with cooldown for performance optimization.
    /// </summary>
    protected virtual void LateUpdate()
    {
        if (Time.frameCount - _lastUpdateFrame < UPDATE_COOLDOWN)
            return;

        if (_player == null || _player.Data == null || _nameText == null)
        {
            ResetText();
            return;
        }

        _sbTag.Clear();
        _sbTagTop.Clear();
        _sbTagBottom.Clear();

        UpdatePlayerInfo();
        UpdatePlayerHighlight();
        UpdateColorBlindTextPosition();
        _nameText.transform.parent.localPosition = new Vector3(0f, 0.8f, -0.5f);

        _lastUpdateFrame = Time.frameCount;
    }

    /// <summary>
    /// Updates player information display.
    /// </summary>
    private void UpdatePlayerInfo()
    {
        if (_player == null)
            return;

        if (_player.Data == null)
            return;

        if (_nameText == null)
            return;

        if (_cachedTranslations == null)
            return;

        if (_sbTag == null || _sbTagTop == null || _sbTagBottom == null)
            return;

        if (_topText == null || _bottomText == null || _infoText == null)
            return;

        var betterData = _player.BetterData();

        if (!_player.DataIsCollected())
        {
            _nameText.text = _cachedTranslations.Loading;
            return;
        }

        if (!BAUConfigs.LobbyPlayerInfo.Value && GameState.IsLobby)
        {
            ResetText();
            _player.RawSetName(_player.Data.PlayerName);
            return;
        }

        string newName = _player.Data.PlayerName;
        string hashPuid = Utils.GetHashPuid(_player);
        string platform = Utils.GetPlatformName(_player, useTag: true);

        string friendCode = ValidateFriendCode(out string friendCodeColor);

        if (DataManager.Settings == null || DataManager.Settings.Gameplay == null)
            return;

        if (DataManager.Settings.Gameplay.StreamerMode)
            platform = _cachedTranslations.PlatformHidden;

        if (!_player.IsInShapeshift())
            SetPlayerOutline(_sbTag);

        if (GameState.IsInGame && GameState.IsLobby && !GameState.IsFreePlay)
        {
            SetLobbyInfo(ref newName, betterData, _sbTag);

            _sbTagTop.Append($"<color=#9e9e9e>{platform}</color>+++")
                    .Append($"<color=#ffd829>Lv: {_player.Data.PlayerLevel + 1}</color>+++");

            _sbTagBottom.Append($"<color={friendCodeColor}>{friendCode}</color>+++");
        }
        else if ((GameState.IsInGame || GameState.IsFreePlay) && !GameState.IsHideNSeek)
        {
            SetInGameInfo(_sbTagTop);
        }

        if (!_player.IsInShapeshift())
        {
            if (_player.IsImpostorTeammate())
                newName = newName.ToColor(Colors.ImpostorRed);

            _player.RawSetName(newName);
        }
        else
        {
            var targetData = Utils.PlayerDataFromPlayerId(_player.shapeshiftTargetPlayerId);
            if (targetData == null)
                return;

            var betterTargetData = targetData.BetterData();
            string name = betterTargetData != null ? betterTargetData.RealName : targetData.PlayerName;

            if (_player.IsImpostorTeammate())
                name = name.ToColor(Colors.ImpostorRed);

            _player.RawSetName(name);
        }

        UpdateTextIfChanged(_topText, _sbTagTop, ref _lastTopText);
        UpdateTextIfChanged(_bottomText, _sbTagBottom, ref _lastBottomText);
        UpdateTextIfChanged(_infoText, _sbTag, ref _lastInfoText);
    }

    /// <summary>
    /// Updates text if changed, optimizing performance.
    /// </summary>
    /// <param name="textMesh">TextMeshPro component to update.</param>
    /// <param name="sb">StringBuilder containing new text.</param>
    /// <param name="lastValue">Reference to last value for comparison.</param>
    private static void UpdateTextIfChanged(TextMeshPro textMesh, StringBuilder sb, ref string lastValue)
    {
        if (textMesh == null)
            return;

        string newText = Utils.FormatInfo(sb);
        if (newText != lastValue)
        {
            textMesh.SetText(newText);
            lastValue = newText;
        }
    }

    /// <summary>
    /// Validates and formats the player's friend code.
    /// </summary>
    /// <param name="color">Output parameter for the friend code color.</param>
    /// <returns>The formatted friend code string.</returns>
    private string ValidateFriendCode(out string color)
    {
        color = "#FFFFFF";
        if (_player == null || _player.Data == null)
            return string.Empty;

        void TryKick()
        {
            if (GameState.IsHost && BetterGameSettings.InvalidFriendCode.GetBool())
            {
                string kickMessage = string.Format(Translator.GetString("AntiCheat.KickMessage"),
                    Translator.GetString("AntiCheat.ByAntiCheat"),
                    Translator.GetString("AntiCheat.Reason.InvalidFriendCode"));
                _player.Kick(true, kickMessage, true);
            }
        }

        string friendCode = _player.Data.FriendCode;

        // Proper friend code validation
        bool isValidFriendCode = true;

        if (string.IsNullOrEmpty(friendCode))
        {
            friendCode = _cachedTranslations.NoFriendCode;
            color = "#ff0000";
            isValidFriendCode = false;
            TryKick();
        }
        else
        {
            // Check if it matches the basic pattern (alphanumeric and # only)
            if (!_friendCodePattern.IsMatch(friendCode) || friendCode.Contains(' '))
            {
                isValidFriendCode = false;
                TryKick();
            }
            else
            {
                // Check if it ends with # followed by exactly 4 digits
                var hashtagMatch = Regex.Match(friendCode, @"#\d{4}$");
                if (!hashtagMatch.Success)
                {
                    isValidFriendCode = false;
                    TryKick();
                }
                else
                {
                    // The part before the # should be reasonable length
                    string namePart = friendCode[..^5];
                    if (namePart.Length < 5 || namePart.Length > 10)
                    {
                        isValidFriendCode = false;
                        TryKick();
                    }
                }
            }
        }

        color = isValidFriendCode ? "#00f7ff" : "#ff0000";

        if (DataManager.Settings.Gameplay.StreamerMode)
        {
            friendCode = new string('*', friendCode.Length);
        }

        return friendCode.Trim();
    }

    /// <summary>
    /// Sets player outline based on data from BetterDataManager.
    /// </summary>
    /// <param name="sbTag">StringBuilder for tag text.</param>
    [HideFromIl2Cpp]
    private void SetPlayerOutline(StringBuilder sbTag)
    {
        if (_player == null)
            return;

        if (_player.Data == null)
            return;

        string hashPuid = Utils.GetHashPuid(_player);
        string friendCode = _player.Data.FriendCode;

        var color = _player.cosmetics.currentBodySprite.BodySprite.material.GetColor("_OutlineColor");

        if (ContainsPlayerData(BetterDataManager.BetterDataFile.SickoData, _player.Data))
        {
            sbTag.Append($"<color=#00f583>{_cachedTranslations.SickoUser}</color>+++");
            _player.SetOutlineByHex(true, "#00f583");
        }
        else if (ContainsPlayerData(BetterDataManager.BetterDataFile.AUMData, _player.Data))
        {
            sbTag.Append($"<color=#4f0000>{_cachedTranslations.AUMUser}</color>+++");
            _player.SetOutlineByHex(true, "#4f0000");
        }
        else if (ContainsPlayerData(BetterDataManager.BetterDataFile.KNData, _player.Data))
        {
            sbTag.Append($"<color=#8731e7>{_cachedTranslations.KNUser}</color>+++");
            _player.SetOutlineByHex(true, "#8731e7");
        }
        else if (ContainsPlayerData(BetterDataManager.BetterDataFile.CheatData, _player.Data))
        {
            sbTag.Append($"<color=#fc0000>{_cachedTranslations.KnownCheater}</color>+++");
            _player.SetOutlineByHex(true, "#fc0000");
        }
        else if (_cachedColors.Any(kvp => color == kvp.Value))
        {
            _player.SetOutline(false, null);
        }
    }

    /// <summary>
    /// Checks if player data exists in a HashSet of UserInfo.
    /// </summary>
    /// <param name="dataList">HashSet of UserInfo to check.</param>
    /// <param name="playerData">Player data to look for.</param>
    /// <returns>True if player data exists in the HashSet.</returns>
    [HideFromIl2Cpp]
    private static bool ContainsPlayerData(HashSet<UserInfo> dataList, NetworkedPlayerInfo playerData)
    {
        foreach (var info in dataList)
        {
            if (info.CheckPlayerData(playerData))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Sets lobby specific information.
    /// </summary>
    [HideFromIl2Cpp]
    private void SetLobbyInfo(ref string newName, ExtendedPlayerInfo betterData, StringBuilder sbTag)
    {
        if (betterData == null)
            return;

        if (_player.IsHost() && BAUConfigs.LobbyPlayerInfo.Value)
            newName = _player.GetPlayerNameAndColor();

        if ((_player.IsLocalPlayer() || betterData.IsBetterUser) && !GameState.IsInGamePlay)
        {
            string verificationSymbol = betterData.IsVerifiedBetterUser || _player.IsLocalPlayer() ? "✓ " : "";

            sbTag.AppendFormat("<color=#0dff00>{1}{0}</color>+++",
                _cachedTranslations.BetterUser, verificationSymbol);
        }
        sbTag.Append($"<color=#b554ff>ID: {_player.PlayerId}</color>+++");
    }

    /// <summary>
    /// Sets in-game specific information.
    /// </summary>
    /// <param name="sbTagTop">StringBuilder for top tag text.</param>
    [HideFromIl2Cpp]
    private void SetInGameInfo(StringBuilder sbTagTop)
    {
        if (_player.IsImpostorTeammate() || _player.IsLocalPlayer() ||
            !PlayerControl.LocalPlayer.IsAlive() && !PlayerControl.LocalPlayer.Is(RoleTypes.GuardianAngel))
        {
            string roleInfo = _player.GetRoleName().ToColor(_player.Data.Role.TeamColor);

            if (!_player.IsImpostorTeam() && _player.myTasks.Count > 0)
            {
                int completedTasks = 0;
                foreach (var task in _player.Data.Tasks)
                {
                    if (task.Complete) completedTasks++;
                }
                roleInfo += $" <color=#cbcbcb>({completedTasks}/{_player.Data.Tasks.Count})</color>";
            }

            sbTagTop.Append(roleInfo + "+++");
        }
    }

    /// <summary>
    /// Updates player highlight/outline.
    /// </summary>
    private void UpdatePlayerHighlight()
    {
        SetPlayerOutline(new StringBuilder(32));
    }

    /// <summary>
    /// Updates color blind text position.
    /// </summary>
    private void UpdateColorBlindTextPosition()
    {
        var text = _player.cosmetics.colorBlindText;
        if (!text.enabled)
            return;

        if (!_player.onLadder && !_player.MyPhysics.Animations.IsPlayingAnyLadderAnimation())
        {
            text.transform.localPosition = new Vector3(0f, -1.3f, 0.4999f);
        }
        else
        {
            text.transform.localPosition = new Vector3(0f, -1.5f, 0.4999f);
        }
    }
}