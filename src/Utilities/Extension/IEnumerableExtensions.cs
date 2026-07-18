using Il2CppSystem.Linq;

namespace BetterAmongUs.Utilities.Extension;

/// <summary>
/// Provides extension methods for IEnumerable collections, including shuffling, selecting a random element, and retrieving the middle element.
/// </summary>
internal static class IEnumerableExtensions
{
    private static readonly Random rng = new();

    /// <summary>
    /// Shuffles all elements in a collection randomly.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to shuffle.</param>
    /// <returns>The shuffled collection.</returns>
    internal static IEnumerable<T> Shuffle<T>(this IEnumerable<T> collection)
    {
        var list = collection.ToList();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
        return list;
    }

    /// <summary>
    /// Selects a random non-null element from the collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to select from.</param>
    /// <returns>A random non-null element from the collection, or <c>null</c> if the collection is empty or only contains null values.</returns>
    internal static T? Random<T>(this IEnumerable<T?> collection) where T : class
    {
        if (collection == null || !collection.Any()) return null;

        var shuffled = collection.Where(item => item != null).Shuffle();
        return shuffled.FirstOrDefault();
    }

    /// <summary>
    /// Selects a random non-null element from the collection and returns both the element and its original index.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to select from.</param>
    /// <returns>A tuple containing:
    /// - The random non-null element (or null if no valid elements found)
    /// - The original index of the element (or -1 if no valid elements found)
    /// </returns>
    internal static (T? element, int index) RandomIndex<T>(this IEnumerable<T?> collection) where T : class
    {
        if (collection == null || !collection.Any())
            return (null, -1);

        var indexedItems = collection
            .Select((item, index) => (item, index))
            .Where(x => x.item != null)
            .ToList();

        if (indexedItems.Count == 0)
            return (null, -1);

        var shuffled = indexedItems.Shuffle();
        var selected = shuffled.First();

        return (selected.item, selected.index);
    }

    /// <summary>
    /// Retrieves the middle element of a collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to retrieve the middle element from.</param>
    /// <returns>The middle element of the collection.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the collection is null or empty.</exception>
    internal static T Middle<T>(this IEnumerable<T> collection)
    {
        if (collection == null || !collection.Any())
            throw new InvalidOperationException("Collection cannot be null or empty.");

        int middleIndex = collection.Count() / 2;
        return collection.Skip(middleIndex).First();
    }
}