namespace BetterAmongUs.Utilities.Extension;

/// <summary>
/// Provides extension methods for safe type casting operations.
/// </summary>
internal static class CastExtensions
{
    /// <summary>
    /// Checks if an object can be cast to the specified type.
    /// </summary>
    /// <typeparam name="T">The target type to check.</typeparam>
    /// <param name="obj">The object to check.</param>
    /// <returns>True if the object can be cast to type T, false otherwise.</returns>
    internal static bool TryCast<T>(this object obj) => obj is T;

    /// <summary>
    /// Attempts to cast an object to the specified reference type.
    /// </summary>
    /// <typeparam name="T">The target reference type.</typeparam>
    /// <param name="obj">The object to cast.</param>
    /// <param name="item">The cast object if successful, or null if not.</param>
    /// <returns>True if the cast was successful, false otherwise.</returns>
    internal static bool TryCast<T>(this object obj, out T? item) where T : class
    {
        if (obj != null && obj is T casted)
        {
            item = casted;
            return true;
        }
        else
        {
            item = null;
            return false;
        }
    }
}