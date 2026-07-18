using AmongUs.GameOptions;
using BetterAmongUs.Utilities;
using BetterAmongUs.Modules;
using HarmonyLib;
using System.Text;
using UnityEngine;

namespace BetterAmongUs.Patches.Managers;

[HarmonyPatch]
internal static class FindGameMoreInfoPopupPatch
{
    private static InfoTextBox? _textBox;
    private static FindGameMoreInfoPopup? _findGameMoreInfoPopup;
    private static readonly StringBuilder _sb = new();

    [HarmonyPatch(typeof(FindGameMoreInfoPopup), nameof(FindGameMoreInfoPopup.SetupInfo))]
    [HarmonyPostfix]
    private static void FindGameMoreInfoPopup_SetupInfo_Postfix(FindGameMoreInfoPopup __instance)
    {
        _findGameMoreInfoPopup = __instance;

        // Create custom settings info text box if it doesn't exist
        if (_textBox == null)
        {
            _textBox = UnityEngine.Object.Instantiate(AccountManager.Instance.genericInfoDisplayBox, __instance.transform);
            _textBox.enabled = false;
            _textBox.name = "SettingsInfo";
            _textBox.gameObject.SetActive(true);
            _textBox.SetOneButton();
            _textBox.button1.gameObject.SetActive(false);
            _textBox.bodyText.fontSizeMin = 1.35f;
            _textBox.bodyText.transform.localPosition = new(0f, 0.923f, 0f);
            _textBox.background.transform.localScale = new Vector3(0.4f, 1f, 1f);
            _textBox.transform.Find("Fill").gameObject.SetActive(false);

            _textBox.transform.GetComponent<TransitionOpen>()?.DestroyMono();
            _textBox.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
            var pos = _textBox.gameObject.AddComponent<AspectPosition>();
            pos.Alignment = AspectPosition.EdgeAlignments.RightBottom;
            pos.DistanceFromEdge = new Vector3(0.9f, 1.3f, -10f);
            pos.AdjustPosition();
        }

        // Display all game settings when popup opens
        ShowAll();
    }

    private static void ShowAll()
    {
        if (_findGameMoreInfoPopup == null)
            return;

        _sb.Clear();
        var options = _findGameMoreInfoPopup.gameListing.Options;

        // Display settings based on game mode
        if (options.GameMode is GameModes.Normal or GameModes.NormalFools)
        {
            // Impostor settings
            FormatOption(StringNames.GameNumImpostors, $"{options.NumImpostors}");
            if (options.TryGetFloat(FloatOptionNames.KillCooldown, out var value1))
            {
                FormatOption(StringNames.GameKillCooldown, $"{value1}s");
            }
            if (options.TryGetFloat(FloatOptionNames.ImpostorLightMod, out var value2))
            {
                FormatOption(StringNames.GameImpostorLight, $"{value2}x");
            }
            if (options.TryGetInt(Int32OptionNames.KillDistance, out var value3))
            {
                FormatOption(StringNames.GameKillDistance, GetKillDistance(value3));
            }

            AddBreak();

            // General gameplay settings
            if (options.TryGetFloat(FloatOptionNames.PlayerSpeedMod, out var value4))
            {
                FormatOption(StringNames.GamePlayerSpeed, $"{value4}x");
            }
            if (options.TryGetFloat(FloatOptionNames.CrewLightMod, out var value5))
            {
                FormatOption(StringNames.GameCrewLight, $"{value5}x");
            }

            AddBreak();

            // Meeting settings
            if (options.TryGetInt(Int32OptionNames.NumEmergencyMeetings, out var value6))
            {
                FormatOption(StringNames.GameNumMeetings, $"{value6}");
            }
            if (options.TryGetInt(Int32OptionNames.EmergencyCooldown, out var value7))
            {
                FormatOption(StringNames.GameEmergencyCooldown, $"{value7}s");
            }
            if (options.TryGetInt(Int32OptionNames.DiscussionTime, out var value8))
            {
                FormatOption(StringNames.GameDiscussTime, $"{value8}s");
            }
            if (options.TryGetInt(Int32OptionNames.VotingTime, out var value9))
            {
                FormatOption(StringNames.GameVotingTime, $"{value9}s");
            }
            if (options.TryGetBool(BoolOptionNames.AnonymousVotes, out var value10))
            {
                FormatOption(StringNames.GameAnonymousVotes, FormatBool(value10));
            }
            if (options.TryGetBool(BoolOptionNames.ConfirmImpostor, out var value11))
            {
                FormatOption(StringNames.GameConfirmImpostor, FormatBool(value11));
            }

            AddBreak();

            // Task settings
            if (options.TryGetInt(Int32OptionNames.TaskBarMode, out var value12))
            {
                FormatOption(StringNames.GameTaskBarMode, GetTaskBarMode(value12));
            }
            if (options.TryGetInt(Int32OptionNames.NumCommonTasks, out var value13))
            {
                FormatOption(StringNames.GameCommonTasks, $"{value13}");
            }
            if (options.TryGetInt(Int32OptionNames.NumLongTasks, out var value14))
            {
                FormatOption(StringNames.GameLongTasks, $"{value14}");
            }
            if (options.TryGetInt(Int32OptionNames.NumShortTasks, out var value15))
            {
                FormatOption(StringNames.GameShortTasks, $"{value15}");
            }
            if (options.TryGetBool(BoolOptionNames.VisualTasks, out var value16))
            {
                FormatOption(StringNames.GameVisualTasks, FormatBool(value16));
            }
        }
        else if (options.GameMode is GameModes.HideNSeek or GameModes.SeekFools)
        {
            // Hide & Seek specific settings (Hider/Crewmate)
            if (options.TryGetFloat(FloatOptionNames.PlayerSpeedMod, out var value1))
            {
                FormatOption(StringNames.GamePlayerSpeed, $"{value1}x");
            }
            if (options.TryGetFloat(FloatOptionNames.EscapeTime, out var value2))
            {
                FormatOption(StringNames.EscapeTime, $"{value2}s");
            }
            if (options.TryGetFloat(FloatOptionNames.CrewLightMod, out var value3))
            {
                FormatOption(StringNames.GameCrewLight, $"{value3}x");
            }
            if (options.TryGetInt(Int32OptionNames.CrewmateVentUses, out var value4))
            {
                FormatOption(StringNames.MaxVentUses, $"{value4}");
            }
            if (options.TryGetBool(BoolOptionNames.UseFlashlight, out var value5))
            {
                FormatOption(StringNames.UseFlashlight, FormatBool(value5));
            }
            if (options.TryGetFloat(FloatOptionNames.CrewmateTimeInVent, out var value6))
            {
                FormatOption(StringNames.MaxTimeInVent, $"{value6}s");
            }
            if (options.TryGetFloat(FloatOptionNames.CrewmateFlashlightSize, out var value7))
            {
                FormatOption(StringNames.CrewmateFlashlightSize, $"{value7}x");
            }
            if (options.TryGetBool(BoolOptionNames.ShowCrewmateNames, out var value8))
            {
                FormatOption(StringNames.ShowCrewmateNames, FormatBool(value8));
            }

            AddBreak();

            // Hide & Seek Seeker (Impostor) settings
            if (options.TryGetFloat(FloatOptionNames.ImpostorFlashlightSize, out var value9))
            {
                FormatOption(StringNames.ImpostorFlashlightSize, $"{value9}x");
            }
            if (options.TryGetFloat(FloatOptionNames.ImpostorLightMod, out var value10))
            {
                FormatOption(StringNames.GameImpostorLight, $"{value10}x");
            }

            AddBreak();

            // Hide & Seek final phase settings
            if (options.TryGetFloat(FloatOptionNames.FinalEscapeTime, out var value11))
            {
                FormatOption(StringNames.FinalEscapeTime, $"{value11}s");
            }
            if (options.TryGetBool(BoolOptionNames.SeekerPings, out var value12))
            {
                FormatOption(StringNames.SeekerPings, FormatBool(value12));
            }
            if (options.TryGetFloat(FloatOptionNames.SeekerFinalSpeed, out var value13))
            {
                FormatOption(StringNames.SeekerFinalSpeed, $"{value13}x");
            }
            if (options.TryGetFloat(FloatOptionNames.MaxPingTime, out var value14))
            {
                FormatOption(StringNames.MaxPingTime, $"{value14}s");
            }
            if (options.TryGetBool(BoolOptionNames.SeekerFinalMap, out var value15))
            {
                FormatOption(StringNames.SeekerFinalMap, FormatBool(value15));
            }

            AddBreak();

            // Hide & Seek task settings
            if (options.TryGetInt(Int32OptionNames.NumCommonTasks, out var value16))
            {
                FormatOption(StringNames.GameCommonTasks, $"{value16}");
            }
            if (options.TryGetInt(Int32OptionNames.NumLongTasks, out var value17))
            {
                FormatOption(StringNames.GameLongTasks, $"{value17}");
            }
            if (options.TryGetInt(Int32OptionNames.NumShortTasks, out var value18))
            {
                FormatOption(StringNames.GameShortTasks, $"{value18}");
            }
        }

        // Update text box with formatted settings
        if (_textBox != null)
        {
            _textBox.SetText(_sb.ToString());
        }
    }

    private static void FormatOption(StringNames optName, string value)
    {
        // Format each setting line with gray label and yellow value
        _sb.AppendLine($"{Translator.GetString(optName)}<#989898>:</color> <#CFCF00>{value}</color>");
    }

    private static void AddBreak()
    {
        // Add separator line between settings groups
        _sb.AppendLine("<#999999>----------------------</color>");
    }

    private static string FormatBool(bool @bool) => @bool ? "<#19FF00>On</color>" : "<#FF000A>Off</color>";

    private static string GetKillDistance(int value)
    {
        // Convert kill distance integer to readable string
        switch (value)
        {
            case 0:
                return Translator.GetString(StringNames.SettingShort);
            case 1:
                return Translator.GetString(StringNames.SettingMedium);
            case 2:
                return Translator.GetString(StringNames.SettingLong);
            default:
                break;
        }

        return string.Empty;
    }

    private static string GetTaskBarMode(int value)
    {
        // Convert task bar mode integer to readable string
        switch (value)
        {
            case 0:
                return Translator.GetString(StringNames.SettingNormalTaskMode);
            case 1:
                return Translator.GetString(StringNames.SettingMeetingTaskMode);
            case 2:
                return Translator.GetString(StringNames.SettingInvisibleTaskMode);
            default:
                break;
        }

        return string.Empty;
    }
}