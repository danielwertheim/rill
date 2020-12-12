using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Rill
{
    /// Defines a commit which represents a batch of events,
    /// that was persisted together against an <see cref="IRillStore"/>.
    public sealed class RillCommit : IEnumerable<Event>
    {
        /// <summary>
        /// Gets the unique id for the commit.
        /// </summary>
        public CommitId Id { get; }

        /// <summary>
        /// Gets the reference to the Rill that the commit
        /// belongs to.
        /// </summary>
        public RillReference Reference { get; }

        /// <summary>
        /// Gets the sequence range of the contained events.
        /// From-inclusive - To-inclusive.
        /// </summary>
        public SequenceRange SequenceRange { get; }

        /// <summary>
        /// Gets the timestamp for when the commit occured.
        /// </summary>
        public Timestamp Timestamp { get; }

        /// <summary>
        /// Gets the events associated with the commit.
        /// </summary>
        public IImmutableList<Event> Events { get; }

        private RillCommit(
            RillReference reference,
            CommitId id,
            SequenceRange sequenceRange,
            Timestamp timestamp,
            IImmutableList<Event> events)
        {
            Id = id;
            Reference = reference;
            SequenceRange = sequenceRange;
            Timestamp = timestamp;
            Events = events;
        }

        private static IImmutableList<Event> RequireAtLeastOneEvent(IImmutableList<Event> events)
        {
            if (!events.Any())
                throw new ArgumentException("A commit must at least contain one event.", nameof(events));

            return events;
        }

        public static RillCommit From(
            RillReference reference,
            CommitId id,
            SequenceRange sequenceRange,
            Timestamp timestamp,
            IImmutableList<Event> events) => new(reference, id, sequenceRange, timestamp, RequireAtLeastOneEvent(events));

        public static RillCommit New(
            RillReference reference,
            IImmutableList<Event> events)
        {
            events = RequireAtLeastOneEvent(events);

            var revision = SequenceRange.From(events[0].Sequence, events[^1].Sequence);

            return From(reference, CommitId.New(), revision, Timestamp.New(), events);
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public IEnumerator<Event> GetEnumerator()
            => Events.GetEnumerator();

        public override string ToString()
            => $"{Reference}_{Id}_{SequenceRange}_{Timestamp}";
    }
}
