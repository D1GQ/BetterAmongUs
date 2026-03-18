using AmongUs.GameOptions;
using BetterAmongUs.Data;
using BetterAmongUs.Helpers;
using BetterAmongUs.Modules;
using BetterAmongUs.Structs;
using Il2CppInterop.Runtime.Attributes;
using System.Text;
using TMPro;
using UnityEngine;

namespace BetterAmongUs.Mono;

/// <summary>
/// Displays extended player information during meetings.
/// </summary>
internal sealed class MeetingInfoDisplay : PlayerInfoDisplay
{
    private PlayerVoteArea? _pva;
    private Vector3 _namePos;
    private Vector3 _infoPos;
    private Vector3 _TopPos;

    private readonly StringBuilder _sbTag = new(256);
    private readonly StringBuilder _sbInfo = new(256);
    private string _lastInfoText = "", _lastTopText = "";
    private int _lastUpdateFrame;
    private const int UPDATE_COOLDOWN = 5;

    private CachedTranslations _cachedTranslations = new();

    /// <summary>
    /// Cached translations for performance optimization.
    /// </summary>
    private class CachedTranslations
    {
        internal readonly string SickoUser = Translator.GetString("Player.SickoUser");
        internal readonly string AUMUser = Translator.GetString("Player.AUMUser");
        internal readonly string KNUser = Translator.GetString("Player.KNUser");
        internal readonly string KnownCheater = Translator.GetString("Player.KnownCheater");
        internal readonly string DisconnectLeft = Translator.GetString("DisconnectReasonMeeting.Left");
        internal readonly string DisconnectAntiCheat = Translator.GetString("DisconnectReasonMeeting.AntiCheat");
        internal readonly string DisconnectBanned = Translator.GetString("DisconnectReasonMeeting.Banned");
        internal readonly string DisconnectKicked = Translator.GetString("DisconnectReasonMeeting.Kicked");
        internal readonly string DisconnectCheater = Translator.GetString("DisconnectReasonMeeting.Cheater");
        internal readonly string DisconnectDefault = Translator.GetString("DisconnectReasonMeeting.Disconnect");
    }

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

        _sbTag.Clear();
        _sbInfo.Clear();

        if (_player != null)
        {
            UpdateInfo();
        }
        else
        {
            UpdateDisconnect();
        }

        UpdateTextPositions();
        _pva.ColorBlindName.transform.localPosition = new Vector3(-0.91f, -0.19f, -0.05f);

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
        if (_player == null || _player.Data == null || _player.BetterData() == null)
            return;

        SetPlayerTags(_sbTag);
        FormatPlayerInfo(_sbTag, _sbInfo);

        string roleText = GetRoleText();
        UpdateNameTextPosition(roleText, _sbInfo.ToString());

        UpdateTextIfChanged(_infoText, _sbInfo.ToString(), ref _lastInfoText);
        UpdateTextIfChanged(_topText, roleText, ref _lastTopText);
    }

    /// <summary>
    /// Sets player tags based on data from BetterDataManager.
    /// </summary>
    /// <param name="sbTag">StringBuilder for tag text.</param>
    [HideFromIl2Cpp]
    private void SetPlayerTags(StringBuilder sbTag)
    {
        if (_player == null)
            return;

        if (_player.Data == null)
            return;

        if (ContainsPlayerData(BetterDataManager.BetterDataFile.SickoData, _player.Data))
            sbTag.Append($"<color=#00f583>{_cachedTranslations.SickoUser}</color>+++");
        else if (ContainsPlayerData(BetterDataManager.BetterDataFile.AUMData, _player.Data))
            sbTag.Append($"<color=#4f0000>{_cachedTranslations.AUMUser}</color>+++");
        else if (ContainsPlayerData(BetterDataManager.BetterDataFile.KNData, _player.Data))
            sbTag.Append($"<color=#8731e7>{_cachedTranslations.KNUser}</color>+++");
        else if (ContainsPlayerData(BetterDataManager.BetterDataFile.CheatData, _player.Data))
            sbTag.Append($"<color=#fc0000>{_cachedTranslations.KnownCheater}</color>+++");
    }

    /// <summary>
    /// Formats player information tags into a readable string.
    /// </summary>
    /// <param name="sbTag">StringBuilder containing tags.</param>
    /// <param name="sbInfo">StringBuilder for formatted info.</param>
    [HideFromIl2Cpp]
    private static void FormatPlayerInfo(StringBuilder sbTag, StringBuilder sbInfo)
    {
        if (sbTag.Length == 0)
            return;

        string tagString = sbTag.ToString();
        string[] tags = tagString.Split(["+++"], System.StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < tags.Length; i++)
        {
            if (!string.IsNullOrEmpty(tags[i]))
            {
                sbInfo.Append(tags[i]);
                if (i < tags.Length - 1)
                {
                    sbInfo.Append(" - ");
                }
            }
        }
    }

    /// <summary>
    /// Gets the role text for display.
    /// </summary>
    /// <returns>Formatted role text.</returns>
    private string GetRoleText()
    {
        if (_player == null) return string.Empty;

        string roleHexColor = _player.IsImpostorTeam() ? "#ff1919" : "#8cffff";
        string role = $"<color={roleHexColor}>{_player.GetRoleName()}</color>";

        if (!_player.IsImpostorTeam() && _player.myTasks.Count > 0)
        {
            int completedTasks = 0;
            foreach (var task in _player.myTasks)
            {
                if (task.IsComplete) completedTasks++;
            }
            role += $" <color=#cbcbcb>({completedTasks}/{_player.myTasks.Count})</color>";
        }

        if (!_player.IsImpostorTeammate())
        {
            if ((PlayerControl.LocalPlayer.IsAlive() || PlayerControl.LocalPlayer.Is(RoleTypes.GuardianAngel)) && !_player.IsLocalPlayer())
            {
                return string.Empty;
            }
        }

        return role;
    }

    /// <summary>
    /// Updates name text position based on role and info text presence.
    /// </summary>
    /// <param name="roleText">The role text.</param>
    /// <param name="infoText">The info text.</param>
    private void UpdateNameTextPosition(string roleText, string infoText)
    {
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
        if (textMesh == null)
            return;

        if (newText != lastValue)
        {
            textMesh.SetText(newText);
            lastValue = newText;
        }
    }

    /// <summary>
    /// Checks if player data exists in a HashSet of UserInfo.
    /// </summary>
    /// <param name="dataList">HashSet of UserInfo to check.</param>
    /// <param name="playerData">Player data to look for.</param>
    /// <returns>True if player data exists in the HashSet.</returns>
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

        var betterData = playerData.BetterData();
        if (betterData == null)
            return string.Empty;

        switch (betterData.DisconnectReason)
        {
            case DisconnectReasons.ExitGame:
                return _cachedTranslations.DisconnectLeft;

            case DisconnectReasons.Banned:
                if (betterData.AntiCheatInfo != null && betterData.AntiCheatInfo.BannedByAntiCheat)
                    return _cachedTranslations.DisconnectAntiCheat;
                else
                    return _cachedTranslations.DisconnectBanned;

            case DisconnectReasons.Kicked:
                return _cachedTranslations.DisconnectKicked;

            case DisconnectReasons.Hacking:
                return _cachedTranslations.DisconnectCheater;

            default:
                return _cachedTranslations.DisconnectDefault;
        }
    }
}