using AmongUs.Data;
using BetterAmongUs.Attributes;
using BetterAmongUs.Data;
using BetterAmongUs.Data.Config;
using BetterAmongUs.Generated;
using BetterAmongUs.Modules;
using BetterAmongUs.Modules.Support;
using BetterAmongUs.MonoScripts.Extended;
using BetterAmongUs.Patches.Gameplay.UI.Settings;
using BetterAmongUs.Structs;
using BetterAmongUs.Utilities;
using Il2CppInterop.Runtime.Attributes;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.MonoScripts;

/// <summary>
/// Displays extended player information during gameplay.
/// </summary>
[RegisterInIl2Cpp]
internal class PlayerInfoDisplay : MonoBehaviour
{
    protected PlayerControl? _player;
    protected TextMeshPro? _nameText;
    protected TextMeshPro? _infoText;
    protected TextMeshPro? _topText;
    protected TextMeshPro? _bottomText;

    private readonly SplitStringBuilder _ssbTag = new(100, '-');
    private readonly SplitStringBuilder _ssbTagTop = new(100, '-');
    private readonly SplitStringBuilder _ssbTagBottom = new(100, '-');
    private string _lastTopText = "", _lastBottomText = "", _lastInfoText = "";
    private int _lastUpdateFrame;
    private const int UPDATE_COOLDOWN = 10;

    /// <summary>
    /// Cached regex pattern for friend code validation.
    /// </summary>
    private static readonly Regex _friendCodePattern = new(@"^[a-zA-Z0-9#]+$", RegexOptions.Compiled);

    /// <summary>
    /// Cached color values for performance optimization.
    /// </summary>
    private static readonly Dictionary<string, Color32> _cachedColors = new()
    {
        [Colors.SickoHexColor] = Utils.HexToColor32(Colors.SickoHexColor),
        [Colors.AUMHexColor] = Utils.HexToColor32(Colors.AUMHexColor),
        [Colors.KNHexColor] = Utils.HexToColor32(Colors.KNHexColor),
        [Colors.MMCHexColor] = Utils.HexToColor32(Colors.MMCHexColor),
        [Colors.CheaterHexColor] = Utils.HexToColor32(Colors.CheaterHexColor)
    };

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

        if (_player == null || _player.Data == null || _player.Data.ExtendedData() == null || _nameText == null)
        {
            ResetText();
            return;
        }

        _ssbTag.Clear();
        _ssbTagTop.Clear();
        _ssbTagBottom.Clear();

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

        if (_topText == null || _bottomText == null || _infoText == null)
            return;

        var betterData = _player.ExtendedData();

        if (!_player.DataIsCollected())
        {
            _nameText.text = TranslationStrings.Player_Loading.LocalizedString;
            return;
        }

        if (!BAUConfigs.LobbyPlayerInfo.Value && GameState.IsLobby)
        {
            ResetText();
            if (!BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_NameOverride))
            {
                _player.RawSetName(_player.Data.PlayerName);
            }
            return;
        }

        string newName = _player.Data.PlayerName;
        string hashPuid = Utils.GetHashPuid(_player);
        string platform = Utils.GetPlatformName(_player, useTag: true);

        string friendCode = ValidateFriendCode(out string friendCodeColor);

        if (DataManager.Settings == null || DataManager.Settings.Gameplay == null)
            return;

        if (DataManager.Settings.Gameplay.StreamerMode)
            platform = TranslationStrings.Player_PlatformHidden.LocalizedString;

        if (!_player.IsInShapeshift())
            SetPlayerOutline(_ssbTag);

        if (GameState.IsInGame && GameState.IsLobby && !GameState.IsFreePlay)
        {
            SetLobbyInfo(ref newName, betterData, _ssbTag);

            _ssbTagTop.Append($"<color=#9e9e9e>{platform}</color>")
                .Append($"<color=#ffd829>Lv: {_player.Data.PlayerLevel + 1}</color>");

            _ssbTagBottom.Append($"<color={friendCodeColor}>{friendCode}</color>");
        }
        else if ((GameState.IsInGame || GameState.IsFreePlay) && !GameState.IsHideNSeek)
        {
            SetInGameInfo(_ssbTagTop);
        }

        if (!BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_NameOverride))
        {
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

                var betterTargetData = targetData.ExtendedData();
                string name = betterTargetData != null ? betterTargetData.RealName : targetData.PlayerName;

                if (_player.IsImpostorTeammate())
                    name = name.ToColor(Colors.ImpostorRed);

                _player.RawSetName(name);
            }
        }

        UpdateTextIfChanged(_topText, _ssbTagTop, ref _lastTopText);
        UpdateTextIfChanged(_bottomText, _ssbTagBottom, ref _lastBottomText);
        UpdateTextIfChanged(_infoText, _ssbTag, ref _lastInfoText);
    }

    /// <summary>
    /// Updates text if changed, optimizing performance.
    /// </summary>
    /// <param name="textMesh">TextMeshPro component to update.</param>
    /// <param name="ssb">StringBuilder containing new text.</param>
    /// <param name="lastValue">Reference to last value for comparison.</param>
    private static void UpdateTextIfChanged(TextMeshPro textMesh, SplitStringBuilder ssb, ref string lastValue)
    {
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_PlayerInfo))
        {
            textMesh.SetText(string.Empty);
            return;
        }

        if (textMesh == null)
            return;

        string newText = ssb.ToString();
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
                string kickMessage = TranslationStrings.AntiCheat_KickMessage.Format(TranslationStrings.AntiCheat_ByAntiCheat, TranslationStrings.AntiCheat_Reason_InvalidFriendCode);
                _player.Kick(true, kickMessage, true);
            }
        }

        string friendCode = _player.Data.FriendCode;

        // Proper friend code validation
        bool isValidFriendCode = true;

        if (string.IsNullOrEmpty(friendCode))
        {
            friendCode = TranslationStrings.Player_NoFriendCode.LocalizedString;
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
    /// <param name="ssbTag">StringBuilder for tag text.</param>
    [HideFromIl2Cpp]
    private void SetPlayerOutline(SplitStringBuilder? ssbTag)
    {
        if (_player == null)
            return;

        if (_player.Data == null)
            return;

        string hashPuid = Utils.GetHashPuid(_player);
        string friendCode = _player.Data.FriendCode;

        var color = _player.cosmetics.currentBodySprite.BodySprite.material.GetColor("_OutlineColor");

        if (BetterDataManager.Files.BetterDataFile.TryGetCheatInfo(_player.Data, out var info))
        {
            if (ssbTag.HasValue)
            {
                ssbTag.Value.Append(info.title.ToColor(info.hexColor));
            }
            _player.SetOutlineByHex(true, info.hexColor);
        }
        else if (_cachedColors.Any(kvp => color == kvp.Value))
        {
            _player.SetOutline(false, null);
        }
    }

    /// <summary>
    /// Sets lobby specific information.
    /// </summary>
    [HideFromIl2Cpp]
    private void SetLobbyInfo(ref string newName, ExtendedPlayerInfo betterData, SplitStringBuilder ssbTag)
    {
        if (betterData == null)
            return;

        if (_player.IsHost() && BAUConfigs.LobbyPlayerInfo.Value)
            newName = _player.GetPlayerNameAndColor();

        if ((_player.IsLocalPlayer() || betterData.IsBetterUser) && !GameState.IsInGamePlay)
        {
            string verificationSymbol = betterData.IsVerifiedBetterUser || _player.IsLocalPlayer() ? "✓ " : "";

            ssbTag.AppendFormat("<color=#0dff00>{1}{0}</color>",
                TranslationStrings.Player_BetterUser.LocalizedString, verificationSymbol);
        }
        ssbTag.Append($"<color=#b554ff>ID: {_player.PlayerId}</color>");
    }

    /// <summary>
    /// Sets in-game specific information.
    /// </summary>
    /// <param name="ssbTagTop">StringBuilder for top tag text.</param>
    [HideFromIl2Cpp]
    private void SetInGameInfo(SplitStringBuilder ssbTagTop)
    {
        ssbTagTop.Append(_player.GetRoleInfo(true));
    }

    /// <summary>
    /// Updates player highlight/outline.
    /// </summary>
    private void UpdatePlayerHighlight()
    {
        SetPlayerOutline(null);
    }

    /// <summary>
    /// Updates color blind text position.
    /// </summary>
    private void UpdateColorBlindTextPosition()
    {
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_CustomColorBlindText))
            return;

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