namespace BetterAmongUs.Utilities;

/// <summary>
/// Provides utility methods for working with enumerations.
/// </summary>
internal static class EnumUtils
{
    /// <summary>
    /// Gets all values of an enumeration type.
    /// </summary>
    /// <typeparam name="T">The enumeration type.</typeparam>
    /// <returns>An array containing all values of the enumeration type, or null if the type is not an enum.</returns>
    internal static T[]? GetAllValues<T>() where T : Enum => Enum.GetValues(typeof(T)) as T[];

    /// <summary>
    /// Gets all names of an enumeration type.
    /// </summary>
    /// <typeparam name="T">The enumeration type.</typeparam>
    /// <returns>An array containing all names of the enumeration type.</returns>
    internal static string[] GetAllNames<T>() where T : Enum => Enum.GetNames(typeof(T));

    /// <summary>
    /// Splits all values in the enumeration into equal-sized chunks.
    /// </summary>
    /// <typeparam name="TEnum">The enumeration type.</typeparam>
    /// <param name="chunkSize">The number of elements each chunk should contain.</param>
    /// <param name="shuffle">Whether to randomize the order of values before chunking.</param>
    /// <param name="exclude">Optional predicate to exclude certain enumeration values.</param>
    /// <returns>A list of arrays, each containing up to chunkSize elements of the enumeration type.</returns>
    internal static List<TEnum[]> Achunk<TEnum>(int chunkSize, bool shuffle = false, Func<TEnum, bool>? exclude = null) where TEnum : Enum
    {
        List<TEnum[]> chunkedList = [];
        TEnum[]? allValues = GetAllValues<TEnum>();
        var rnd = new Random();
        if (shuffle) allValues = [.. allValues.Shuffle(rnd)];
        if (exclude != null) allValues = [.. allValues.Where(exclude)];

        for (int i = 0; i < allValues.Length; i += chunkSize)
        {
            TEnum[] chunk = new TEnum[Math.Min(chunkSize, allValues.Length - i)];
            Array.Copy(allValues, i, chunk, 0, chunk.Length);
            chunkedList.Add(chunk);
        }

        return chunkedList;
    }
}