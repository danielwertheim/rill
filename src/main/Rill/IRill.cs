namespace Rill
{
    public interface IRill<T> : IRillConsumable<T>
    {
        /// <summary>
        /// Emits passed event info to all consumers.
        /// </summary>
        /// <remarks>
        /// - If ONE consumer fails, consumers will be notified of the failing event.
        /// - If ALL consumer handles without exception, consumers will be notified of successful event.
        ///   Halts on any failures during successful notification and will cause Emit to throw.
        /// </remarks>
        /// <param name="content"></param>
        /// <param name="id"></param>
        /// <returns>Emitted event.</returns>
        Event<T> Emit(T content, EventId? id = null);

        /// <summary>
        /// Completes the Rill and signals all active consumers.
        /// </summary>
        void Complete();
    }
}
