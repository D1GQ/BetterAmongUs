namespace BetterAmongUs;

internal partial class BAUPlugin
{
    /// <summary>
    /// Contains constants That are used across BAU.
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        /// Example comment for BanPlayerList file.
        /// </summary>
        internal const string BAN_PLAYER_LIST_CONTENT = """
                                                        // Example ban entries (friend code and/or hashed PUID)
                                                        // Format: [FriendCode], [HashedPUID]
                                                        // Example with both:
                                                        // FriendCode#0000, abc123def456789
                                                        // Example with just friend code:
                                                        // FriendCode#0000
                                                        // Example with just hashed PUID:
                                                        // , hash123xyz789
                                                        """;

        /// <summary>
        /// Example comment for BanNameList file.
        /// </summary>
        internal const string BAN_NAME_LIST_CONTENT = """
                                                      // Example banned player name regex patterns
                                                      // Each pattern on a new line
                                                      // 
                                                      // ^TNT$
                                                      // ^hyde$
                                                      // ^[\P{IsBasicLatin}\s]+$
                                                      """;

        /// <summary>
        /// Example comment for BanChatList file.
        /// </summary>
        internal const string BAN_CHAT_LIST_CONTENT = """
                                                      // Example banned chat message regex patterns
                                                      // Each pattern on a new line
                                                      // 
                                                      // (?i)(?<!(?:say|said|don'?t|didn'?t|wr[io]te|\bthe\b|\bat\b).*)(?: |^)sta*r?t*(?:ing|\b)(?!.*ban)
                                                      // (?i)^go[go]*$");
                                                      // (?i)ni?gg?(?:a|er)
                                                      // (?i)sns(?-i).*[A-Z]{6}
                                                      // [A-Z]{6}.*(?i)sns
                                                      // (?i)(?:modded|expert|no cooldown) lobby(?-i).*[A-Z]{6}
                                                      // [A-Z]{6}.*(?i)(?:modded|expert|no cooldown) lobby
                                                      """;

        /// <summary>
        /// Legacy example comment for BanNameList file.
        /// </summary>
        internal const string BAN_NAME_LIST_CONTENT_LEGACY = """
                                                             // Example banned player names
                                                             // Each name on a new line - supports wildcards with **
                                                             // ** at start and end: contains anywhere
                                                             // ** at start only: ends with
                                                             // ** at end only: starts with
                                                             // No **: exact match (case-insensitive)
                                                             // 
                                                             // HackerPlayer123
                                                             // CheaterAccount
                                                             // **Bot**
                                                             // **Script
                                                             // Exploit**
                                                             // **Cheat**
                                                             """;

        internal const string WHITE_LIST_CONTENT = """
                                                   // Example whitelist entries (friend code and/or hashed PUID)
                                                   // Format: [FriendCode], [HashedPUID]
                                                   // Example with both:
                                                   // FriendCode#0000, abc123def456789
                                                   // Example with just friend code:
                                                   // FriendCode#0000
                                                   // Example with just hashed PUID:
                                                   // , hash123xyz789
                                                   """;

        internal const char ENCRYPTED_LOG_PREFIX = '\u2063';
        internal const char ENCRYPTED_LOG_POSTFIX = '\u2064';
        internal const int MAX_CHAT_TEXT = 120;
        internal const string BAU_CUSTOM_RPC_FLAG = "bau:rpc";
        internal const string BAU_MODDED_PROTOCOL_FLAG = "bau:flags";
    }
}