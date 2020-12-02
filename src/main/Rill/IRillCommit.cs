using System.Collections.Generic;
using System.Collections.Immutable;

namespace Rill
{
    /// <summary>
    /// Defines a commit which represents a batch of events,
    /// that was persisted together against an <see cref="IRillStore{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRillCommit<T> : IEnumerable<Event<T>>
    {
        CommitId Id { get; }
        RillReference Reference { get; }
        Revision Revision { get; }
        Timestamp Timestamp { get; }
        IImmutableList<Event<T>> Events { get; }
    }
}
