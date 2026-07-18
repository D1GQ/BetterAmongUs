using BetterAmongUs.Attributes;
using BetterAmongUs.Commands.Arguments;
using BetterAmongUs.Modules;
using BetterAmongUs.Utilities;

namespace BetterAmongUs.Commands;

[RegisterCommand]
internal sealed class KickCommand : BaseCommand
{
    internal override string Name => "kick";
    internal override string Description => "Kick a player from the game";
    internal override bool CanRunCommand(out string reason)
    {
        if (!GameState.IsHost)
        {
            reason = "Can only run as host";
            return false;
        }

        return base.CanRunCommand(out reason);
    }
    public KickCommand()
    {
        _playerArgument = new PlayerArgument(this);
        _boolArgument = new BoolArgument(this, "{ban}");
        Arguments = [_playerArgument, _boolArgument];
    }
    private readonly PlayerArgument _playerArgument;
    private readonly BoolArgument _boolArgument;

    internal override void Run()
    {
        if (!_playerArgument.TryParse(out var player))
            return;

        if (!_boolArgument.TryParse(out var ban))
            return;

        if (!player.IsHost())
        {
            player.Kick(ban);
        }
    }
}
