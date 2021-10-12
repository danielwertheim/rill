using System;
using System.Linq;

namespace Rill
{
    public sealed record RillReference
    {
        private readonly string _reference;
        private readonly int _hashCode;

        public string Name { get; }
        public string Id { get; }

        private RillReference(string name, string id)
        {
            _reference = $"{name}:{id}";
            _hashCode = StringComparer.OrdinalIgnoreCase.GetHashCode(_reference);

            Name = name;
            Id = id;
        }

        private static string EnsureValidName(string name)
            => string.IsNullOrWhiteSpace(name)
                ? throw new ArgumentException("A Name must be provided. An empty string is not allowed.", nameof(name))
                : name.All(char.IsLetter)
                    ? name
                    : throw new ArgumentException("Name can only contain letters.", nameof(name));

        private static string EnsureValidId(string id)
            => string.IsNullOrWhiteSpace(id)
                ? throw new ArgumentException("An Id must be provided. An empty string is not allowed.", nameof(id))
                : id.All(char.IsLetterOrDigit)
                    ? id
                    : throw new ArgumentException("Id can only contain letters or digits.", nameof(id));

        public static RillReference From(string name, string id)
            => new(EnsureValidName(name), EnsureValidId(id));

        public static RillReference From(string value)
        {
            var parts = value.Split(':');
            if (parts.Length != 2)
                throw new ArgumentException(
                    "When reconstructing from a single string value, the string must consist of exactly one Name part and one Id part.",
                    nameof(value));
            return From(parts[0], parts[1]);
        }

        public static RillReference New(string name)
            => From(name, Guid.NewGuid().ToString("N"));

        public static explicit operator string(RillReference reference)
            => reference._reference;

        public bool Equals(RillReference? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return string.Equals(_reference, other._reference, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
            => _hashCode;

        public override string ToString()
            => _reference;
    }
}
