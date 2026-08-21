#pragma warning disable CS0162

using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using BetterAmongUs.Attributes;
using BetterAmongUs.Data;
using BetterAmongUs.Data.Config;
using BetterAmongUs.Data.Json;
using BetterAmongUs.Enums;
using BetterAmongUs.Managers;
using BetterAmongUs.Modules;
using BetterAmongUs.Modules.OptionItems;
using BetterAmongUs.Modules.Support;
using BetterAmongUs.Network;
using BetterAmongUs.Patches.Client;
using BetterAmongUs.Patches.Gameplay.UI.Settings;
using BetterAmongUs.Utilities;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BetterAmongUs;

[BepInPlugin(ModInfo.PLUGIN_GUID, ModInfo.PLUGIN_NAME, ModInfo.PLUGIN_VERSION)]
[BepInProcess(ModInfo.AmongUs.PROCESS_NAME)]
internal partial class BAUPlugin : BasePlugin
{
    /// <summary>
    /// Gets the formatted version text for display.
    /// </summary>
    /// <param name="newLine">Whether to use newline separation for additional info.</param>
    /// <returns>Formatted version string.</returns>
    internal static string GetVersionText(bool newLine = false)
    {
        string text = string.Empty;

        string newLineText = newLine ? "\n" : " ";

        switch (ModInfo.ReleaseBuildType)
        {
            case ReleaseTypes.Release:
                text = $"v{BetterAmongUsVersion}";
                break;
            case ReleaseTypes.Beta:
                text = $"v{BetterAmongUsVersion}{newLineText}Beta {ModInfo.BETA_NUM}";
                break;
            case ReleaseTypes.Dev:
                text = $"v{BetterAmongUsVersion}{newLineText}Dev {ModInfo.CommitHash}-{ModInfo.BuildDate}";
                break;
            default:
                break;
        }

        if (ModInfo.IS_HOTFIX)
            text += $"{newLineText}Hotfix {ModInfo.HOTFIX_NUM}";

        return text;
    }

    /// <summary>
    /// Gets the BAUPlugin instance.
    /// </summary>
    internal static BAUPlugin? Instance { get; private set; }

    /// <summary>
    /// Gets the Harmony instance used for patching.
    /// </summary>
    internal static Harmony Harmony { get; } = new Harmony(ModInfo.PLUGIN_GUID);

    /// <summary>
    /// Gets the BetterAmongUs version string.
    /// </summary>
    internal static string BetterAmongUsVersion => ModInfo.PLUGIN_VERSION;

    /// <summary>
    /// Gets the application version string.
    /// </summary>
    internal static string AppVersion => Application.version;

    /// <summary>
    /// Gets the Among Us version string from reference data.
    /// </summary>
    internal static string AmongUsVersion => ReferenceDataManager.Instance.Refdata.userFacingVersion;

    /// <summary>
    /// Gets platform-specific data.
    /// </summary>
    internal static PlatformSpecificData PlatformData => global::Constants.GetPlatformData();

    /// <summary>
    /// Gets the list of all PlayerControl instances.
    /// </summary>
    internal static List<PlayerControl> AllPlayerControls = [];

    /// <summary>
    /// Gets the list of all alive PlayerControl instances.
    /// </summary>
    internal static List<PlayerControl> AllAlivePlayerControls => [.. AllPlayerControls.Where(pc => pc.IsAlive())];

    /// <summary>
    /// Gets all DeadBody objects in the scene.
    /// </summary>
    internal static DeadBody[] AllDeadBodys => [.. UnityEngine.Object.FindObjectsOfType<DeadBody>()];

    /// <summary>
    /// Gets all Vent objects in the scene.
    /// </summary>
    internal static Vent[] AllVents => UnityEngine.Object.FindObjectsOfType<Vent>();

    /// <summary>
    /// Gets the BepInEx logger instance.
    /// </summary>
    internal static ManualLogSource? Logger;

    public override void Load()
    {
        Instance = this;

        try
        {
            foreach (var listener in BepInEx.Logging.Logger.Listeners)
            {
                if (listener.GetType().Name.ToLower().Contains("Unity"))
                {
                    BepInEx.Logging.Logger.Listeners.Remove(listener);
                    break;
                }
            }

            if (!ModInfo.Starlight)
            {
                SetupConsole();
            }

            RegisterInIl2Cpp.Initialize();
            IL2CPPChainloader.Instance.Finished += OnChainloaderFinished;
        }
        catch (Exception ex)
        {
            Logger_.Error(ex);
        }
    }

    /// <summary>
    /// Runs when the BepInEx Chainloader has finished.
    /// </summary>
    private void OnChainloaderFinished()
    {
        if (BAUModdedSupportEvents.OnBAULoadEvent.InvokeAll(this).Any(b => b == false))
            return;

        BAUModdedSupportFlags.Initialize();
        GithubAPI.Connect();
        BAUConfigs.LoadConfigs();
        BetterDataManager.Initialize();
        AudioOverrideManager.Initialize();
        Translator.Initialize();
        Harmony.PatchAll();
        GameSettingsPatch.SetupSettings(true);
        BAUModdedSupportEvents.OnBAUOptionsLoadedEvent.InvokeAll([.. OptionItem.AllOptions.Cast<object>()]);
        AutoRegisterAttribute.Initialize();
        OutfitData.Initialize();
        SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>)OnSceneLoaded);

        if (File.Exists(BetterDataManager.Files.logFilePath))
            File.WriteAllText(BetterDataManager.Files.previousLogFilePath, File.ReadAllText(BetterDataManager.Files.logFilePath));

        File.WriteAllText(BetterDataManager.Files.logFilePath, "");
        Logger_.Log("Better Among Us successfully loaded!");

        string SupportedVersions = string.Join(" ", ModInfo.SupportedAmongUsVersions);
        Logger_.Log($"BetterAmongUs {BetterAmongUsVersion}-{ModInfo.BuildDate} - [{AppVersion} --> {SupportedVersions}] {Utils.GetPlatformName(PlatformData.Platform)}");
    }

    /// <summary>
    /// Unloads the mod to switch to vanilla.
    /// </summary>
    internal void UnloadBAU()
    {
        ConsoleManager.DetachConsole();
        BetterNotificationManager.Detach();
        ClientPatch.Unpatch();
        Harmony.UnpatchAll();
        ModManager.Instance.ModStamp.gameObject.SetActive(false);
        SceneChanger.ChangeScene("MainMenu");
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode _)
    {
        if (AmongUsClient.Instance != null)
        {
            if (scene.name == AmongUsClient.Instance.MainMenuScene)
            {
                BAUModdedSupportFlags.ClearTempFlags();
            }
        }
    }

    /// <summary>
    /// Sets up the console window for logging.
    /// </summary>
    private static void SetupConsole()
    {
        ConsoleManager.CreateConsole();
        ConsoleManager.ConfigPreventClose.Value = true;
        if (ConsoleManager.ConfigConsoleEnabled.Value) ConsoleManager.DetachConsole();
        ConsoleManager.ConfigConsoleEnabled.Value = false;
        ConsoleManager.SetConsoleTitle("Among Us - BAU Console");
        Logger = BepInEx.Logging.Logger.CreateLogSource(ModInfo.PLUGIN_GUID);
        var customLogListener = new CustomLogListener();
        BepInEx.Logging.Logger.Listeners.Add(customLogListener);
        ConsoleManager.SetConsoleColor(ConsoleColor.Green);
        ConsoleManager.ConsoleStream.WriteLine($".--------------------------------------------------------------------------------.\r\n|  ____       _   _                 _                                  _   _     |\r\n| | __ )  ___| |_| |_ ___ _ __     / \\   _ __ ___   ___  _ __   __ _  | | | |___ |\r\n| |  _ \\ / _ \\ __| __/ _ \\ '__|   / _ \\ | '_ ` _ \\ / _ \\| '_ \\ / _` | | | | / __||\r\n| | |_) |  __/ |_| ||  __/ |     / ___ \\| | | | | | (_) | | | | (_| | | |_| \\__ \\|\r\n| |____/ \\___|\\__|\\__\\___|_|    /_/   \\_\\_| |_| |_|\\___/|_| |_|\\__, |  \\___/|___/|\r\n|                                                              |___/             |\r\n'--------------------------------------------------------------------------------'");
    }
}