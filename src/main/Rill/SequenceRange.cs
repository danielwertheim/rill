using System;

namespace Rill
{
    public sealed class SequenceRange : IEquatable<SequenceRange>
    {
        public Sequence Lower { get; }
        public Sequence Upper { get; }

        public static readonly SequenceRange Any = new(Sequence.First, Sequence.Max);

        private SequenceRange(Sequence lower, Sequence upper)
        {
            Lower = lower;
            Upper = upper;
        }

        public static SequenceRange From(Sequence lower, Sequence upper)
        {
            if(upper < lower)
                throw new ArgumentException("Range must be from low to high.");

            return new SequenceRange(lower, upper);
        }

        public static SequenceRange FirstTo(Sequence upper)
            => From(Sequence.First, upper);

        public bool Includes(Sequence value)
            => value >= Lower && value <= Upper;

        public static bool operator ==(SequenceRange? left, SequenceRange? right)
            => Equals(left, right);

        public static bool operator !=(SequenceRange? left, SequenceRange? right)
            => !Equals(left, right);

        public bool Equals(SequenceRange? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Lower.Equals(other.Lower) && Upper.Equals(other.Upper);
        }

        public override bool Equals(object? obj)
            => ReferenceEquals(this, obj) || obj is SequenceRange other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(Lower, Upper);

        public override string ToString()
            => $"|{Lower} -> {Upper}|";
    }
}
