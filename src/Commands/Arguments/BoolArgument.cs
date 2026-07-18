namespace BetterAmongUs.Commands.Arguments;

/// <summary>
/// Represents a boolean command argument.
/// </summary>
/// <param name="command">The command this argument belongs to.</param>
/// <param name="argInfo">Information about the argument (default: "{bool}").</param>
internal sealed class BoolArgument(BaseCommand command, string argInfo = "{bool}") : BaseArgument<bool>(command, argInfo)
{
    protected override string[] GetArgSuggestions()
    {
        return ["true", "false"];
    }

    /// <summary>
    /// Tries to parse the argument as a boolean value.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    internal override bool TryParse(out bool result)
    {
        if (Arg.ToLower() == "true")
        {
            result = true;
            return true;
        }
        else if (Arg.ToLower() is "false" or "")
        {
            result = false;
            return true;
        }
        else
        {
            BaseCommand.CommandErrorText($"Invalid Syntax!");
            result = default;
            return false;
        }
    }
}