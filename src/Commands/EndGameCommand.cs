using BetterAmongUs.Attributes;
using BetterAmongUs.Modules;

namespace BetterAmongUs.Commands;

[RegisterCommand]
internal sealed class EndGameCommand : BaseCommand
{
    internal override string Name => "endgame";
    internal override string Description => "Force end the game";
    internal override bool CanRunCommand(out string reason)
    {
        if (!GameState.IsHost)
        {
            reason = "Can only run as host";
            return false;
        }

        if (!GameState.IsInGamePlay)
        {
            reason = "Can only run in gameplay";
            return false;
        }

        return base.CanRunCommand(out reason);
    }

    internal override void Run()
    {
        GameManager.Instance.RpcEndGame(GameOverReason.ImpostorDisconnect, false);
    }
}
