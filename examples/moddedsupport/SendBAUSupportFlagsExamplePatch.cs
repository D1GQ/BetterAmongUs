using HarmonyLib;
using Hazel;

namespace BetterAmongUs.Examples;

/// <summary>
/// Example patch that demonstrates how to send BetterAmongUs modded support flags to BAU users.
/// This patch hooks into the player info serialization process to transmit custom flags that control various BAU features.
/// </summary>
[HarmonyPatch]
internal static class SendBAUSupportFlagsExamplePatch
{
    /// <summary>
    /// Postfix patch that runs after NetworkedPlayerInfo.Serialize.
    /// When the host is serializing the player data, this method writes
    /// BAU support flags into the message stream to communicate mod capabilities to the BAU client.
    /// </summary>
    /// <param name="__instance">The NetworkedPlayerInfo instance being serialized.</param>
    /// <param name="writer">The MessageWriter used for serialization.</param>
    /// <remarks>
    /// This example sends the "command.force.bau.prefix" flag which forces all BAU commands
    /// to use the "bau:" prefix. The flag is hashed using GetFlagHash for efficient transmission.
    /// <para>
    /// <b>Important:</b> This functionality only works on protocol version +25 (modded protocol)!
    /// </para>
    /// </remarks>
    [HarmonyPatch(typeof(NetworkedPlayerInfo), nameof(NetworkedPlayerInfo.Serialize))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    internal static void Serialize_Postfix(NetworkedPlayerInfo __instance, MessageWriter writer)
    {
        if (!AmongUsClient.Instance.AmHost || __instance.OwnerId != -2)
            return;

        writer.Write("bau:flags"); // Mark message as BAU flags
        writer.WritePacked(1); // Amount of flags we are sending
        writer.Write(GetFlagHash("command.force.bau.prefix")); // Send flag hash, must use GetFlagHash with the exact logic in this example!
    }

    /// <summary>
    /// Generates a deterministic integer hash from a string for use as a flag identifier.
    /// </summary>
    /// <param name="input">The input string to hash</param>
    /// <returns>
    /// A 32-bit integer hash value. Returns 0 for null or empty strings.
    /// </returns>
    internal static int GetFlagHash(string input)
    {
        if (string.IsNullOrEmpty(input)) return 0;

        int hash = 17;
        hash = hash * 31 + input.Length;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            hash = hash * 31 + c;
            hash = hash * 31 + i;
        }

        hash = hash * 31 + input.Length;
        return hash;
    }
}
