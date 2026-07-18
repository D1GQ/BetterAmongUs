namespace BetterAmongUs.Enums;

/// <summary>
/// Defines custom Remote Procedure Call (RPC) identifiers used in BetterAmongUs for cheat detection and communication.
/// </summary>
internal enum CustomRPC : int
{
    // ========================================
    //            Known Cheat RPCS
    // ========================================

    // Cheat RPC's
    /// <summary>
    /// RPC identifier for Sicko cheat detection.
    /// </summary>
    Sicko = 420, // Results in 164

    /// <summary>
    /// RPC identifier for AUM (Among Us Menu) cheat detection.
    /// </summary>
    AUM = 42069, // Results in 85

    /// <summary>
    /// RPC identifier for AUM chat communication.
    /// </summary>
    AUMChat = 101,

    /// <summary>
    /// RPC identifier for KillNetwork cheat detection.
    /// </summary>
    KillNetwork = 250,

    /// <summary>
    /// RPC identifier for KillNetwork chat communication.
    /// </summary>
    KillNetworkChat = 119,

    /// <summary>
    /// RPC identifier for ModMenuCrew cheat detection.
    /// </summary>
    ModMenuCrew = 202,

    /// <summary>
    /// RPC identifier for ModMenuCrew chat communication.
    /// </summary>
    ModMenuCrewChat = 201,

    // ========================================
    //                BAU RPCS
    // ========================================

    // Better Among Us
    /// <summary>
    /// Legacy RPC for BetterAmongUs checks (currently unused).
    /// </summary>
    LegacyBetterCheck = 150, // Unused

    /// <summary>
    /// RPC for sending a shared secret to another player.
    /// </summary>
    SendSecretToPlayer = 151,

    /// <summary>
    /// RPC for checking the hash of a shared secret received from another player.
    /// </summary>
    CheckSecretHashFromPlayer = 152,
}