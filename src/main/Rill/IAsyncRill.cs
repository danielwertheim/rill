using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rill
{
    public interface IAsyncRill<T> : IDisposable
    {
        /// <summary>
        /// Gets the Rill reference which identifies a Rill.
        /// </summary>
        RillReference Reference { get; }

        /// <summary>
        /// Gets the current sequence.
        /// </summary>
        Sequence Sequence { get; }

        /// <summary>
        /// Exposes the Rill as a stream of <typeparamref name="{T}"/>.
        /// </summary>
        IAsyncRillConsumable<T> Consume { get; }

        /// <summary>
        /// Exposes the Rill as a stream of anything.
        /// </summary>
        IAsyncRillConsumable<object> ConsumeAny { get; }

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
        /// <param name="cancellationToken"></param>
        /// <returns>Emitted event.</returns>
        ValueTask<Event<T>> EmitAsync(T content, EventId? id = null, Sequence? sequence = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Completes the Rill and signals all active consumers.
        /// </summary>
        void Complete();
    }
}
