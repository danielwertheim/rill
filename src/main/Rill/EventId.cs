using System;

namespace Rill
{
    public sealed class EventId :
        IEquatable<EventId>,
        IFormattable
    {
        private readonly Guid _value;

        public EventId(Guid value)
        {
            _value = value != Guid.Empty
                ? value
                : throw new ArgumentException("Empty GUID not allowed.", nameof(value));
        }

        public static EventId New() => new EventId(Guid.NewGuid());

        public static explicit operator Guid(EventId id) => id._value;

        public static explicit operator EventId(Guid value) => new EventId(value);

        public static bool operator ==(EventId left, EventId right)
            => Equals(left, right);

        public static bool operator !=(EventId left, EventId right)
            => !Equals(left, right);

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

        public string ToString(string? format, IFormatProvider? formatProvider)
            => _value.ToString(format, formatProvider);
    }
}
