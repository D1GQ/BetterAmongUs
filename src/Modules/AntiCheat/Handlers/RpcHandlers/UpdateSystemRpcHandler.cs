using BetterAmongUs.Attributes;
using BetterAmongUs.Enums;
using BetterAmongUs.Generated;
using BetterAmongUs.MonoScripts.Extended;
using BetterAmongUs.Patches.Gameplay.UI.Settings;
using BetterAmongUs.Utilities;
using Hazel;
using InnerNet;
using UnityEngine;

namespace BetterAmongUs.Modules.AntiCheat.Handlers.RpcHandlers;

[RegisterRpcHandler(RpcCalls.UpdateSystem)]
internal sealed class UpdateSystemRpcHandler : IRpcHandler<ShipStatus>
{
    private bool CheckConsoleDistance<T>(PlayerControl player, float distance = 2f) where T : PlayerTask, new()
    {
        var playerPos = player.GetCustomPosition();
        var consolesPos = new T().FindConsolesPos();

        foreach (var consolePos in consolesPos)
        {
            if (Vector2.Distance(consolePos, playerPos) < distance)
                return true;
        }

        return false;
    }

    public AntiCheatFlags CheckRpc(ShipStatus innerNetObject, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
        SystemTypes systemType = (SystemTypes)reader.ReadByte();
        PlayerControl sender = reader.ReadNetObject<PlayerControl>();

        if (innerNetObject.Systems.TryGetValue(systemType, out var system))
        {
            switch (systemType)
            {
                case SystemTypes.Sabotage:
                    return HandleSabotageSystem(system.Cast<SabotageSystemType>(), sender, reader, ref translationReason);
                case SystemTypes.Ventilation:
                    return HandleVentilationSystem(system.Cast<VentilationSystem>(), sender, reader, ref translationReason);
                case SystemTypes.Electrical:
                    return HandleSwitchSystem(system.Cast<SwitchSystem>(), sender, reader, ref translationReason);
                case SystemTypes.Comms:
                    return HandleCommsSystem(system, sender, reader, ref translationReason);
                case SystemTypes.MushroomMixupSabotage:
                    return HandleMushroomMixupSabotageSystem(system.Cast<MushroomMixupSabotageSystem>(), sender, reader, ref translationReason);
                case SystemTypes.Reactor:
                case SystemTypes.Laboratory:
                    return HandleReactorSystem(system.Cast<ReactorSystemType>(), sender, reader, ref translationReason);
                case SystemTypes.HeliSabotage:
                    return HandleHeliSabotageSystem(system.Cast<HeliSabotageSystem>(), sender, reader, ref translationReason);
                case SystemTypes.LifeSupp:
                    return HandleLifeSuppSystem(system.Cast<LifeSuppSystemType>(), sender, reader, ref translationReason);
            }
        }

        return AntiCheatFlags.None;
    }

    private AntiCheatFlags HandleSabotageSystem(SabotageSystemType sabotageSystem, PlayerControl sender, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
        byte count = reader.ReadByte();

        if (!sender.IsImpostorTeam())
        {
            return AntiCheatFlags.Cancel;
        }

        if (sabotageSystem.Timer > 0f)
        {
            return AntiCheatFlags.Cancel;
        }

        return AntiCheatFlags.None;
    }

    private AntiCheatFlags HandleVentilationSystem(VentilationSystem ventilationSystem, PlayerControl sender, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
        _ = reader.ReadByte();

        _ = reader.ReadUInt16();
        var operation = (VentilationSystem.Operation)reader.ReadByte();
        if (operation == VentilationSystem.Operation.BootImpostors && !sender.IsHost())
        {
            return AntiCheatFlags.High | AntiCheatFlags.Cancel;
        }

        return AntiCheatFlags.None;
    }

    private AntiCheatFlags HandleSwitchSystem(SwitchSystem switchSystem, PlayerControl sender, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
        byte count = reader.ReadByte();

        if (count == 128) // Direct sabotage call from client, which is not possible, only the host should have this count when HandleSabotageSystem it's called
        {
            return AntiCheatFlags.High | AntiCheatFlags.Cancel;
        }

        if (!switchSystem.IsActive)
        {
            return AntiCheatFlags.Cancel;
        }

        if (!CheckConsoleDistance<ElectricTask>(sender))
        {
            return AntiCheatFlags.Cancel;
        }

        return AntiCheatFlags.None;
    }

    private AntiCheatFlags HandleCommsSystem(ISystemType system, PlayerControl sender, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
        try
        {
            var hqHudSystem = system.Cast<HqHudSystemType>();
            return HandleHqHudSystem(hqHudSystem, sender, reader, ref translationReason);
        }
        catch
        {

        }

        try
        {
            var hudOverrideSystem = system.Cast<HudOverrideSystemType>();
            return HandleHudOverrideSystem(hudOverrideSystem, sender, reader, ref translationReason);
        }
        catch
        {

        }

        return AntiCheatFlags.None;
    }

    private AntiCheatFlags HandleHqHudSystem(HqHudSystemType hqHudSystem, PlayerControl sender, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
        byte count = reader.ReadByte();

        if ((count & 128) > 0) // Direct sabotage call from client, which is not possible, only the host should have this count when HandleSabotageSystem it's called
        {
            return AntiCheatFlags.High | AntiCheatFlags.Cancel;
        }

        if (!hqHudSystem.IsActive)
        {
            return AntiCheatFlags.Cancel;
        }

        if (!CheckConsoleDistance<HqHudOverrideTask>(sender, 2f))
        {
            return AntiCheatFlags.Cancel;
        }

        return AntiCheatFlags.None;
    }

    private AntiCheatFlags HandleHudOverrideSystem(HudOverrideSystemType hudOverrideSystem, PlayerControl sender, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
        byte count = reader.ReadByte();

        if (count == 128) // Direct sabotage call from client, which is not possible, only the host should have this count when HandleSabotageSystem it's called
        {
            return AntiCheatFlags.High | AntiCheatFlags.Cancel;
        }

        if (!hudOverrideSystem.IsActive)
        {
            return AntiCheatFlags.Cancel;
        }

        if (!CheckConsoleDistance<HudOverrideTask>(sender, 2f))
        {
            return AntiCheatFlags.Cancel;
        }

        return AntiCheatFlags.None;
    }

    private AntiCheatFlags HandleMushroomMixupSabotageSystem(MushroomMixupSabotageSystem mushroomMixupSabotage, PlayerControl sender, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
        byte count = reader.ReadByte();

        if (count == 1) // Direct sabotage call from client, which is not possible, only the host should have this count when HandleSabotageSystem it's called
        {
            return AntiCheatFlags.High | AntiCheatFlags.Cancel;
        }

        if (mushroomMixupSabotage.IsActive)
        {
            return AntiCheatFlags.Cancel;
        }

        return AntiCheatFlags.None;
    }

    private AntiCheatFlags HandleReactorSystem(ReactorSystemType reactorSystem, PlayerControl sender, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
        byte count = reader.ReadByte();

        if (count == 128 || count == 16) // Direct sabotage call from client, which is not possible, only the host should have this count when HandleSabotageSystem it's called
        {
            return AntiCheatFlags.High | AntiCheatFlags.Cancel;
        }

        if (!reactorSystem.IsActive)
        {
            return AntiCheatFlags.Cancel;
        }

        if (count.HasAnyBit(64))
        {
            foreach (var tuple in reactorSystem.UserConsolePairs)
            {
                if (tuple.Item1 == sender.PlayerId)
                {
                    return AntiCheatFlags.Cancel;
                }
            }
        }

        return AntiCheatFlags.None;
    }

    private AntiCheatFlags HandleHeliSabotageSystem(HeliSabotageSystem heliSabotageSystem, PlayerControl sender, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
        byte count = reader.ReadByte();

        if (count == 128) // Direct sabotage call from client, which is not possible, only the host should have this count when HandleSabotageSystem it's called
        {
            return AntiCheatFlags.High | AntiCheatFlags.Cancel;
        }

        if (!heliSabotageSystem.IsActive)
        {
            return AntiCheatFlags.Cancel;
        }

        if (!CheckConsoleDistance<HeliCharlesTask>(sender))
        {
            return AntiCheatFlags.Cancel;
        }

        return AntiCheatFlags.None;
    }

    private AntiCheatFlags HandleLifeSuppSystem(LifeSuppSystemType lifeSuppSystem, PlayerControl sender, MessageReader reader, ref TranslationStrings.TranslationString translationReason)
    {
        byte count = reader.ReadByte();

        if (count == 128) // Direct sabotage call from client, which is not possible, only the host should have this count when HandleSabotageSystem it's called
        {
            return AntiCheatFlags.High | AntiCheatFlags.Cancel;
        }

        if (!lifeSuppSystem.IsActive)
        {
            return AntiCheatFlags.Cancel;
        }


        return AntiCheatFlags.None;
    }

    public void HandleRpc(ShipStatus innerNetObject, MessageReader reader)
    {
        _ = reader.ReadByte();
        PlayerControl sender = reader.ReadNetObject<PlayerControl>();

        if (sender.ExtendedData() != null && BetterGameSettings.RpcRateLimiting.GetBool())
        {
            sender.ExtendedData().AntiCheatInfo.RPCSentPS++;
        }
    }
}
