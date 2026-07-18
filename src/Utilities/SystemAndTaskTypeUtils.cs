using BetterAmongUs.Modules;

namespace BetterAmongUs.Utilities;

/// <summary>
/// Provides utility methods and constants for working with system types and task types.
/// </summary>
internal static class SystemAndTaskTypeUtils
{
    /// <summary>
    /// Array of all task types that are considered sabotage tasks.
    /// </summary>
    internal static readonly TaskTypes[] AllSabotageTasks =
    [
        TaskTypes.FixLights,
        TaskTypes.FixComms,
        TaskTypes.RestoreOxy,
        TaskTypes.ResetReactor,
        TaskTypes.ResetSeismic
    ];

    /// <summary>
    /// Array of all system types that are considered sabotage systems across all maps.
    /// </summary>
    internal static readonly SystemTypes[] AllSabotages =
    [
        SystemTypes.Electrical,
        SystemTypes.Reactor,
        SystemTypes.Laboratory,
        SystemTypes.LifeSupp,
        SystemTypes.HeliSabotage,
        SystemTypes.Comms,
        SystemTypes.MushroomMixupSabotage
    ];

    /// <summary>
    /// Array of system types that are considered critical sabotages.
    /// </summary>
    internal static readonly SystemTypes[] CriticalSabotages =
    [
        SystemTypes.Reactor,
        SystemTypes.Laboratory,
        SystemTypes.LifeSupp,
        SystemTypes.HeliSabotage,
    ];

    /// <summary>
    /// Array of system types that are considered non-critical sabotages.
    /// </summary>
    internal static readonly SystemTypes[] NoneCriticalSabotages =
    [
        SystemTypes.Electrical,
        SystemTypes.Comms,
        SystemTypes.MushroomMixupSabotage
    ];

    /// <summary>
    /// Checks whether a specific system type is currently active on the map.
    /// </summary>
    /// <param name="type">The system type to check.</param>
    /// <returns>True if the system is active, false otherwise.</returns>
    /// <remarks>
    /// System behavior varies by map ID and system type. This method handles map-specific
    /// system activation logic for different sabotage types.
    /// </remarks>
    internal static bool IsSystemActive(SystemTypes type)
    {
        if (GameState.IsHideNSeek || ShipStatus.Instance == null ||
            !ShipStatus.Instance.Systems.TryGetValue(type, out var system))
        {
            return false;
        }

        int mapId = GameState.GetActiveMapId;

        switch (type)
        {
            case SystemTypes.Electrical when mapId != 5:
                var switchSystem = system.TryCast<SwitchSystem>();
                return switchSystem != null && !switchSystem.IsActive;

            case SystemTypes.Reactor when mapId != 2:
                var reactorSystem = system.TryCast<ReactorSystemType>();
                return reactorSystem != null && reactorSystem.IsActive;

            case SystemTypes.Laboratory when mapId == 2:
                var labSystem = system.TryCast<ReactorSystemType>();
                return labSystem != null && labSystem.IsActive;

            case SystemTypes.LifeSupp when mapId is 0 or 3:
                var lifeSuppSystem = system.TryCast<LifeSuppSystemType>();
                return lifeSuppSystem != null && lifeSuppSystem.IsActive;

            case SystemTypes.HeliSabotage when mapId == 4:
                var heliSystem = system.TryCast<HeliSabotageSystem>();
                return heliSystem != null && heliSystem.IsActive;

            case SystemTypes.Comms when mapId is 1 or 5:
                var hqSystem = system.TryCast<HqHudSystemType>();
                return hqSystem != null && hqSystem.IsActive;

            case SystemTypes.Comms:
                var hudSystem = system.TryCast<HudOverrideSystemType>();
                return hudSystem != null && hudSystem.IsActive;

            case SystemTypes.MushroomMixupSabotage when mapId == 5:
                var mushroomSystem = system.TryCast<MushroomMixupSabotageSystem>();
                return mushroomSystem != null && mushroomSystem.IsActive;

            default:
                return false;
        }
    }

    /// <summary>
    /// Checks if any critical sabotage is currently active.
    /// </summary>
    /// <returns>True if a critical sabotage is active, false otherwise.</returns>
    internal static bool IsCriticalSabotageActive()
    {
        return CriticalSabotages.Any(IsSystemActive);
    }

    /// <summary>
    /// Checks if any non-critical sabotage is currently active.
    /// </summary>
    /// <returns>True if a non-critical sabotage is active, false otherwise.</returns>
    internal static bool IsNoneCriticalSabotageActive()
    {
        return NoneCriticalSabotages.Any(IsSystemActive);
    }

    /// <summary>
    /// Checks if any sabotage (critical or non-critical) is currently active.
    /// </summary>
    /// <returns>True if any sabotage is active, false otherwise.</returns>
    internal static bool IsAnySabotageActive()
    {
        return AllSabotages.Any(IsSystemActive);
    }

    /// <summary>
    /// Determines whether a given task type is a sabotage task.
    /// </summary>
    /// <param name="taskType">The task type to check.</param>
    /// <returns>True if the task type is a sabotage task, false otherwise.</returns>
    internal static bool IsSabotageTask(TaskTypes taskType)
    {
        return AllSabotageTasks.Contains(taskType);
    }
}