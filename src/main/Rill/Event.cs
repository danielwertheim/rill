using System;

namespace Rill
{
    public static class Event
    {
        public static Event<T> Create<T>(T content, EventId? id = null, EventSequence? sequence = null, Timestamp? timestamp = null)
            => Event<T>.Create(content, id, sequence, timestamp);
    }

    public sealed class Event<T> : IEquatable<Event<T>>
    {
        public EventId Id { get; }

        public Timestamp Timestamp { get; }
        public EventSequence Sequence { get; } //TODO: Should probably be removed and owned by the rills and perhaps named RillSequence. OnNew(ev, rillSequence)
        public T Content { get; }

        private Event(
            EventId id,
            EventSequence sequence,
            Timestamp timestamp,
            T content)
        {
            Id = id;
            Sequence = sequence;
            Timestamp = timestamp;
            Content = content;
        }

        public static Event<T> Create(T content, EventId? id = null, EventSequence? sequence = null, Timestamp? timestamp = null)
            => new Event<T>(
                id ?? EventId.New(),
                sequence ?? EventSequence.First, //TODO: Eeek
                timestamp ?? Timestamp.New(),
                content);

        public Event<TResult> Map<TResult>(Func<T, TResult> map)
            => new Event<TResult>(Id, Sequence, Timestamp, map(Content));

        public bool TryCast<TResult>(out Event<TResult>? ev)
        {
            if (Content is TResult result)
            {
                ev = new Event<TResult>(Id, Sequence, Timestamp, result);

                return true;
            }

            ev = default;

            return false;
        }

        public static bool operator ==(Event<T>? left, Event<T>? right)
            => Equals(left, right);

        public static bool operator !=(Event<T>? left, Event<T>? right)
            => !Equals(left, right);

        public bool Equals(Event<T>? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id);
        }

        public override bool Equals(object? obj)
            => ReferenceEquals(this, obj) || obj is Event<T> other && Equals(other);

        public override int GetHashCode() => Id.GetHashCode();
    }
}
