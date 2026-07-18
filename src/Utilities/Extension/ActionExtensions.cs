using UnityEngine.Events;

namespace BetterAmongUs.Utilities.Extension;

/// <summary>
/// Provides extension methods for UnityEvent classes to simplify adding listeners with Action delegates.
/// </summary>
internal static class ActionExtensions
{
    /// <summary>
    /// Adds a listener to the UnityEvent that invokes the specified action when the event is raised.
    /// </summary>
    /// <param name="unityEvent">The UnityEvent to add the listener to.</param>
    /// <param name="action">The action to invoke when the event is raised.</param>
    internal static void AddListener(this UnityEvent unityEvent, Action action)
    {
        unityEvent.AddListener(action);
    }

    /// <summary>
    /// Adds a listener to the UnityEvent&lt;T0&gt; that invokes the specified action when the event is raised.
    /// </summary>
    /// <typeparam name="T0">The type of the first event parameter.</typeparam>
    /// <param name="unityEvent">The UnityEvent&lt;T0&gt; to add the listener to.</param>
    /// <param name="action">The action to invoke when the event is raised, accepting one parameter of type T0.</param>
    internal static void AddListener<T0>(this UnityEvent<T0> unityEvent, Action<T0> action)
    {
        unityEvent.AddListener(action);
    }

    /// <summary>
    /// Adds a listener to the UnityEvent&lt;T0, T1&gt; that invokes the specified action when the event is raised.
    /// </summary>
    /// <typeparam name="T0">The type of the first event parameter.</typeparam>
    /// <typeparam name="T1">The type of the second event parameter.</typeparam>
    /// <param name="unityEvent">The UnityEvent&lt;T0, T1&gt; to add the listener to.</param>
    /// <param name="action">The action to invoke when the event is raised, accepting two parameters of types T0 and T1.</param>
    internal static void AddListener<T0, T1>(this UnityEvent<T0, T1> unityEvent, Action<T0, T1> action)
    {
        unityEvent.AddListener(action);
    }

    /// <summary>
    /// Adds a listener to the UnityEvent&lt;T0, T1, T2&gt; that invokes the specified action when the event is raised.
    /// </summary>
    /// <typeparam name="T0">The type of the first event parameter.</typeparam>
    /// <typeparam name="T1">The type of the second event parameter.</typeparam>
    /// <typeparam name="T2">The type of the third event parameter.</typeparam>
    /// <param name="unityEvent">The UnityEvent&lt;T0, T1, T2&gt; to add the listener to.</param>
    /// <param name="action">The action to invoke when the event is raised, accepting three parameters of types T0, T1, and T2.</param>
    internal static void AddListener<T0, T1, T2>(this UnityEvent<T0, T1, T2> unityEvent, Action<T0, T1, T2> action)
    {
        unityEvent.AddListener(action);
    }

    /// <summary>
    /// Adds a listener to the UnityEvent&lt;T0, T1, T2, T3&gt; that invokes the specified action when the event is raised.
    /// </summary>
    /// <typeparam name="T0">The type of the first event parameter.</typeparam>
    /// <typeparam name="T1">The type of the second event parameter.</typeparam>
    /// <typeparam name="T2">The type of the third event parameter.</typeparam>
    /// <typeparam name="T3">The type of the fourth event parameter.</typeparam>
    /// <param name="unityEvent">The UnityEvent&lt;T0, T1, T2, T3&gt; to add the listener to.</param>
    /// <param name="action">The action to invoke when the event is raised, accepting four parameters of types T0, T1, T2, and T3.</param>
    internal static void AddListener<T0, T1, T2, T3>(this UnityEvent<T0, T1, T2, T3> unityEvent, Action<T0, T1, T2, T3> action)
    {
        unityEvent.AddListener(action);
    }
}