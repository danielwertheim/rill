using System;
using System.Collections.Generic;

namespace Rill
{
    public sealed class Sequence :
        IEquatable<Sequence>,
        IComparable<Sequence>,
        IFormattable
    {
        private readonly long _value;

        public static readonly Sequence None = new(0);
        public static readonly Sequence First = new(1);
        public static readonly Sequence Max = new(long.MaxValue);

        private Sequence(long value) => _value = value;

        public static Sequence From(long sequence)
        {
            if (sequence < 0)
                throw new ArgumentOutOfRangeException(nameof(sequence), sequence, "Sequence can not be lower than 0.");

            return new Sequence(sequence);
        }

        public Sequence Add(long change)
        {
            if (change <= 0)
                throw new ArgumentOutOfRangeException(nameof(change), change, "Change can not be lower than 1.");

            return From(_value + change);
        }

        public Sequence Increment()
            => Add(1);

        public Sequence Copy()
            => From(_value);

        public bool IsNone()
            => this == None;

        public bool IsFirst()
            => this == First;

        public bool IsMax()
            => this == Max;

        public bool IsBetweenInclusive(long minInclusive, long maxInclusive)
            => _value >= minInclusive && _value <= maxInclusive;

        public static explicit operator long(Sequence id)
            => id._value;

        public static bool operator ==(Sequence? left, Sequence? right)
            => left?._value == right?._value;

        public static bool operator !=(Sequence? left, Sequence? right)
            => left?._value != right?._value;

        public static bool operator ==(Sequence left, long right)
            => left._value == right;

        public static bool operator !=(Sequence left, long right)
            => left._value != right;

        public static long operator %(Sequence left, long right)
            => left._value % right;

        public static bool operator <(Sequence left, Sequence right) =>
            Comparer<Sequence>.Default.Compare(left, right) < 0;

        public static bool operator >(Sequence left, Sequence right) =>
            Comparer<Sequence>.Default.Compare(left, right) > 0;

        public static bool operator <=(Sequence left, Sequence right) =>
            Comparer<Sequence>.Default.Compare(left, right) <= 0;

        public static bool operator >=(Sequence left, Sequence right) =>
            Comparer<Sequence>.Default.Compare(left, right) >= 0;

        public int CompareTo(Sequence? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return _value.CompareTo(other._value);
        }

        public bool Equals(Sequence? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _value.Equals(other._value);
        }

        public override bool Equals(object? obj)
            => ReferenceEquals(this, obj) || obj is Sequence other && Equals(other);

        public override int GetHashCode()
            => _value.GetHashCode();

        public override string ToString()
            => _value.ToString();

        public string ToString(string? format, IFormatProvider? formatProvider)
            => _value.ToString(format, formatProvider);
    }
}
