using System;
using System.Collections.Generic;

namespace Rill
{
    public sealed class Timestamp :
        IEquatable<Timestamp>,
        IComparable<Timestamp>,
        IFormattable
    {
        private readonly DateTime _value;

        private Timestamp(DateTime value) => _value = value;

        public static Timestamp New() => new(DateTime.UtcNow);

        public static Timestamp From(DateTime value)
        {
            if (value == default)
                throw new ArgumentOutOfRangeException(nameof(value), value, "A date time must be specified.");

            if (value.Kind != DateTimeKind.Utc)
                throw new ArgumentException("Only UTC date times are valid as timestamps.");

            return new Timestamp(value);
        }

        public static implicit operator DateTime(Timestamp t) => t._value;
        public static implicit operator DateTimeOffset(Timestamp t) => t._value;

        public static bool operator ==(Timestamp left, Timestamp right)
            => left._value == right._value;

        public static bool operator !=(Timestamp left, Timestamp right)
            => left._value != right._value;

        public static bool operator ==(Timestamp left, DateTime right)
            => left._value == right;

        public static bool operator !=(Timestamp left, DateTime right)
            => left._value != right;

        public static bool operator <(Timestamp left, Timestamp right) =>
            Comparer<Timestamp>.Default.Compare(left, right) < 0;

        public static bool operator >(Timestamp left, Timestamp right) =>
            Comparer<Timestamp>.Default.Compare(left, right) > 0;

        public static bool operator <=(Timestamp left, Timestamp right) =>
            Comparer<Timestamp>.Default.Compare(left, right) <= 0;

        public static bool operator >=(Timestamp left, Timestamp right) =>
            Comparer<Timestamp>.Default.Compare(left, right) >= 0;

        public int CompareTo(Timestamp? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return _value.CompareTo(other._value);
        }

        public bool Equals(Timestamp? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _value.Equals(other._value);
        }

        public override bool Equals(object? obj)
            => ReferenceEquals(this, obj) || obj is Timestamp other && Equals(other);

        public override int GetHashCode()
            => _value.GetHashCode();

        public override string ToString()
            => _value.ToString("O");

        public string ToString(string? format, IFormatProvider? formatProvider)
            => _value.ToString(format, formatProvider);
    }
}
