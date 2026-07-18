using BepInEx;
using BepInEx.Logging;
using BetterAmongUs.Data;
using BetterAmongUs.Utilities;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BetterAmongUs.Modules;

/// <summary>
/// Provides logging utilities for BetterAmongUs with various log levels and destinations.
/// </summary>
internal static class Logger_
{
    /// <summary>
    /// Logs a message with specified parameters.
    /// </summary>
    /// <param name="info">The message to log.</param>
    /// <param name="tag">The log tag/category.</param>
    /// <param name="logConsole">Whether to output to console.</param>
    /// <param name="color">The console color for the message.</param>
    /// <param name="hostOnly">Whether to log only when the client is host.</param>
    internal static void Log(string info, string tag = "Log", bool logConsole = true, ConsoleColor color = ConsoleColor.White, bool hostOnly = false)
    {
        try
        {
            if (hostOnly && !GameState.IsHost)
                return;

            string mark = $"{DateTime.Now:HH:mm} [BetterLog][{tag}]";
            string logFilePath = BetterDataManager.Files.logFilePath;
            string newLine = $"{mark}: {Utils.RemoveHtmlText(info)}";
            File.AppendAllText(logFilePath, newLine + Environment.NewLine);
            BAUPlugin.Logger.LogInfo($"[{tag}] {info}");
            if (logConsole)
            {
                ConsoleManager.SetConsoleColor(color);
                ConsoleManager.ConsoleStream.WriteLine($"{DateTime.Now:HH:mm} BetterAmongUs[{tag}]: {Utils.RemoveHtmlText(info)}");
            }
        }
        catch { }
    }

    /// <summary>
    /// Logs method calls with detailed information about the caller.
    /// </summary>
    /// <param name="info">Additional information about the method call.</param>
    /// <param name="runtimeType">The type containing the method (optional).</param>
    /// <param name="hostOnly">Whether to log only when the client is host.</param>
    /// <param name="callerFilePath">Automatically populated: path of the file containing the caller.</param>
    /// <param name="callerLineNumber">Automatically populated: line number of the caller.</param>
    /// <param name="callerMemberName">Automatically populated: name of the calling member.</param>
    internal static void LogMethod(
        string info = "",
        Type? runtimeType = null,
        bool hostOnly = false,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerMemberName] string callerMemberName = "")
    {
        var loggedMethodFrame = new StackFrame(1, true);

        var loggedMethod = loggedMethodFrame.GetMethod();
        string loggedMethodName = loggedMethod.Name;
        string? loggedClassFullName = runtimeType?.FullName ?? loggedMethod.DeclaringType?.FullName;
        string? loggedClassName = runtimeType?.Name ?? loggedMethod.DeclaringType?.Name;

        string logMessage = string.IsNullOrEmpty(info)
            ? $"{loggedClassFullName}.{loggedMethodName} was called from {Path.GetFileName(callerFilePath)}({callerLineNumber}) in {callerMemberName}."
            : $"{loggedClassFullName}.{loggedMethodName} was called from {Path.GetFileName(callerFilePath)}({callerLineNumber}) in {callerMemberName}. Info: {info}.";

        Log(logMessage, "MethodLog", hostOnly);
    }

    /// <summary>
    /// Logs private method calls with detailed information, using encrypted storage.
    /// </summary>
    /// <param name="info">Additional information about the method call.</param>
    /// <param name="runtimeType">The type containing the method (optional).</param>
    /// <param name="hostOnly">Whether to log only when the client is host.</param>
    /// <param name="callerFilePath">Automatically populated: path of the file containing the caller.</param>
    /// <param name="callerLineNumber">Automatically populated: line number of the caller.</param>
    /// <param name="callerMemberName">Automatically populated: name of the calling member.</param>
    internal static void LogMethodPrivate(
    string info = "",
    Type? runtimeType = null,
    bool hostOnly = false,
    [CallerFilePath] string callerFilePath = "",
    [CallerLineNumber] int callerLineNumber = 0,
    [CallerMemberName] string callerMemberName = "")
    {
        var loggedMethodFrame = new StackFrame(1, true);

        var loggedMethod = loggedMethodFrame.GetMethod();
        string loggedMethodName = loggedMethod.Name;
        string? loggedClassFullName = runtimeType?.FullName ?? loggedMethod.DeclaringType?.FullName;
        string? loggedClassName = runtimeType?.Name ?? loggedMethod.DeclaringType?.Name;

        string logMessage = string.IsNullOrEmpty(info)
            ? $"{loggedClassFullName}.{loggedMethodName} was called from {Path.GetFileName(callerFilePath)}({callerLineNumber}) in {callerMemberName}."
            : $"{loggedClassFullName}.{loggedMethodName} was called from {Path.GetFileName(callerFilePath)}({callerLineNumber}) in {callerMemberName}. Info: {info}.";

        LogPrivate(logMessage, "MethodLog", hostOnly);
    }

    /// <summary>
    /// Logs a header message with visual formatting.
    /// </summary>
    /// <param name="info">The header text.</param>
    /// <param name="tag">The log tag/category.</param>
    /// <param name="hostOnly">Whether to log only when the client is host.</param>
    /// <param name="logConsole">Whether to output to console.</param>
    internal static void LogHeader(string info, string tag = "LogHeader", bool hostOnly = false, bool logConsole = true) => Log($"   >-------------- {info} --------------<", tag, hostOnly: hostOnly, logConsole: logConsole);

    /// <summary>
    /// Logs cheat detection messages with green console color.
    /// </summary>
    /// <param name="info">The cheat detection message.</param>
    /// <param name="tag">The log tag/category.</param>
    /// <param name="hostOnly">Whether to log only when the client is host.</param>
    /// <param name="logConsole">Whether to output to console.</param>
    internal static void LogCheat(string info, string tag = "AntiCheat", bool hostOnly = false, bool logConsole = true) => Log(info, tag, color: ConsoleColor.Green, hostOnly: hostOnly, logConsole: logConsole);

    /// <summary>
    /// Logs error messages with red console color.
    /// </summary>
    /// <param name="info">The error message.</param>
    /// <param name="tag">The log tag/category.</param>
    /// <param name="hostOnly">Whether to log only when the client is host.</param>
    /// <param name="logConsole">Whether to output to console.</param>
    internal static void Error(string info, string tag = "Error", bool hostOnly = false, bool logConsole = true) => Log(info, tag, color: ConsoleColor.Red, hostOnly: hostOnly, logConsole: logConsole);

    /// <summary>
    /// Logs exception details with red console color.
    /// </summary>
    /// <param name="ex">The exception to log.</param>
    /// <param name="tag">The log tag/category.</param>
    /// <param name="hostOnly">Whether to log only when the client is host.</param>
    /// <param name="logConsole">Whether to output to console.</param>
    internal static void Error(Exception ex, string tag = "Error", bool hostOnly = false, bool logConsole = true) => Log(ex.ToString(), tag, color: ConsoleColor.Red, hostOnly: hostOnly, logConsole: logConsole);

    /// <summary>
    /// Logs warning messages with yellow console color.
    /// </summary>
    /// <param name="info">The warning message.</param>
    /// <param name="tag">The log tag/category.</param>
    /// <param name="hostOnly">Whether to log only when the client is host.</param>
    /// <param name="logConsole">Whether to output to console.</param>
    internal static void Warning(string info, string tag = "Warning", bool hostOnly = false, bool logConsole = true) => Log(info, tag, color: ConsoleColor.Yellow, hostOnly: hostOnly, logConsole: logConsole);

    /// <summary>
    /// Logs a test message for debugging purposes.
    /// </summary>
    internal static void Test()
    {
        Log("------------------> TEST <------------------", "TEST");
        InGame("TEST");
    }

    /// <summary>
    /// Logs a message in-game via the disconnect message notifier.
    /// </summary>
    /// <param name="info">The message to display in-game.</param>
    /// <param name="hostOnly">Whether to log only when the client is host.</param>
    internal static void InGame(string info, bool hostOnly = false)
    {
        if (hostOnly && !GameState.IsHost)
            return;

        if (HudManager.InstanceExists) HudManager.Instance.Notifier.AddDisconnectMessage(info);
        Log(info, "InGame", hostOnly: hostOnly);
    }

    /// <summary>
    /// Logs a private message with encryption for sensitive data.
    /// </summary>
    /// <param name="info">The sensitive message to log.</param>
    /// <param name="tag">The log tag/category.</param>
    /// <param name="hostOnly">Whether to log only when the client is host.</param>
    internal static void LogPrivate(string info, string tag = "Log", bool hostOnly = false)
    {
        try
        {
            if (hostOnly && !GameState.IsHost)
                return;

            string mark = $"{DateTime.Now:HH:mm} [BetterLog][PrivateLog][{tag}]";
            string logFilePath = BetterDataManager.Files.logFilePath;
            string newLine = $"{mark}: " + Encryptor.Encrypt($"{info}");
            File.AppendAllText(logFilePath, newLine + Environment.NewLine);
        }
        catch { }
    }
}

/// <summary>
/// Custom log listener for BepInEx that forwards logs to the BetterAmongUs logging system.
/// </summary>
internal class CustomLogListener : ILogListener
{
    /// <summary>
    /// Gets or sets the log levels to filter.
    /// </summary>
    public LogLevel LogLevelFilter { get; set; } = LogLevel.Info | LogLevel.Warning | LogLevel.Error;

    /// <summary>
    /// Handles log events from BepInEx.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="eventArgs">The log event arguments.</param>
    public void LogEvent(object sender, LogEventArgs eventArgs)
    {
        if (eventArgs.Source.SourceName.ToLower().Contains("unity")
            || eventArgs.Source.SourceName.ToLower().Contains("betteramongus"))
            return;

        if (eventArgs.Level is LogLevel.None or LogLevel.Info)
        {
            Logger_.Log(eventArgs.Data.ToString(), "BepInEx." + eventArgs.Source.SourceName, logConsole: false);
        }
        else if (eventArgs.Level is LogLevel.Warning)
        {
            Logger_.Warning(eventArgs.Data.ToString(), "BepInEx." + eventArgs.Source.SourceName, logConsole: false);
        }
        else if (eventArgs.Level is LogLevel.Error or LogLevel.Fatal)
        {
            Logger_.Error(eventArgs.Data.ToString(), "BepInEx." + eventArgs.Source.SourceName, logConsole: false);
        }
    }

    /// <summary>
    /// Disposes the log listener.
    /// </summary>
    public void Dispose() { }

    /// <summary>
    /// Flushes the log listener.
    /// </summary>
    public void Flush() { }
}