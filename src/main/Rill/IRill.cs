using System;

namespace Rill
{
    public interface IRill : IRillConsumable<Event>
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
        /// <returns>Emitted event.</returns>
        Event<T> Emit<T>(T content, EventId? id = null, Sequence? sequence = null) where T : class;

        /// <summary>
        /// Emits passed event to all consumers.
        /// </summary>
        /// <param name="ev"></param>
        void Emit(Event ev);

        /// <summary>
        /// Emits all events in the passed commit to all consumers.
        /// </summary>
        /// <param name="commit"></param>
        void Emit(RillCommit commit);
    }
}
