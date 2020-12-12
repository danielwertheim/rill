namespace Rill
{
    /// <summary>
    /// Defines a consumer which can consume events from a <see cref="IRillConsumable{T}"/> implementation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRillConsumer<in T>
    {
        /// <summary>
        /// Invoked when the Rill gets a new event.
        /// </summary>
        /// <param name="value"></param>
        void OnNew(T value);

        /// <summary>
        /// Invoked when the event has been dispatched and handled
        /// by all consumers without causing any errors.
        /// </summary>
        /// <param name="eventId"></param>
        void OnAllSucceeded(EventId eventId);

        /// <summary>
        /// Invoked when the event caused a failure in any consumer.
        /// </summary>
        /// <param name="eventId"></param>
        void OnAnyFailed(EventId eventId);
    }
}
