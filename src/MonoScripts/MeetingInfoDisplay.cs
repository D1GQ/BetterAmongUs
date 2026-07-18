using BetterAmongUs.Attributes;
using BetterAmongUs.Data;
using BetterAmongUs.Generated;
using BetterAmongUs.Modules.Support;
using BetterAmongUs.MonoScripts.Extended;
using BetterAmongUs.Structs;
using BetterAmongUs.Utilities;
using Il2CppInterop.Runtime.Attributes;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.MonoScripts;

/// <summary>
/// Displays extended player information during meetings.
/// </summary>
[RegisterInIl2Cpp]
internal sealed class MeetingInfoDisplay : PlayerInfoDisplay
{
    private PlayerVoteArea? _pva;
    private Vector3 _namePos;
    private Vector3 _infoPos;
    private Vector3 _TopPos;

    private readonly SplitStringBuilder _ssbTag = new(100, '-');
    private readonly SplitStringBuilder _ssbInfo = new(100, '-');
    private string _lastInfoText = "", _lastTopText = "";
    private int _lastUpdateFrame;
    private const int UPDATE_COOLDOWN = 5;

    /// <summary>
    /// Initializes the meeting info display.
    /// </summary>
    /// <param name="player">The player to display info for.</param>
    /// <param name="pva">The PlayerVoteArea associated with the player.</param>
    internal void Init(PlayerControl? player, PlayerVoteArea pva)
    {
        _player = player;
        _pva = pva;

        _nameText = pva.NameText;
        _infoText = InstantiatePlayerInfoText("InfoText_Info_TMP", new Vector3(0f, 0.28f), pva.transform);
        _topText = InstantiatePlayerInfoText("InfoText_T_TMP", new Vector3(0f, 0.15f), pva.transform);
        _infoText.fontSize = 1.3f;
        _topText.fontSize = 1.3f;
        _namePos = _nameText.transform.localPosition - new Vector3(0f, 0.02f, 0f);
        _infoPos = _infoText.transform.localPosition;
        _TopPos = _topText.transform.localPosition;

        var PlayerLevel = pva.transform.Find("PlayerLevel");
        PlayerLevel.localPosition = new Vector3(PlayerLevel.localPosition.x, PlayerLevel.localPosition.y, -2f);
        var LevelDisplay = Instantiate(PlayerLevel, pva.transform);
        LevelDisplay.transform.SetSiblingIndex(pva.transform.Find("PlayerLevel").GetSiblingIndex() + 1);
        LevelDisplay.gameObject.name = "PlayerId";
        LevelDisplay.GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 1f, 1f);
        var IdLabel = LevelDisplay.transform.Find("LevelLabel");
        var IdNumber = LevelDisplay.transform.Find("LevelNumber");
        IdLabel.gameObject.DestroyTextTranslators();
        IdLabel.GetComponent<TextMeshPro>().text = "ID";
        IdNumber.GetComponent<TextMeshPro>().text = pva.TargetPlayerId.ToString();
        IdLabel.name = "IdLabel";
        IdNumber.name = "IdNumber";
        PlayerLevel.transform.position += new Vector3(0.23f, 0f);
    }

    /// <summary>
    /// LateUpdate override with cooldown for performance optimization.
    /// </summary>
    protected override void LateUpdate()
    {
        if (Time.frameCount - _lastUpdateFrame < UPDATE_COOLDOWN)
            return;

        if (_pva == null)
            return;

        _ssbTag.Clear();
        _ssbInfo.Clear();

        if (_player != null)
        {
            UpdateInfo();
        }
        else
        {
            UpdateDisconnect();
        }

        UpdateTextPositions();

        if (!BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_CustomColorBlindText))
        {
            _pva.ColorBlindName.transform.localPosition = new Vector3(-0.91f, -0.19f, -0.05f);
        }

        _lastUpdateFrame = Time.frameCount;
    }

    /// <summary>
    /// Updates the text positions based on content presence.
    /// </summary>
    private void UpdateTextPositions()
    {
        if (_nameText == null)
            return;

        if (_infoText == null)
            return;

        if (_topText == null)
            return;

        bool hasInfoText = !string.IsNullOrEmpty(_infoText.text);
        bool hasTopText = !string.IsNullOrEmpty(_topText.text);

        if (hasInfoText && hasTopText)
        {
            _nameText.transform.localPosition = _namePos + new Vector3(0f, -0.1f, 0f);
            _infoText.transform.localPosition = _infoPos + new Vector3(0f, -0.1f, 0f);
            _topText.transform.localPosition = _TopPos + new Vector3(0f, -0.1f, 0f);
        }
        else if (hasInfoText || hasTopText)
        {
            _nameText.transform.localPosition = _namePos;
            _infoText.transform.localPosition = _TopPos;
            _topText.transform.localPosition = _TopPos;
        }
        else
        {
            _nameText.transform.localPosition = _namePos;
            _infoText.transform.localPosition = _infoPos;
            _topText.transform.localPosition = _TopPos;
        }
    }

    /// <summary>
    /// Updates player information display.
    /// </summary>
    private void UpdateInfo()
    {
        if (_player == null || _player.Data == null || _player.ExtendedData() == null)
            return;

        SetPlayerTags(_ssbTag);
        _ssbInfo.Append(_player.GetRoleInfo(true));

        UpdateNameTextPosition(_ssbInfo.ToString(), _ssbInfo.ToString());

        UpdateTextIfChanged(_infoText, _ssbInfo.ToString(), ref _lastInfoText);
        UpdateTextIfChanged(_topText, _ssbTag.ToString(), ref _lastTopText);
    }

    /// <summary>
    /// Sets player tags based on data from BetterDataManager.
    /// </summary>
    /// <param name="ssbTag">StringBuilder for tag text.</param>
    [HideFromIl2Cpp]
    private void SetPlayerTags(SplitStringBuilder ssbTag)
    {
        if (_player == null)
            return;

        if (_player.Data == null)
            return;

        if (BetterDataManager.Files.BetterDataFile.TryGetCheatInfo(_player.Data, out var info))
        {
            ssbTag.Append(info.title.ToColor(info.hexColor));
        }
    }

    /// <summary>
    /// Updates name text position based on role and info text presence.
    /// </summary>
    /// <param name="roleText">The role text.</param>
    /// <param name="infoText">The info text.</param>
    private void UpdateNameTextPosition(string roleText, string infoText)
    {
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_PlayerMeetingInfo))
            return;

        bool hasRole = !string.IsNullOrEmpty(roleText);
        bool hasInfo = !string.IsNullOrEmpty(infoText);

        Vector3 textPos;
        if (hasRole && hasInfo)
            textPos = new Vector3(_pva.NameText.transform.localPosition.x, -0.045f);
        else
            textPos = new Vector3(_pva.NameText.transform.localPosition.x, 0.015f);

        _pva.NameText.transform.localPosition = textPos;
    }

    /// <summary>
    /// Updates text if changed, optimizing performance.
    /// </summary>
    /// <param name="textMesh">TextMeshPro component to update.</param>
    /// <param name="newText">New text to set.</param>
    /// <param name="lastValue">Reference to last value for comparison.</param>
    private static void UpdateTextIfChanged(TextMeshPro textMesh, string newText, ref string lastValue)
    {
        if (BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_PlayerMeetingInfo))
        {
            textMesh.SetText(string.Empty);
            return;
        }

        if (textMesh == null)
            return;

        if (newText != lastValue)
        {
            textMesh.SetText(newText);
            lastValue = newText;
        }
    }

    /// <summary>
    /// Updates display for disconnected players.
    /// </summary>
    private void UpdateDisconnect()
    {
        string disconnectText = GetDisconnectText();

        if (disconnectText != _lastInfoText)
        {
            if (_infoText != null)
            {
                _infoText.SetText($"<color=#6b6b6b>{disconnectText}</color>");
                _lastInfoText = disconnectText;
            }
        }

        if (_lastTopText != string.Empty)
        {
            if (_topText != null)
            {
                _topText.SetText("");
                _lastTopText = string.Empty;
            }
        }

        var votePlayerBase = _pva.transform.Find("votePlayerBase");
        if (votePlayerBase != null)
        {
            votePlayerBase.gameObject.SetActive(false);
        }
        var deadXBorder = _pva.transform.Find("deadX_border");
        if (deadXBorder != null)
        {
            deadXBorder.gameObject.SetActive(false);
        }
        _pva.ClearForResults();
        _pva.SetDisabled();
    }

    /// <summary>
    /// Gets disconnect reason text for display.
    /// </summary>
    /// <returns>Disconnect reason text.</returns>
    private string GetDisconnectText()
    {
        var playerData = GameData.Instance.GetPlayerById(_pva.TargetPlayerId);
        if (playerData == null)
            return string.Empty;

        var betterData = playerData.ExtendedData();
        if (betterData == null)
            return string.Empty;

        switch (betterData.DisconnectReason)
        {
            case DisconnectReasons.ExitGame:
                return TranslationStrings.DisconnectReasonMeeting_Left.LocalizedString;

            case DisconnectReasons.Banned:
                if (betterData.AntiCheatInfo != null && betterData.AntiCheatInfo.BannedByAntiCheat)
                    return TranslationStrings.DisconnectReasonMeeting_AntiCheat.LocalizedString;
                else
                    return TranslationStrings.DisconnectReasonMeeting_Banned.LocalizedString;

            case DisconnectReasons.Kicked:
                return TranslationStrings.DisconnectReasonMeeting_Kicked.LocalizedString;

            case DisconnectReasons.Hacking:
                return TranslationStrings.DisconnectReasonMeeting_Cheater.LocalizedString;

            default:
                return TranslationStrings.DisconnectReasonMeeting_Left.LocalizedString;
        }
    }
}