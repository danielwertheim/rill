using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Rill
{
    public static class RillCommit
    {
        public static IRillCommit<T> New<T>(
            RillReference reference,
            IImmutableList<Event<T>> events) => RillCommit<T>.New(reference, events);

        public static IRillCommit<T> From<T>(
            CommitId id,
            RillReference reference,
            Revision revision,
            Timestamp timestamp,
            IImmutableList<Event<T>> events) => RillCommit<T>.From(id, reference, revision, timestamp, events);
    }

    internal sealed class RillCommit<T> : IRillCommit<T>
    {
        public CommitId Id { get; }
        public RillReference Reference { get; }
        public Revision Revision { get; }
        public Timestamp Timestamp { get; }
        public IImmutableList<Event<T>> Events { get; }

        private RillCommit(
            CommitId id,
            RillReference reference,
            Revision revision,
            Timestamp timestamp,
            IImmutableList<Event<T>> events)
        {
            if (!events.Any())
                throw new ArgumentException("A commit must at least contain one event.", nameof(events));

            Id = id;
            Reference = reference;
            Revision = revision;
            Timestamp = timestamp;
            Events = events;
        }

        private static IImmutableList<Event<T>> RequireAtLeastOneEvent(IImmutableList<Event<T>> events)
        {
            if (!events.Any())
                throw new ArgumentException("A commit must at least contain one event.", nameof(events));

            return events;
        }

        internal static RillCommit<T> From(
            CommitId id,
            RillReference reference,
            Revision revision,
            Timestamp timestamp,
            IImmutableList<Event<T>> events)
        {
            return new RillCommit<T>(id, reference, revision, timestamp, RequireAtLeastOneEvent(events));
        }

        internal static RillCommit<T> New(
            RillReference reference,
            IImmutableList<Event<T>> events)
        {
            events = RequireAtLeastOneEvent(events);

            var revision = Revision.From(events[0].Sequence, events[^1].Sequence);

            return From(CommitId.New(), reference, revision, Timestamp.New(), events);
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public IEnumerator<Event<T>> GetEnumerator()
            => Events.GetEnumerator();
    }
}
