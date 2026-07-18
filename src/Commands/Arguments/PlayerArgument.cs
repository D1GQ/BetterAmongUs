using BetterAmongUs.Utilities;

namespace BetterAmongUs.Commands.Arguments;

/// <summary>
/// Represents a player command argument.
/// </summary>
/// <param name="command">The command this argument belongs to.</param>
/// <param name="argInfo">Information about the argument (default: "{player}").</param>
internal sealed class PlayerArgument(BaseCommand command, string argInfo = "{player}") : BaseArgument<PlayerControl>(command, argInfo)
{
    protected override string[] GetArgSuggestions()
    {
        List<string> suggestions = [];
        foreach (var player in BAUPlugin.AllPlayerControls.OrderBy(pc => pc.IsLocalPlayer() ? 0 : 1))
        {
            if (player.Data == null)
                continue;

            if (!string.IsNullOrEmpty(player.Data.PlayerName))
            {
                suggestions.Add(player.Data.PlayerName.Replace(' ', '_'));
            }

            if (!string.IsNullOrEmpty(player.Data.FriendCode))
            {
                suggestions.Add(player.Data.FriendCode);
            }

            suggestions.Add($"ID{player.Data.PlayerId}");
        }

        return [.. suggestions];
    }

    /// <summary>
    /// Tries to parse the player argument and find the corresponding PlayerControl. 
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    internal override bool TryParse(out PlayerControl result)
    {
        foreach (var player in BAUPlugin.AllPlayerControls)
        {
            if (player.Data == null) continue;

            if (IsMatchingPlayer(player.Data))
            {
                result = player;
                return true;
            }
        }

        result = default!;
        BaseCommand.CommandErrorText("Player not found!");
        return false;
    }

    /// <summary>
    /// Checks if the given player info matches the argument value.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private bool IsMatchingPlayer(NetworkedPlayerInfo data)
    {
        return Arg.Equals(data.PlayerName.Replace(' ', '_'), StringComparison.OrdinalIgnoreCase)
            || Arg.Equals(data.FriendCode, StringComparison.OrdinalIgnoreCase)
            || Arg.Equals($"ID{data.PlayerId}", StringComparison.OrdinalIgnoreCase);
    }
}