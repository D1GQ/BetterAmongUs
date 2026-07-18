using AmongUs.GameOptions;
using BetterAmongUs.Interfaces;
using BetterAmongUs.Utilities;
using System.Text.Json.Serialization;

namespace BetterAmongUs.Data.Replay.Events;

[Serializable]
internal sealed class SetRoleEvent : IReplayEvent<SetRoleEvent.SetRoleReplayData, SetRoleEvent.SetRoleReplayArgs>
{
    [JsonPropertyName("id")]
    public string Id => "set_role";

    [JsonPropertyName("eventData")]
    public SetRoleReplayData? EventData { get; set; }

    public void Play()
    {
        if (EventData == null)
            return;

        var player = Utils.PlayerFromPlayerId(EventData.PlayerId);
        if (player == null)
            return;

        var roleType = (RoleTypes)EventData.RoleType;
        player.RpcSetRole(roleType, true);
    }

    public void Undo()
    {
    }

    public void Record(SetRoleReplayArgs args)
    {
        EventData = new SetRoleReplayData(args.Player.PlayerId, (int)args.RoleType);
    }

    internal record SetRoleReplayData(int PlayerId, int RoleType) : IReplayEvent.Data;

    internal record SetRoleReplayArgs(PlayerControl Player, RoleTypes RoleType) : IReplayEvent.Args;
}