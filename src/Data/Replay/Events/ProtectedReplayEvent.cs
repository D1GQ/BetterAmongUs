using BetterAmongUs.Interfaces;
using BetterAmongUs.Utilities;
using System.Text.Json.Serialization;

namespace BetterAmongUs.Data.Replay.Events;

[Serializable]
internal sealed class ProtectedReplayEvent : IReplayEvent<ProtectedReplayEvent.ProtectedReplayData, ProtectedReplayEvent.ProtectedReplayArgs>
{
    [JsonPropertyName("id")]
    public string Id => "player_protected";

    [JsonPropertyName("eventData")]
    public ProtectedReplayData? EventData { get; set; }

    public void Play()
    {
        if (EventData == null)
            return;

        var target = Utils.PlayerFromPlayerId(EventData.TargetId);
        if (target == null)
            return;

        RoleEffectAnimation roleEffectAnimation = UnityEngine.Object.Instantiate(RoleManager.Instance.protectAnim, target.transform);
        roleEffectAnimation.SetMaskLayerBasedOnWhoShouldSee(true);
        roleEffectAnimation.Play(target, null, target.cosmetics.FlipX, RoleEffectAnimation.SoundType.Global, 0f, true, 0f);
    }

    public void Undo()
    {
    }

    public void Record(ProtectedReplayArgs args)
    {
        EventData = new ProtectedReplayData(args.Killer.PlayerId, args.Target.PlayerId);
    }

    internal record ProtectedReplayData(int KillerId, int TargetId) : IReplayEvent.Data;

    internal record ProtectedReplayArgs(PlayerControl Killer, PlayerControl Target) : IReplayEvent.Args;
}