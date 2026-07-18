using BetterAmongUs.Attributes;
using BetterAmongUs.Commands.Arguments;
using BetterAmongUs.Data;
using BetterAmongUs.Utilities;

namespace BetterAmongUs.Commands;

[RegisterCommand]
internal sealed class RemovePlayerCommand : BaseCommand
{
    internal override string Name => "removeplayer";
    internal override string Description => "Remove player from local <color=#4f92ff>Anti-Cheat</color> data";

    public RemovePlayerCommand()
    {
        _identifierArgument = new StringArgument(this, "{identifier}")
        {
            ArgSuggestions = () =>
                BetterDataManager.Files.BetterDataFile.AllCheatData
                    .SelectMany(info => new[] { info.HashPuid.Replace(' ', '_'), info.FriendCode.Replace(' ', '_'), info.PlayerName.Replace(' ', '_') })
                    .ToArray()
        };
        Arguments = [_identifierArgument];
    }
    private readonly StringArgument _identifierArgument;

    internal override void Run()
    {
        if (_identifierArgument.TryParse(out var identifierArgument))
        {
            if (BetterDataManager.RemovePlayer(identifierArgument) == true)
            {
                Utils.AddChatPrivate($"{identifierArgument} successfully removed from local <color=#4f92ff>Anti-Cheat</color> data!");
            }
            else
            {
                Utils.AddChatPrivate($"{identifierArgument} Could not find player data from identifier");
            }
        }
    }
}
