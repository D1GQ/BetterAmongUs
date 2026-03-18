using BetterAmongUs.Attributes;
using BetterAmongUs.Commands.Arguments;
using BetterAmongUs.Helpers;
using System.Text;

namespace BetterAmongUs.Commands;

[RegisterCommand]
internal sealed class PlayerInfoCommand : BaseCommand
{
    internal override string Name => "player";
    internal override string Description => "Get a Players information";

    public PlayerInfoCommand()
    {
        _playerArgument = new PlayerArgument(this);
        Arguments = [_playerArgument];
    }
    private readonly PlayerArgument _playerArgument;

    internal override void Run()
    {
        var player = _playerArgument.TryGetTarget();
        if (player == null)
            return;

        if (player.Data == null)
            return;

        StringBuilder sb = new();
        var hexColor = Utils.Color32ToHex(Palette.PlayerColors[player.CurrentOutfit.ColorId]);
        var format1 = "┌ •";
        var format2 = "├ •";
        var format3 = "└ •";
        sb.Append($"<size=150%><color={hexColor}><b>{player.Data.PlayerName}</color> Info:</b></size>\n");
        sb.Append($"{format1} <color=#c1c1c1>ID: {player.Data.PlayerId}</color>\n");
        sb.Append($"{format2} <color=#c1c1c1>HashPUID: {Utils.GetHashStr($"{player.Data.Puid}")}</color>\n");
        sb.Append($"{format2} <color=#c1c1c1>Platform: {Utils.GetPlatformName(player)}</color>\n");
        sb.Append($"{format3} <color=#c1c1c1>FriendCode: {player.Data.FriendCode}</color>");
        CommandResultText(sb.ToString());
    }
}
