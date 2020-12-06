using System;

namespace Rill
{
    public sealed class CommitId :
        IEquatable<CommitId>,
        IFormattable
    {
        private readonly Guid _value;

        private CommitId(Guid value) => _value = value;

        public static CommitId New() => new CommitId(Guid.NewGuid());

        public static CommitId From(Guid value)
        {
            if (value == Guid.Empty)
                throw new ArgumentException("Empty GUID is not allowed.", nameof(value));

            return new CommitId(value);
        }

        public static explicit operator Guid(CommitId id) => id._value;

        public static bool operator ==(CommitId left, CommitId right)
            => left._value == right._value;

        public static bool operator !=(CommitId left, CommitId right)
            => left._value != right._value;

        public static bool operator ==(CommitId left, Guid right)
            => left._value == right;

        public static bool operator !=(CommitId left, Guid right)
            => left._value != right;

        public bool Equals(CommitId? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _value.Equals(other._value);
        }

        public override bool Equals(object? obj)
            => ReferenceEquals(this, obj) || obj is CommitId other && Equals(other);

        public override int GetHashCode()
            => _value.GetHashCode();

        public override string ToString()
            => _value.ToString("N");

        public string ToString(string? format, IFormatProvider? formatProvider)
            => _value.ToString(format, formatProvider);
    }
}
