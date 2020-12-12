using System;

namespace Rill
{
    public sealed class RillReference : IEquatable<RillReference>
    {
        private readonly int _hashCode;

        public string Name { get; }
        public Guid Id { get; }

        private static int GenerateHashCode(string name, Guid id)
        {
            var hashCode = new HashCode();

            hashCode.Add(name, StringComparer.OrdinalIgnoreCase);
            hashCode.Add(id);

            return hashCode.ToHashCode();
        }

        private RillReference(string name, Guid id)
        {
            Name = name;
            Id = id;
            _hashCode = GenerateHashCode(name, id);
        }

        public static RillReference From(string name, Guid id)
        {
            if (name == string.Empty)
                throw new ArgumentException("An empty string is not allowed.", nameof(name));

            if (id == Guid.Empty)
                throw new ArgumentException("An empty GUID is not allowed.", nameof(id));

            return new RillReference(name, id);
        }

        public static RillReference New(string name)
            => From(name, Guid.NewGuid());

        private static bool IdEquals(RillReference left, RillReference right)
            => left.Id.Equals(right.Id);

        private static bool NameEquals(RillReference left, RillReference right)
            => string.Equals(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);

        public static bool operator ==(RillReference left, RillReference right)
            => IdEquals(left, right) && NameEquals(left, right);

        public static bool operator !=(RillReference left, RillReference right)
            => !IdEquals(left, right) || !NameEquals(left, right);

        public bool Equals(RillReference? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return IdEquals(this, other) && NameEquals(this, other);
        }

        public override bool Equals(object? obj)
            => ReferenceEquals(this, obj) || obj is RillReference other && Equals(other);

        public override int GetHashCode()
            => _hashCode;

        public override string ToString()
            => $"{Name}:{Id:N}";
    }
}
