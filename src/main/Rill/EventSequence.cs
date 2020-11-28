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

        public static readonly EventSequence None = new EventSequence(0);
        public static readonly EventSequence First = new EventSequence(1);
        public static readonly EventSequence Max = new EventSequence(long.MaxValue);

        private EventSequence(long value) => _value = value;

        public static EventSequence From(long eventSequence)
        {
            if (eventSequence < 0)
                throw new ArgumentOutOfRangeException(nameof(eventSequence), eventSequence, "Sequence can not be lower than 0.");

            return new EventSequence(eventSequence);
        }

        public EventSequence Add(long change)
        {
            if (change <= 0)
                throw new ArgumentOutOfRangeException(nameof(change), change, "Change can not be lower than 1.");

            return From(_value + change);
        }

        public EventSequence Increment()
            => Add(1);

        public bool IsNone() => this == None;
        public bool IsFirst() => this == First;
        public bool IsMax() => this == Max;

        public bool IsBetweenInclusive(long minInclusive, long maxInclusive)
            => _value >= minInclusive && _value <= maxInclusive;

        public static explicit operator long(EventSequence id)
            => id._value;

        public static bool operator ==(EventSequence left, EventSequence right)
            => left._value == right._value;

        public static bool operator !=(EventSequence left, EventSequence right)
            => left._value != right._value;

        public static bool operator ==(EventSequence left, long right)
            => left._value == right;

        public static bool operator !=(EventSequence left, long right)
            => left._value != right;

        public static bool operator <(EventSequence left, EventSequence right) =>
            Comparer<EventSequence>.Default.Compare(left, right) < 0;

        public static bool operator >(EventSequence left, EventSequence right) =>
            Comparer<EventSequence>.Default.Compare(left, right) > 0;

        public static bool operator <=(EventSequence left, EventSequence right) =>
            Comparer<EventSequence>.Default.Compare(left, right) <= 0;

        public static bool operator >=(EventSequence left, EventSequence right) =>
            Comparer<EventSequence>.Default.Compare(left, right) >= 0;

        public static long operator %(EventSequence left, long right)
            => left._value % right;

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
