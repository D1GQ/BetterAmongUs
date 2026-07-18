using Il2CppInterop.Runtime.InteropTypes;

namespace BetterAmongUs.Utilities.Extension;

/// <summary>
/// Provides extension methods for working with Il2Cpp collections in a LINQ-like manner.
/// </summary>
internal static class Il2CppExtensions
{
    /// <summary>
    /// Performs the specified action on each element of an Il2Cpp IEnumerable.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source collection.</param>
    /// <param name="action">The action to perform on each element.</param>
    internal static void ForEachIl2Cpp<T>(this Il2CppSystem.Collections.Generic.IEnumerable<T> source, Action<T> action)
    {
        if (source == null || action == null)
            return;

        var list = new Il2CppSystem.Collections.Generic.List<T>(source);
        for (int i = 0; i < list.Count; i++)
        {
            action(list[i]);
        }
    }

    /// <summary>
    /// Returns the first element that satisfies a condition, or a default value if no such element is found.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source collection.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>The first element that satisfies the condition, or default(T) if no such element is found.</returns>
    internal static T? FirstOrDefaultIl2Cpp<T>(this Il2CppSystem.Collections.Generic.IEnumerable<T> source, Func<T, bool> predicate)
    {
        if (source == null || predicate == null) return default;

        var list = new Il2CppSystem.Collections.Generic.List<T>(source);
        for (int i = 0; i < list.Count; i++)
        {
            var item = list[i];
            if (item != null && predicate(item))
                return item;
        }

        return default;
    }

    /// <summary>
    /// Performs the specified action on each element of an Il2Cpp List (more efficient than IEnumerable version).
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source list.</param>
    /// <param name="action">The action to perform on each element.</param>
    internal static void ForEachIl2Cpp<T>(this Il2CppSystem.Collections.Generic.List<T> source, Action<T> action)
    {
        if (source == null || action == null)
            return;

        // Direct iteration - no conversion needed!
        for (int i = 0; i < source.Count; i++)
        {
            action(source[i]);
        }
    }

    /// <summary>
    /// Returns the first element of an Il2Cpp List that satisfies a condition, or a default value if no such element is found.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source list.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>The first element that satisfies the condition, or default(T) if no such element is found.</returns>
    internal static T? FirstOrDefaultIl2Cpp<T>(this Il2CppSystem.Collections.Generic.List<T> source, Func<T, bool> predicate)
    {
        if (source == null || predicate == null) return default;

        // Direct iteration - no conversion needed!
        for (int i = 0; i < source.Count; i++)
        {
            var item = source[i];
            if (item != null && predicate(item))
                return item;
        }

        return default;
    }

    /// <summary>
    /// Determines whether any element of an Il2Cpp List satisfies a condition.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="list">The source list.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>true if any elements in the source list pass the test in the predicate; otherwise, false.</returns>
    public static bool AnyIl2Cpp<T>(this Il2CppSystem.Collections.Generic.List<T> list, Func<T, bool> predicate)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (predicate(list[i])) return true;
        }
        return false;
    }

    /// <summary>
    /// Returns a number that represents how many elements in the Il2Cpp List satisfy a condition.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source list.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>A number that represents how many elements in the list satisfy the condition.</returns>
    internal static int CountIl2Cpp<T>(this Il2CppSystem.Collections.Generic.List<T> source, Func<T, bool>? predicate = null)
    {
        if (source == null) return 0;

        int count = 0;
        for (int i = 0; i < source.Count; i++)
        {
            if (predicate == null || predicate(source[i]))
                count++;
        }
        return count;
    }

    /// <summary>
    /// Filters an Il2Cpp List based on a predicate.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source list.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>A List<T> that contains elements from the input list that satisfy the condition.</returns>
    internal static List<T> WhereIl2Cpp<T>(this Il2CppSystem.Collections.Generic.List<T> source, Func<T, bool> predicate)
    {
        if (source == null || predicate == null) return [];

        var result = new List<T>();
        for (int i = 0; i < source.Count; i++)
        {
            var item = source[i];
            if (predicate(item))
                result.Add(item);
        }
        return result;
    }

    /// <summary>
    /// Determines whether all elements of an Il2Cpp List satisfy a condition.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source list.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>true if every element of the source list passes the test in the predicate, or if the list is empty; otherwise, false.</returns>
    internal static bool AllIl2Cpp<T>(this Il2CppSystem.Collections.Generic.List<T> source, Func<T, bool> predicate)
    {
        if (source == null || predicate == null) return false;

        for (int i = 0; i < source.Count; i++)
        {
            if (!predicate(source[i]))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Determines whether an Il2Cpp List contains a specific Il2Cpp object.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source list.</param>
    /// <param name="value">The value to locate in the list.</param>
    /// <returns>true if the list contains an element that equals the specified value; otherwise, false.</returns>
    internal static bool ContainsIl2Cpp<T>(this Il2CppSystem.Collections.Generic.List<T> source, T value) where T : Il2CppObjectBase
    {
        if (source == null) return false;

        for (int i = 0; i < source.Count; i++)
        {
            if (source[i]?.Equals(value) == true)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Projects each element of an Il2Cpp List into a new form.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source list.</typeparam>
    /// <typeparam name="TResult">The type of the value returned by the selector function.</typeparam>
    /// <param name="source">The source list.</param>
    /// <param name="selector">A transform function to apply to each element.</param>
    /// <returns>A List<TResult> whose elements are the result of invoking the transform function on each element of source.</returns>
    internal static List<TResult> SelectIl2Cpp<T, TResult>(this Il2CppSystem.Collections.Generic.List<T> source, Func<T, TResult> selector)
    {
        if (source == null || selector == null) return [];

        var result = new List<TResult>();
        for (int i = 0; i < source.Count; i++)
        {
            result.Add(selector(source[i]));
        }
        return result;
    }
}