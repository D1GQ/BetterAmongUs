namespace BetterAmongUs.Commands.Arguments;

/// <summary>
/// Abstract base class for command arguments.
/// </summary>
/// <param name="command">The command this argument belongs to.</param>
/// <param name="argInfo">Information about the argument, such as its name, description, or format.</param>
internal abstract class BaseArgument(BaseCommand command, string argInfo)
{
    /// <summary>
    /// Gets the command this argument belongs to.
    /// </summary>
    /// <value>The parent <see cref="BaseCommand"/> instance that owns this argument.</value>
    internal BaseCommand Command { get; } = command;

    /// <summary>
    /// Gets information about the argument.
    /// </summary>
    /// <value>A string containing metadata about the argument, such as its name, description, or expected format.</value>
    internal string ArgInfo { get; } = argInfo;

    /// <summary>
    /// Gets or sets the current argument value as a string.
    /// </summary>
    /// <value>The raw string value of the argument. Defaults to an empty string.</value>
    internal string Arg { get; set; } = string.Empty;

    /// <summary>
    /// Gets the argument suggestions for auto-completion.
    /// </summary>
    /// <returns>An array of suggestion strings for the current argument.</returns>
    protected virtual string[] GetArgSuggestions()
    {
        return [];
    }

    /// <summary>
    /// Resets the argument back to its default state.
    /// </summary>
    internal void Reset()
    {
        Arg = string.Empty;
    }

    /// <summary>
    /// Gets the closest suggestion for the current argument value.
    /// </summary>
    /// <returns>The closest matching suggestion, or an empty string if none found.</returns>
    internal string GetClosestSuggestion() =>
        GetArgSuggestions().FirstOrDefault(name => name.StartsWith(Arg, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
}

/// <summary>
/// Generic abstract base class for typed command arguments.
/// </summary>
/// <typeparam name="T">The target type that this argument will be parsed into.</typeparam>
/// <param name="command">The command this argument belongs to.</param>
/// <param name="argInfo">Information about the argument, such as its name, description, or format.</param>
internal abstract class BaseArgument<T>(BaseCommand command, string argInfo) : BaseArgument(command, argInfo)
{
    /// <summary>
    /// Attempts to parse the current argument value to type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="result">When this method returns, contains the parsed value if parsing succeeded; otherwise, the default value for <typeparamref name="T"/>.</param>
    /// <returns><c>true</c> if the argument was successfully parsed; otherwise, <c>false</c>.</returns>
    internal abstract bool TryParse(out T result);
}