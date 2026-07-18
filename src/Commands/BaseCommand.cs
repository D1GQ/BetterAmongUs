using BetterAmongUs.Attributes;
using BetterAmongUs.Commands.Arguments;
using BetterAmongUs.Modules.Support;
using BetterAmongUs.Utilities;

namespace BetterAmongUs.Commands;

/// <summary>
/// Abstract base class for all commands in BetterAmongUs.
/// </summary>
internal abstract class BaseCommand
{
    /// <summary>
    /// Gets an array of all registered commands.
    /// </summary>
    internal static readonly BaseCommand[] allCommands = [.. RegisterCommandAttribute.Instances];

    /// <summary>
    /// Gets all names for this command (including short names).
    /// </summary>
    internal string[] Names => ShortNames.Concat(new[] { Name }).ToArray();

    /// <summary>
    /// Gets the primary name of the command.
    /// </summary>
    internal abstract string Name { get; }

    /// <summary>
    /// Gets the short names for the command.
    /// </summary>
    internal virtual string[] ShortNames => [];

    /// <summary>
    /// Gets the description of the command.
    /// </summary>
    internal abstract string Description { get; }

    /// <summary>
    /// Gets or sets the arguments for this command.
    /// </summary>
    internal BaseArgument[] Arguments { get; set; } = [];

    /// <summary>
    /// Gets whether this command sets a chat timer.
    /// </summary>
    internal virtual bool SetChatTimer { get; set; } = false;

    /// <summary>
    /// Determines whether this command should be shown in command lists.
    /// </summary>
    /// <returns>True if the command should be shown; otherwise, false.</returns>
    internal virtual bool ShowCommand() => true;

    /// <summary>
    /// Determines whether this command can be executed based on current conditions.
    /// </summary>
    /// <param name="reason">When this method returns, contains the reason why the command cannot be run if it returns false; otherwise, an empty string.</param>
    /// <returns>True if the command can be executed; otherwise, false.</returns>
    internal virtual bool CanRunCommand(out string reason)
    {
        reason = string.Empty;
        return true;
    }

    /// <summary>
    /// Executes the command.
    /// </summary>
    internal abstract void Run();

    /// <summary>
    /// Resets all associated Arguments with Command Back to its default state.
    /// </summary>
    internal void ResetArguments()
    {
        foreach (var arg in Arguments)
        {
            arg.Reset();
        }
    }

    /// <summary>
    /// Formats and optionally displays command result text.
    /// </summary>
    /// <param name="text">The result text to display.</param>
    /// <param name="onlyGetStr">If true, only returns the formatted string without displaying it.</param>
    /// <returns>The formatted result text.</returns>
    internal static string CommandResultText(string text, bool onlyGetStr = false)
    {
        if (!onlyGetStr) Utils.AddChatPrivate(text);
        return text;
    }

    /// <summary>
    /// Formats and optionally displays command error text.
    /// </summary>
    /// <param name="error">The error text to display.</param>
    /// <param name="onlyGetStr">If true, only returns the formatted string without displaying it.</param>
    /// <returns>The formatted error text.</returns>
    internal static string CommandErrorText(string error, bool onlyGetStr = false)
    {
        string er = "<color=#f50000><size=150%><b>Error:</b></size></color>";
        if (!onlyGetStr) Utils.AddChatPrivate($"<color=#730000>{er}\n{error}");
        return $"<color=#730000>{er}\n{error}";
    }

    /// <summary>
    /// Determines whether this command is enabled.
    /// </summary>
    /// <returns>True if the command is enabled; otherwise, false.</returns>
    internal bool IsEnabled()
    {
        return ShowCommand() && !BAUModdedSupportFlags.HasFlag(BAUModdedSupportFlags.Disable_Command + Name);
    }
}