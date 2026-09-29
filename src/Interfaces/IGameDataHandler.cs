using Hazel;

namespace BetterAmongUs.Interfaces;

internal interface IGameDataHandler
{
    void Handle(MessageReader reader);
}
