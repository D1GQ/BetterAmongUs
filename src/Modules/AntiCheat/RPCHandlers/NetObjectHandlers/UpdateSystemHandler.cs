using BetterAmongUs.Attributes;
using BetterAmongUs.Utilities;
using Hazel;
using UnityEngine;

namespace BetterAmongUs.Modules.AntiCheat.RPCHandlers.NetObjectHandlers;

[RegisterRPCHandler]
internal sealed class UpdateSystemHandler : RPCHandler
{
    internal override byte CallId => (byte)RpcCalls.UpdateSystem;

    internal SystemTypes CatchedSystemType;

    private readonly Dictionary<uint, Func<PlayerControl?, ISystemType, MessageReader, byte, bool>> systemHandlers;

    private static SabotageSystemType SabotageSystem => ShipStatus.Instance.Systems[SystemTypes.Sabotage].Cast<SabotageSystemType>();

    internal UpdateSystemHandler()
    {
        systemHandlers = new Dictionary<uint, Func<PlayerControl?, ISystemType, MessageReader, byte, bool>>
        {
            { (uint)SystemTypes.Sabotage, (sender, system, reader, count) => HandleSabotageSystem(sender, system.Cast<SabotageSystemType>(), reader) },
            { (uint)SystemTypes.Ventilation, (sender, system, reader, count) => HandleVentilationSystem(sender, system.Cast<VentilationSystem>(), count) },
            { (uint)SystemTypes.Electrical, (sender, system, reader, count) => HandleSwitchSystem(sender, system.Cast<SwitchSystem>(), count) },
            { (uint)SystemTypes.Comms, (sender, system, reader, count) => HandleCommsSystem(sender, system, count) },
            { (uint)SystemTypes.MushroomMixupSabotage, (sender, system, reader, count) => HandleMushroomMixupSabotageSystem(sender, system.Cast<MushroomMixupSabotageSystem>(), count) },
            { (uint)SystemTypes.Doors, (sender, system, reader, count) => HandleDoorsSystem(sender, system.Cast<DoorsSystemType>(), count) },
            { (uint)SystemTypes.Reactor, (sender, system, reader, count) => HandleReactorSystem(sender, system.Cast<ReactorSystemType>(), count) },
            { (uint)SystemTypes.Laboratory, (sender, system, reader, count) => HandleReactorSystem(sender, system.Cast<ReactorSystemType>(), count) },
            { (uint)SystemTypes.HeliSabotage, (sender, system, reader, count) => HandleHeliSabotageSystem(sender, system.Cast<HeliSabotageSystem>(), count) },
            { (uint)SystemTypes.LifeSupp, (sender, system, reader, count) => HandleLifeSuppSystem(sender, system.Cast<LifeSuppSystemType>(), count) }
        };
    }

    internal static bool CheckConsoleDistance<T>(PlayerControl? player, float distance = 2f) where T : PlayerTask, new()
    {
        if (player == null) return false;

        var playerPos = player.GetCustomPosition();
        var consolesPos = new T().FindConsolesPos();

        foreach (var consolePos in consolesPos)
        {
            if (Vector2.Distance(consolePos, playerPos) < distance)
                return true;
        }

        return false;
    }

    internal override bool HandleAntiCheatCancel(PlayerControl? sender, MessageReader reader)
    {
        if (sender.IsHost()) return true;

        byte count = reader.ReadByte();

        if (ShipStatus.Instance.Systems.TryGetValue(CatchedSystemType, out ISystemType system))
        {
            uint systemKey = (uint)CatchedSystemType;

            if (systemHandlers.TryGetValue(systemKey, out var handler))
            {
                return handler.Invoke(sender, system, reader, count);
            }
        }

        return true;
    }

    private static bool HandleSabotageSystem(PlayerControl? sender, SabotageSystemType sabotageSystem, MessageReader reader)
    {
        byte count = reader.ReadByte();

        if (!sender.IsImpostorTeam())
        {
            return false;
        }

        if (sabotageSystem.Timer > 0f)
        {
            return false;
        }

        return true;
    }

    private static bool HandleVentilationSystem(PlayerControl? sender, VentilationSystem ventilationSystem, byte count)
    {

        return true;
    }

    private static bool HandleSwitchSystem(PlayerControl? sender, SwitchSystem switchSystem, byte count)
    {
        if (count == 128) // Direct sabotage call from client, which is not possible, only the host should have this count when HandleSabotageSystem it's called
        {
            return false;
        }

        if (!switchSystem.IsActive)
        {
            return false;
        }

        if (!CheckConsoleDistance<ElectricTask>(sender))
        {
            return false;
        }

        return true;
    }

    private static bool HandleCommsSystem(PlayerControl? sender, ISystemType system, byte count)
    {
        if (system == null) return false;

        try
        {
            var hqHudSystem = system.Cast<HqHudSystemType>();
            return HandleHqHudSystem(sender, hqHudSystem, count);
        }
        catch
        {

        }

        try
        {
            var hudOverrideSystem = system.Cast<HudOverrideSystemType>();
            return HandleHudOverrideSystem(sender, hudOverrideSystem, count);
        }
        catch
        {

        }

        return true;
    }

    private static bool HandleHqHudSystem(PlayerControl? sender, HqHudSystemType hqHudSystem, byte count)
    {
        if ((count & 128) > 0) // Direct sabotage call from client, which is not possible, only the host should have this count when HandleSabotageSystem it's called
        {
            return false;
        }

        if (!hqHudSystem.IsActive)
        {
            return false;
        }

        if (!CheckConsoleDistance<HqHudOverrideTask>(sender, 2f))
        {
            return false;
        }

        return true;
    }

    private static bool HandleHudOverrideSystem(PlayerControl? sender, HudOverrideSystemType hudOverrideSystem, byte count)
    {
        if (count == 128) // Direct sabotage call from client, which is not possible, only the host should have this count when HandleSabotageSystem it's called
        {
            return false;
        }

        if (!hudOverrideSystem.IsActive)
        {
            return false;
        }

        if (!CheckConsoleDistance<HudOverrideTask>(sender, 2f))
        {
            return false;
        }

        return true;
    }

    private static bool HandleMushroomMixupSabotageSystem(PlayerControl? sender, MushroomMixupSabotageSystem mushroomMixupSabotage, byte count)
    {
        if (count == 1) // Direct sabotage call from client, which is not possible, only the host should have this count when HandleSabotageSystem it's called
        {
            return false;
        }

        if (mushroomMixupSabotage.IsActive)
        {
            return false;
        }

        return true;
    }

    private static bool HandleDoorsSystem(PlayerControl? sender, DoorsSystemType doorsSystem, byte count)
    {
        if (count == 128) // Direct sabotage call from client, which is not possible, only the host should have this count when HandleSabotageSystem it's called
        {
            return false;
        }

        return true;
    }

    private static bool HandleReactorSystem(PlayerControl? sender, ReactorSystemType reactorSystem, byte count)
    {
        if (count == 128 || count == 16) // Direct sabotage call from client, which is not possible, only the host should have this count when HandleSabotageSystem it's called
        {
            return false;
        }

        if (!reactorSystem.IsActive)
        {
            return false;
        }

        if (count.HasAnyBit(64))
        {
            foreach (var tuple in reactorSystem.UserConsolePairs)
            {
                if (tuple.Item1 == sender.PlayerId)
                {
                    return false;
                }
            }
        }

        /*
          if (!CheckConsoleDistance<ReactorTask>(sender))
          {
              return false;
          }
         */

        return true;
    }

    private static bool HandleHeliSabotageSystem(PlayerControl? sender, HeliSabotageSystem heliSabotageSystem, byte count)
    {
        if (count == 128) // Direct sabotage call from client, which is not possible, only the host should have this count when HandleSabotageSystem it's called
        {
            return false;
        }

        if (!heliSabotageSystem.IsActive)
        {
            return false;
        }

        if (!CheckConsoleDistance<HeliCharlesTask>(sender))
        {
            return false;
        }

        return true;
    }

    private static bool HandleLifeSuppSystem(PlayerControl? sender, LifeSuppSystemType lifeSuppSystem, byte count)
    {
        if (count == 128) // Direct sabotage call from client, which is not possible, only the host should have this count when HandleSabotageSystem it's called
        {
            return false;
        }

        if (!lifeSuppSystem.IsActive)
        {
            return false;
        }

        /*
        if (!CheckConsoleDistance<NoOxyTask>(sender))
        {
            return false;
        }
        */

        return true;
    }
}