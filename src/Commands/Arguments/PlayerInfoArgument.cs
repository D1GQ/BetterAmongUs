using BetterAmongUs.Utilities;

namespace BetterAmongUs.Commands.Arguments;

/// <summary>
/// Represents a player command argument.
/// </summary>
/// <param name="command">The command this argument belongs to.</param>
/// <param name="argInfo">Information about the argument (default: "{player}").</param>
internal sealed class PlayerInfoArgument(BaseCommand command, string argInfo = "{player}") : BaseArgument<NetworkedPlayerInfo>(command, argInfo)
{
    protected override string[] GetArgSuggestions()
    {
        List<string> suggestions = [];
        var allPlayers = GameData.Instance.AllPlayers;
        int count = allPlayers.Count;

        for (int i = 0; i < count; i++)
        {
            var playerData = allPlayers[i];
            if (playerData.IsLocalData())
            {
                int insertIndex = suggestions.Count;

                if (!string.IsNullOrEmpty(playerData.PlayerName))
                    suggestions.Insert(insertIndex, playerData.PlayerName.Replace(' ', '_'));

                if (!string.IsNullOrEmpty(playerData.FriendCode))
                    suggestions.Insert(insertIndex + 1, playerData.FriendCode);

                suggestions.Insert(insertIndex + 2, $"ID{playerData.PlayerId}");
            }
            else
            {
                if (!string.IsNullOrEmpty(playerData.PlayerName))
                    suggestions.Add(playerData.PlayerName.Replace(' ', '_'));

                if (!string.IsNullOrEmpty(playerData.FriendCode))
                    suggestions.Add(playerData.FriendCode);

                suggestions.Add($"ID{playerData.PlayerId}");
            }
        }

        return [.. suggestions];
    }

    /// <summary>
    /// Tries to parse the player argument and find the corresponding PlayerControl. 
    /// </summary>
    internal override bool TryParse(out NetworkedPlayerInfo result)
    {
        foreach (var playerData in GameData.Instance.AllPlayers)
        {
            if (IsMatchingPlayer(playerData))
            {
                result = playerData;
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
    private bool IsMatchingPlayer(NetworkedPlayerInfo data)
    {
        return Arg.Equals(data.PlayerName.Replace(' ', '_'), StringComparison.OrdinalIgnoreCase)
            || Arg.Equals(data.FriendCode, StringComparison.OrdinalIgnoreCase)
            || Arg.Equals($"ID{data.PlayerId}", StringComparison.OrdinalIgnoreCase);
    }
}