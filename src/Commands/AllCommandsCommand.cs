using BetterAmongUs.Attributes;
using BetterAmongUs.Patches.Gameplay.UI.Chat;

namespace BetterAmongUs.Commands;

[RegisterCommand]
internal sealed class AllCommandsCommand : BaseCommand
{
    internal override string Name => "commands";
    internal override string Description => "Get information about all commands";

    internal override void Run()
    {
        BaseCommand[] allNormalCommands = [.. allCommands.Where(cmd => cmd.IsEnabled())];
        string list;
        var open = "<color=#858585>┌──────── </color>";
        var mid = "<color=#858585>├ </color>";
        var close = "<color=#858585>└──────── </color>";
        list = "<color=#00751f><b><size=150%>Command List</size></b></color>\n" + open;

        if (allNormalCommands.Length > 0)
        {
            foreach (var command in allNormalCommands)
            {
                list += $"\n{mid}<color=#e0b700><b>{ChatCommandsPatch.CommandPrefix}{command.Name}</b></color> <size=65%><color=#735e00>{command.Description}.</color></size>";
            }
        }

        list += "\n" + close;
        CommandResultText(list);
    }
}
