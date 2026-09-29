namespace BetterAmongUs.Enums;

[Flags]
internal enum AntiCheatFlags
{
    None = 0,
    Low = 1 << 0,
    Medium = 1 << 1,
    High = 1 << 2,
    Cancel = 1 << 3,
}