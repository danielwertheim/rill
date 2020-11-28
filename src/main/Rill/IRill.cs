using System;

namespace Rill
{
    public interface IRill<T> : IDisposable
    {
        /// <summary>
        /// Exposes the Rill as a stream of <typeparamref name="{T}"/>.
        /// </summary>
        IRillConsumable<T> Consume { get; }

        /// <summary>
        /// Exposes the Rill as a stream of anything.
        /// </summary>
        IRillConsumable<object> ConsumeAny { get; }

        /// <summary>
        /// Emits passed event info to all consumers.
        /// </summary>
        /// <remarks>
        /// - If ONE consumer fails, ALL consumers will be notified of the failing event.
        /// - If ALL consumers handles the event without throwing, consumers will be notified of successful event.
        /// - Any failures during successful notification and will cause Emit to throw.
        /// </remarks>
        /// <param name="content"></param>
        /// <param name="id"></param>
        /// <param name="sequence"></param>
        /// <returns>Emitted event.</returns>
        Event<T> Emit(T content, EventId? id = null, Sequence? sequence = null);

        /// <summary>
        /// Completes the Rill and signals all active consumers.
        /// </summary>
        void Complete();
    }
}
