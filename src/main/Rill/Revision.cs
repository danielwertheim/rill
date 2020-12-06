using System;

namespace Rill
{
    public sealed class Revision : IEquatable<Revision>
    {
        public Sequence Lower { get; }
        public Sequence Upper { get; }

        private Revision(Sequence lower, Sequence upper)
        {
            Lower = lower;
            Upper = upper;
        }

        public static Revision From(Sequence lower, Sequence upper)
        {
            if(upper < lower)
                throw new ArgumentException("Range must be from low to high.");

            return new Revision(lower, upper);
        }

        public bool Includes(Sequence value)
            => value >= Lower && value <= Upper;

        public static bool operator ==(Revision? left, Revision? right)
            => Equals(left, right);

        public static bool operator !=(Revision? left, Revision? right)
            => !Equals(left, right);

        public bool Equals(Revision? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Lower.Equals(other.Lower) && Upper.Equals(other.Upper);
        }

        public override bool Equals(object? obj)
            => ReferenceEquals(this, obj) || obj is Revision other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(Lower, Upper);

        public override string ToString()
            => $"|{Lower} -> {Upper}|";
    }
}
