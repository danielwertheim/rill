using System;

namespace Rill
{
    public sealed class Event : Event<object>
    {
        private Event(
            EventId id,
            Sequence sequence,
            Timestamp timestamp,
            object content) : base(id, sequence, timestamp, content)
        {
        }

        public new static Event From(EventId id, Sequence sequence, Timestamp timestamp, object content)
            => new(
                id,
                sequence,
                timestamp,
                content);

        public new static Event New(object content, EventId? id = null, Sequence? sequence = null, Timestamp? timestamp = null)
            => From(
                id ?? EventId.New(),
                sequence ?? Sequence.First,
                timestamp ?? Timestamp.New(),
                content);
    }

    public class Event<T> : IEquatable<Event<T>> where T : class
    {
        public EventId Id { get; }
        public Timestamp Timestamp { get; }
        public Sequence Sequence { get; }
        public T Content { get; }

        protected Event(
            EventId id,
            Sequence sequence,
            Timestamp timestamp,
            T content)
        {
            Id = id;
            Sequence = sequence;
            Timestamp = timestamp;
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public static Event<T> From(EventId id, Sequence sequence, Timestamp timestamp, T content)
            => new(
                id,
                sequence,
                timestamp,
                content);

        public static Event<T> New(T content, EventId? id = null, Sequence? sequence = null, Timestamp? timestamp = null)
            => From(
                id ?? EventId.New(),
                sequence ?? Sequence.First,
                timestamp ?? Timestamp.New(),
                content);

        public Event<TResult> MapContent<TResult>(Func<T, TResult> map) where TResult : class
            => new(Id, Sequence, Timestamp, map(Content));

        public Event AsUntyped()
            => Event.From(Id, Sequence, Timestamp, Content);

        public bool TryCast<TResult>(out Event<TResult>? ev) where TResult : class
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
