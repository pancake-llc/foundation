namespace Pancake
{
    /// <summary>
    /// Represents a mechanism for triggering an event.
    /// </summary>
    public interface IEventTrigger
    {
        /// <summary>
        /// Trigger the event.
        /// </summary>
        void Trigger();
    }

    /// <summary>
    /// Represents a mechanism for triggering an event with an argument of type <typeparamref name="TArgument"/>.
    /// </summary>
    /// <typeparam name="TArgument">
    /// Type of the argument that gets passed to all listener methods when the event occurs.
    /// </typeparam>
    public interface IEventTrigger<in TArgument>
    {
        /// <summary>
        /// Trigger the event.
        /// </summary>
        /// <param name="argument"> Object related to the event. </param>
        void Trigger(TArgument argument);
    }
}