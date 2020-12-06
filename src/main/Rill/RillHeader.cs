using System;

namespace Rill
{
    public sealed class RillHeader
    {
        public RillReference Reference { get; }
        public Sequence Sequence { get; }
        public Timestamp CreatedAt { get; }
        public Timestamp LastChangedAt { get; }

        private RillHeader(
            RillReference reference,
            Sequence sequence,
            Timestamp createdAt,
            Timestamp lastChangedAt)
        {
            Reference = reference;
            Sequence = sequence;
            CreatedAt = createdAt;
            LastChangedAt = lastChangedAt;
        }

        public static RillHeader From(
            RillReference reference,
            Sequence sequence,
            Timestamp createdAt,
            Timestamp lastChangedAt)
        {
            if(lastChangedAt < createdAt)
                throw new ArgumentException("Last changed can not take presence before Created timestamp.");

            return new RillHeader(reference, sequence, createdAt, lastChangedAt);
        }

        public static RillHeader New(RillReference reference, Timestamp? timestamp = default)
        {
            var ts = timestamp ?? Timestamp.New();

            return new RillHeader(reference, Sequence.None, ts, ts);
        }
    }
}
