using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rill
{
    public interface IAsyncRill<T> : IDisposable
    {
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
        /// - If ONE consumer fails, consumers will be notified of the failing event.
        /// - If ALL consumer handles without exception, consumers will be notified of successful event.
        ///   Halts on any failures during successful notification and will cause Emit to throw.
        /// </remarks>
        /// <param name="content"></param>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Emitted event.</returns>
        ValueTask<Event<T>> EmitAsync(T content, EventId? id = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Completes the Rill and signals all active consumers.
        /// </summary>
        void Complete();
    }
}
