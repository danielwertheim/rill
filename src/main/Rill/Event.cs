using System;

namespace Rill
{
    public sealed class Event<T> : IEquatable<Event<T>>
    {
        public EventId Id { get; }
        public EventSequence Sequence { get; }
        public T Content { get; }

        public Event(EventId id, EventSequence sequence, T content)
        {
            Id = id;
            Sequence = sequence;
            Content = content;
        }

        public Event<TResult> Map<TResult>(Func<T, TResult> map)
            => new Event<TResult>(Id, Sequence, map(Content));

        public bool TryDownCast<TResult>(out Event<TResult>? ev)
        {
            if (Content is TResult result)
            {
                ev = new Event<TResult>(Id, Sequence, result);

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
