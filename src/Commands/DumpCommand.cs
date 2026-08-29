using BepInEx;
using BetterAmongUs.Attributes;
using BetterAmongUs.Data;
using BetterAmongUs.Modules;

namespace BetterAmongUs.Commands;

[RegisterCommand]
internal sealed class DumpCommand : BaseCommand
{
    internal override string Name => "dump";
    internal override string Description => "Dump the entire log to the user's desktop";

    internal override bool CanRunCommand(out string reason)
    {
        if (GameState.IsInGamePlay)
        {
            reason = "Only can run in lobby";
            return false;
        }

        return base.CanRunCommand(out reason);
    }

    internal override void Run()
    {
        string bepInExLog = Path.Combine(Paths.BepInExRootPath, "LogOutput.log");
        if (!File.Exists(bepInExLog))
        {
            CommandErrorText("BepInEx log file not found!");
            return;
        }

        if (!BAUPlugin.ModInfo.Starlight)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string logFolderPath = Path.Combine(desktopPath, "BAULogDumps");

            if (!Directory.Exists(logFolderPath))
            {
                Directory.CreateDirectory(logFolderPath);
            }

            string log;
            using (FileStream fileStream = new(bepInExLog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new(fileStream))
            {
                log = reader.ReadToEnd();
            }

            string decryptedLog = BAULogger.DecryptLogs(log);

            string logFileName = "log-" + BAUPlugin.ModInfo.VERSION_STRING + "-" + DateTime.Now.ToString("yyyy.MM.dd-HH.mm.ss") + "-bepinex" + ".log";
            string logFilePath_New = Path.Combine(logFolderPath, logFileName);
            File.WriteAllText(logFilePath_New, decryptedLog);

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = logFolderPath,
                UseShellExecute = true,
                Verb = "open"
            });

            CommandResultText($"Dump logs at <color=#b1b1b1>'{logFolderPath}'</color>");
        }
        else
        {
            string dataPath = BetterDataManager.Folders.fileFolderPath;
            string logFolderPath = Path.Combine(dataPath, "BAULogDumps");
            if (!Directory.Exists(logFolderPath))
            {
                Directory.CreateDirectory(logFolderPath);
            }

            string log;
            using (FileStream fileStream = new(bepInExLog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new(fileStream))
            {
                log = reader.ReadToEnd();
            }

            string decryptedLog = BAULogger.DecryptLogs(log);

            string logFileName = "log-" + BAUPlugin.ModInfo.VERSION_STRING + "-" + DateTime.Now.ToString("yyyy.MM.dd-HH.mm.ss") + "-bepinex" + ".log";
            string logFilePath_New = Path.Combine(logFolderPath, logFileName);
            File.WriteAllText(logFilePath_New, decryptedLog);

            CommandResultText($"Dump logs at <color=#b1b1b1>'{logFolderPath}'</color>");
        }
    }
}