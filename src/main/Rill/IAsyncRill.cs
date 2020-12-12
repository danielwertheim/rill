using System.Threading;
using System.Threading.Tasks;

namespace Rill
{
    public interface IAsyncRill : IAsyncRillConsumable<Event>
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
        /// Emits passed event info to all consumers.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="id"></param>
        /// <param name="sequence"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Emitted event.</returns>
        ValueTask<Event<T>> EmitAsync<T>(T content, EventId? id = null, Sequence? sequence = null, CancellationToken cancellationToken = default)
            where T : class;

        /// <summary>
        /// Emits passed event to all consumers.
        /// </summary>
        /// <param name="ev"></param>
        /// <param name="cancellationToken"></param>
        Task EmitAsync(Event ev, CancellationToken cancellationToken = default);

        /// <summary>
        /// Emits all events in the passed commit to all consumers.
        /// </summary>
        /// <param name="commit"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task EmitAsync(RillCommit commit, CancellationToken cancellationToken = default);
    }
}
