using System;

namespace Rill
{
    public sealed class RillDetails
    {
        public RillReference Reference { get; }
        public Sequence Sequence { get; }
        public Timestamp CreatedAt { get; }
        public Timestamp LastChangedAt { get; }

        private RillDetails(
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

        public static RillDetails From(
            RillReference reference,
            Sequence sequence,
            Timestamp createdAt,
            Timestamp lastChangedAt)
        {
            if(lastChangedAt < createdAt)
                throw new ArgumentException("Last changed can not take presence before Created timestamp.");

            return new RillDetails(reference, sequence, createdAt, lastChangedAt);
        }

        public static RillDetails New(RillReference reference, Timestamp? timestamp = default)
        {
            var ts = timestamp ?? Timestamp.New();

            return new RillDetails(reference, Sequence.None, ts, ts);
        }
    }
}
