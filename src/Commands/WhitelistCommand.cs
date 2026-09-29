using BetterAmongUs.Attributes;
using BetterAmongUs.Commands.Arguments;
using BetterAmongUs.Data;
using BetterAmongUs.Modules;
using BetterAmongUs.Utilities;

namespace BetterAmongUs.Commands;

[RegisterCommand]
internal sealed class WhitelistCommand : BaseCommand
{
    internal override string Name => "whitelist";
    internal override string Description => "Add a player to the whitelist";

    public WhitelistCommand()
    {
        _playerArgument = new PlayerArgument(this);
        Arguments = [_playerArgument];
    }
    private readonly PlayerArgument _playerArgument;

    internal override void Run()
    {
        if (!_playerArgument.TryParse(out var player))
            return;

        if (player.Data == null)
            return;

        BetterDataManager.AddToWhiteList(player.Data.FriendCode, player.Data.Puid);

        if (TextFileHandler.CompareStringMatch(BetterDataManager.Files.whiteListFilePath, [player.Data.FriendCode, player.GetHashPuid()]))
        {
            CommandResultText("Successfully added to the whitelist.");
        }
        else
        {
            CommandErrorText("Failed to add to the whitelist.");
        }
    }
}
