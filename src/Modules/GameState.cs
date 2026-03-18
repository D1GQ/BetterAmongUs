using AmongUs.GameOptions;
using BetterAmongUs.Data.Config;

namespace BetterAmongUs.Modules;

/// <summary>
/// Provides static properties to check various game states and conditions.
/// </summary>
internal static class GameState
{
    /**********Check Game Status***********/
    /// <summary>
    /// Gets whether any players exist in the game.
    /// </summary>
    internal static bool InGame => BAUPlugin.AllPlayerControls.Any();

    /// <summary>
    /// Gets whether the current game mode is Normal or NormalFools.
    /// </summary>
    internal static bool IsNormalGame => GameOptionsManager.Instance != null &&
        GameOptionsManager.Instance.CurrentGameOptions != null &&
        GameOptionsManager.Instance.CurrentGameOptions.GameMode is GameModes.Normal or GameModes.NormalFools;

    /// <summary>
    /// Gets whether the current game mode is HideNSeek or SeekFools.
    /// </summary>
    internal static bool IsHideNSeek => GameOptionsManager.Instance != null &&
        GameOptionsManager.Instance.CurrentGameOptions != null &&
        GameOptionsManager.Instance.CurrentGameOptions.GameMode is GameModes.HideNSeek or GameModes.SeekFools;

    /// <summary>
    /// Gets whether the Skeld map is currently active.
    /// </summary>
    internal static bool SkeldIsActive => GameOptionsManager.Instance != null &&
        GameOptionsManager.Instance.CurrentGameOptions != null &&
        (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId == MapNames.Skeld;

    /// <summary>
    /// Gets whether the MiraHQ map is currently active.
    /// </summary>
    internal static bool MiraHQIsActive => GameOptionsManager.Instance != null &&
        GameOptionsManager.Instance.CurrentGameOptions != null &&
        (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId == MapNames.MiraHQ;

    /// <summary>
    /// Gets whether the Polus map is currently active.
    /// </summary>
    internal static bool PolusIsActive => GameOptionsManager.Instance != null &&
        GameOptionsManager.Instance.CurrentGameOptions != null &&
        (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId == MapNames.Polus;

    /// <summary>
    /// Gets whether the Dleks map is currently active.
    /// </summary>
    internal static bool DleksIsActive => GameOptionsManager.Instance != null &&
        GameOptionsManager.Instance.CurrentGameOptions != null &&
        (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId == MapNames.Dleks;

    /// <summary>
    /// Gets whether the Airship map is currently active.
    /// </summary>
    internal static bool AirshipIsActive => GameOptionsManager.Instance != null &&
        GameOptionsManager.Instance.CurrentGameOptions != null &&
        (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId == MapNames.Airship;

    /// <summary>
    /// Gets whether the Fungle map is currently active.
    /// </summary>
    internal static bool FungleIsActive => GameOptionsManager.Instance != null &&
        GameOptionsManager.Instance.CurrentGameOptions != null &&
        (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId == MapNames.Fungle;

    /// <summary>
    /// Gets the ID of the currently active map.
    /// </summary>
    internal static byte GetActiveMapId => GameOptionsManager.Instance != null &&
        GameOptionsManager.Instance.CurrentGameOptions != null ?
        GameOptionsManager.Instance.CurrentGameOptions.MapId : (byte)0;

    /// <summary>
    /// Checks whether a specific system type is currently active on the map.
    /// </summary>
    /// <param name="type">The system type to check.</param>
    /// <returns>True if the system is active, false otherwise.</returns>
    internal static bool IsSystemActive(SystemTypes type)
    {
        if (IsHideNSeek || ShipStatus.Instance == null ||
            !ShipStatus.Instance.Systems.TryGetValue(type, out var system))
        {
            return false;
        }

        int mapId = GetActiveMapId;

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
    /// Checks if any critical (death-causing) sabotage is currently active.
    /// </summary>
    /// <returns>True if a critical sabotage is active, false otherwise.</returns>
    internal static bool IsCriticalSabotageActive()
    {
        var deathSabotages = new[]
        {
            SystemTypes.Reactor,
            SystemTypes.Laboratory,
            SystemTypes.LifeSupp,
            SystemTypes.HeliSabotage,
        };

        return deathSabotages.Any(IsSystemActive);
    }

    /// <summary>
    /// Checks if any non-critical sabotage is currently active.
    /// </summary>
    /// <returns>True if a non-critical sabotage is active, false otherwise.</returns>
    internal static bool IsNoneCriticalSabotageActive()
    {
        var noneDeathSabotages = new[]
        {
            SystemTypes.Electrical,
            SystemTypes.Comms,
            SystemTypes.MushroomMixupSabotage
        };

        return noneDeathSabotages.Any(IsSystemActive);
    }

    /// <summary>
    /// Checks if any sabotage (critical or non-critical) is currently active.
    /// </summary>
    /// <returns>True if any sabotage is active, false otherwise.</returns>
    internal static bool IsAnySabotageActive()
    {
        var allSabotages = new[]
        {
            SystemTypes.Electrical,
            SystemTypes.Reactor,
            SystemTypes.Laboratory,
            SystemTypes.LifeSupp,
            SystemTypes.HeliSabotage,
            SystemTypes.Comms,
            SystemTypes.MushroomMixupSabotage
        };

        return allSabotages.Any(IsSystemActive);
    }

    /// <summary>
    /// Gets whether the player is in a game.
    /// </summary>
    internal static bool IsInGame => InGame;

    /// <summary>
    /// Gets whether the player is in the lobby (joined but game not started).
    /// </summary>
    internal static bool IsLobby => AmongUsClient.Instance != null &&
        AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Joined;

    /// <summary>
    /// Gets whether the intro cutscene is currently playing.
    /// </summary>
    internal static bool IsInIntro => IntroCutscene.Instance != null;

    /// <summary>
    /// Gets whether gameplay is currently active (not in lobby, intro, or ended).
    /// </summary>
    internal static bool IsInGamePlay => (InGame && IsShip && !IsLobby && !IsInIntro) || IsFreePlay;

    /// <summary>
    /// Gets whether the game has ended.
    /// </summary>
    internal static bool IsEnded => AmongUsClient.Instance != null &&
        AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Ended;

    /// <summary>
    /// Gets whether the player is not joined to any game.
    /// </summary>
    internal static bool IsNotJoined => AmongUsClient.Instance != null &&
        AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.NotJoined;

    /// <summary>
    /// Gets whether the current game is an online game.
    /// </summary>
    internal static bool IsOnlineGame => AmongUsClient.Instance != null &&
        AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame;

    /// <summary>
    /// Gets whether the player is connected to a vanilla (official) server.
    /// </summary>
    internal static bool IsVanillaServer
    {
        get
        {
            if (!IsOnlineGame) return false;

            if (ServerManager.Instance == null || ServerManager.Instance.CurrentRegion == null)
                return false;

            string region = ServerManager.Instance.CurrentRegion.Name;
            return region == "North America" || region == "Europe" || region == "Asia";
        }
    }

    /// <summary>
    /// Gets whether the current game is a local game.
    /// </summary>
    internal static bool IsLocalGame => AmongUsClient.Instance != null &&
        AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame;

    /// <summary>
    /// Gets whether the current game is in free play mode.
    /// </summary>
    internal static bool IsFreePlay => AmongUsClient.Instance != null &&
        AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay;

    /// <summary>
    /// Gets whether the player is in a task (not in a meeting).
    /// </summary>
    internal static bool IsInTask => InGame && MeetingHud.Instance == null;

    /// <summary>
    /// Gets whether a meeting is currently active.
    /// </summary>
    internal static bool IsMeeting => InGame && MeetingHud.Instance != null;

    /// <summary>
    /// Gets whether voting is currently in progress during a meeting.
    /// </summary>
    internal static bool IsVoting
    {
        get
        {
            if (!IsMeeting || MeetingHud.Instance == null)
                return false;

            return MeetingHud.Instance.state is MeetingHud.VoteStates.Voted or MeetingHud.VoteStates.NotVoted;
        }
    }

    /// <summary>
    /// Gets whether the meeting is proceeding to results.
    /// </summary>
    internal static bool IsProceeding
    {
        get
        {
            if (!IsMeeting || MeetingHud.Instance == null)
                return false;

            return MeetingHud.Instance.state == MeetingHud.VoteStates.Proceeding;
        }
    }

    /// <summary>
    /// Gets whether a player is being exiled.
    /// </summary>
    internal static bool IsExilling
    {
        get
        {
            if (ExileController.Instance == null)
                return false;

            return !(AirshipIsActive && Minigame.Instance != null && Minigame.Instance.isActiveAndEnabled);
        }
    }

    /// <summary>
    /// Gets whether the game start countdown is active.
    /// </summary>
    internal static bool IsCountDown => GameStartManager.InstanceExists &&
        GameStartManager.Instance != null &&
        GameStartManager.Instance.startState == GameStartManager.StartingStates.Countdown;

    /// <summary>
    /// Gets whether a ship (map) is currently loaded.
    /// </summary>
    internal static bool IsShip => ShipStatus.Instance != null;

    /// <summary>
    /// Gets whether the local player is the host.
    /// </summary>
    internal static bool IsHost => AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost;

    /// <summary>
    /// Gets whether the lobby is private-only (requires specific settings).
    /// </summary>
    internal static bool IsPrivateOnlyLobby => (BAUConfigs.PrivateOnlyLobby.Value ||
        (AmongUsClient.Instance != null && AmongUsClient.Instance.AmLocalHost)) && IsHost;

    /// <summary>
    /// Gets whether the local player can move.
    /// </summary>
    internal static bool IsCanMove => PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.CanMove;

    /// <summary>
    /// Gets whether the local player is dead.
    /// </summary>
    internal static bool IsDead
    {
        get
        {
            if (PlayerControl.LocalPlayer == null)
                return false;

            var data = PlayerControl.LocalPlayer.Data;
            return data != null && data.IsDead;
        }
    }
}