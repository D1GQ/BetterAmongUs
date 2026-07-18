namespace BetterAmongUs.Modules;

/// <summary>
/// Tracks a value and records the last time it was changed.
/// </summary>
/// <typeparam name="T">The type of value to track.</typeparam>
internal sealed class ValueTracker<T>
{
    private T? _currentValue;
    private T? _previousTrackedValue;
    private float _lastChangeTime;

    /// <summary>
    /// Gets the current tracked value.
    /// </summary>
    public T? Value => _currentValue;

    /// <summary>
    /// Gets the time elapsed since the last value change.
    /// </summary>
    internal float TimeSinceLastChange => UnityEngine.Time.time - _lastChangeTime;

    /// <summary>
    /// Updates the tracked value if it differs from the current value.
    /// </summary>
    /// <param name="newValue">The new value to set.</param>
    internal void Update(T newValue)
    {
        _currentValue = newValue;

        if (!Equals(newValue, _previousTrackedValue))
        {
            _previousTrackedValue = newValue;
            _lastChangeTime = UnityEngine.Time.time;
        }
    }
}