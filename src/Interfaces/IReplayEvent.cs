namespace BetterAmongUs.Interfaces;

/// <summary>
/// Base interface for all replay events.
/// </summary>
public interface IReplayEvent
{
    /// <summary>
    /// Base record for all replay event data.
    /// </summary>
    public abstract record Data;

    /// <summary>
    /// Base record for all replay event arguments.
    /// </summary>
    public abstract record Args;
}

/// <summary>
/// Generic interface for replay events with typed data and arguments.
/// </summary>
/// <typeparam name="EventDataType">The type of event data. Must inherit from <see cref="IReplayEvent.Data"/>.</typeparam>
/// <typeparam name="EventArgType">The type of event arguments. Must inherit from <see cref="IReplayEvent.Args"/>.</typeparam>
public interface IReplayEvent<EventDataType, EventArgType> : IReplayEvent
    where EventDataType : IReplayEvent.Data
    where EventArgType : IReplayEvent.Args
{
    /// <summary>
    /// Gets the unique identifier for the event type.
    /// </summary>
    /// <value>A string that uniquely identifies this type of event.</value>
    string Id { get; }

    /// <summary>
    /// Gets the stored event data.
    /// </summary>
    /// <value>The event data of type <typeparamref name="EventDataType"/>, or null if not recorded.</value>
    EventDataType? EventData { get; }

    /// <summary>
    /// Records the event using the provided arguments.
    /// </summary>
    /// <param name="replayEventArg">The arguments used to construct the event data.</param>
    void Record(EventArgType replayEventArg);

    /// <summary>
    /// Replays the event using the stored event data.
    /// </summary>
    void Play();

    /// <summary>
    /// Undoes the effects of the replayed event.
    /// </summary>
    void Undo();
}