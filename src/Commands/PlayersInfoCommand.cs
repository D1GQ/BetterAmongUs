using BetterAmongUs.Attributes;
using BetterAmongUs.Helpers;
using System.Text;

namespace BetterAmongUs.Commands;

[RegisterCommand]
internal sealed class PlayersInfoCommand : BaseCommand
{
    internal override string Name => "players";
    internal override string Description => "Get all Player information";

    internal override void Run()
    {
        StringBuilder sb = new();
        foreach (PlayerControl player in BAUPlugin.AllPlayerControls.Where(player => !player.isDummy))
        {
            if (player == null || player.Data == null) continue;

            var hexColor = Utils.Color32ToHex(Palette.PlayerColors[player.CurrentOutfit.ColorId]);
            sb.Append($"<color={hexColor}><b>{player.Data.PlayerName}</color> Info:</b>\n");
            sb.Append($"<color=#c1c1c1>{player.Data.PlayerId}</color> - ");
            sb.Append($"<color=#c1c1c1>{Utils.GetHashStr($"{player.Data.Puid}")}</color> - ");
            sb.Append($"<color=#c1c1c1>{Utils.GetPlatformName(player)}</color> - ");
            sb.Append($"<color=#c1c1c1>{player.Data.FriendCode}</color>");
            sb.Append("\n\n");
        }
        CommandResultText(sb.ToString());
    }
}
