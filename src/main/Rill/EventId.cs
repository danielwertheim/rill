using System;

namespace Rill
{
    public sealed class EventId :
        IEquatable<EventId>,
        IFormattable
    {
        private readonly Guid _value;

        private EventId(Guid value) => _value = value;

        public static EventId New() => new EventId(Guid.NewGuid());

        public static EventId From(Guid value)
        {
            if (value == Guid.Empty)
                throw new ArgumentException("Empty GUID not allowed.", nameof(value));

            return new EventId(value);
        }

        public static explicit operator Guid(EventId id) => id._value;

        public static bool operator ==(EventId left, EventId right)
            => left._value == right._value;

        public static bool operator !=(EventId left, EventId right)
            => left._value != right._value;

        public static bool operator ==(EventId left, Guid right)
            => left._value == right;

        public static bool operator !=(EventId left, Guid right)
            => left._value != right;

        public bool Equals(EventId? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _value.Equals(other._value);
        }

        public override bool Equals(object? obj)
            => ReferenceEquals(this, obj) || obj is EventId other && Equals(other);

        public override int GetHashCode()
            => _value.GetHashCode();

        public override string ToString()
            => _value.ToString();

        public string ToString(string? format, IFormatProvider? formatProvider)
            => _value.ToString(format, formatProvider);
    }
}
