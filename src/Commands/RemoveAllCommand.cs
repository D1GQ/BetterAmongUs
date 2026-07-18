using BetterAmongUs.Data;
using BetterAmongUs.Utilities;
using BetterAmongUs.Attributes;

namespace BetterAmongUs.Commands;

[RegisterCommand]
internal sealed class RemoveAllCommand : BaseCommand
{
    internal override string Name => "removeall";
    internal override string Description => "Remove all players from local <color=#4f92ff>Anti-Cheat</color> data";
    internal override void Run()
    {
        BetterDataManager.ClearCheatData();
        Utils.AddChatPrivate($"All data successfully removed from local <color=#4f92ff>Anti-Cheat</color>!");
    }
}
