using System;
using System.Collections.Generic;

namespace Rill
{
    public sealed class EventSequence :
        IEquatable<EventSequence>,
        IComparable<EventSequence>,
        IFormattable
    {
        private readonly long _value;

        public static readonly EventSequence Any = new EventSequence(-1);
        public static readonly EventSequence None = new EventSequence(0);
        public static readonly EventSequence First = new EventSequence(1);
        public static readonly EventSequence Max = new EventSequence(long.MaxValue);

        private EventSequence(long value) => _value = value;

        private static EventSequence Create(long eventSequence)
        {
            if (eventSequence < 0)
                throw new ArgumentOutOfRangeException(nameof(eventSequence), eventSequence, "Sequence can not be lower than 0.");

            return new EventSequence(eventSequence);
        }

        public EventSequence Increment()
            => new EventSequence(_value + 1);

        public EventSequence Add(long change)
        {
            if (change <= 0)
                throw new ArgumentOutOfRangeException(nameof(change), change, "Change can not be lower than 1.");

            return new EventSequence(_value + change);
        }

        public bool IsFirst() => this == First;
        public bool IsMax() => this == Max;

        public bool IsBetweenInclusive(long minInclusive, long maxInclusive)
            => _value >= minInclusive && _value <= maxInclusive;

        public static explicit operator long(EventSequence id)
            => id._value;

        public static explicit operator EventSequence(long value)
            => Create(value);

        public static bool operator ==(EventSequence left, EventSequence right)
            => Equals(left, right);

        public static bool operator !=(EventSequence left, EventSequence right)
            => !Equals(left, right);

        public static bool operator <(EventSequence left, EventSequence right) =>
            Comparer<EventSequence>.Default.Compare(left, right) < 0;

        public static bool operator >(EventSequence left, EventSequence right) =>
            Comparer<EventSequence>.Default.Compare(left, right) > 0;

        public static bool operator <=(EventSequence left, EventSequence right) =>
            Comparer<EventSequence>.Default.Compare(left, right) <= 0;

        public static bool operator >=(EventSequence left, EventSequence right) =>
            Comparer<EventSequence>.Default.Compare(left, right) >= 0;

        public int CompareTo(EventSequence? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return _value.CompareTo(other._value);
        }

        public bool Equals(EventSequence? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _value.Equals(other._value);
        }

        public override bool Equals(object? obj)
            => ReferenceEquals(this, obj) || obj is EventSequence other && Equals(other);

        public override int GetHashCode()
            => _value.GetHashCode();

        public override string ToString()
            => _value.ToString();

        public string ToString(string? format, IFormatProvider? formatProvider)
            => _value.ToString(format, formatProvider);
    }
}
