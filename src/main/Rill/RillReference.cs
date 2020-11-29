using System;

namespace Rill
{
    public sealed class RillReference : IEquatable<RillReference>
    {
        private readonly string _name;
        private readonly Guid _id;
        private readonly int _hashCode;

        public string Name => _name;
        public Guid Id => _id;

        private static int GenerateHashCode(Guid id, string name)
        {
            var hashCode = new HashCode();

            hashCode.Add(name, StringComparer.OrdinalIgnoreCase);
            hashCode.Add(id);

            return hashCode.ToHashCode();
        }

        private RillReference(string name, Guid id)
        {
            _name = name;
            _id = id;
            _hashCode = GenerateHashCode(id, name);
        }

        public static RillReference From(string name, Guid id)
        {
            if (name == string.Empty)
                throw new ArgumentException("An empty string is not allowed.", nameof(name));

            if (id == Guid.Empty)
                throw new ArgumentException("An empty GUID is not allowed.", nameof(id));

            return new RillReference(name, id);
        }

        private static bool IdEquals(RillReference left, RillReference right)
            => left._id.Equals(right._id);

        private static bool NameEquals(RillReference left, RillReference right)
            => string.Equals(left._name, right._name, StringComparison.OrdinalIgnoreCase);

        public static RillReference New(string name)
            => From(name, Guid.NewGuid());

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
            => $"{_name}:{_id:N}";
    }
}
