using System;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;

namespace Rill.Stores.MongoDB
{
    internal class RillDoc
    {
        internal string Id { get; }
        internal RillReferenceData Reference { get; }
        internal long Sequence { get; }
        internal DateTimeOffset CreatedAt { get; }
        internal DateTimeOffset LastChangedAt { get; }

        private RillDoc(
            string id,
            RillReferenceData reference,
            long sequence,
            DateTimeOffset createdAt,
            DateTimeOffset lastChangedAt)
        {
            Id = id;
            Reference = reference;
            Sequence = sequence;
            CreatedAt = createdAt;
            LastChangedAt = lastChangedAt;
        }

        internal static RillDoc From(RillReference reference, SequenceRange sequenceRange, Timestamp createdAt, Timestamp lastChangedAt)
            => new((string)reference, RillReferenceData.From(reference), (long)sequenceRange.Upper, createdAt, lastChangedAt);

        internal static RillDoc New(RillReference reference, SequenceRange sequenceRange, Timestamp createdAt)
            => From(reference, sequenceRange, createdAt, createdAt);

        internal RillDetails ToDetails() =>
            RillDetails.From(
                RillReference.From(Reference.Name, Reference.Id),
                Rill.Sequence.From(Sequence),
                Timestamp.From(CreatedAt.UtcDateTime),
                Timestamp.From(LastChangedAt.UtcDateTime));

        internal static void ConfigureSchema() =>
            BsonClassMap.RegisterClassMap<RillDoc>(schema =>
            {
                schema.AutoMap();
                schema
                    .MapIdMember(d => d.Id)
                    .SetIdGenerator(NullIdChecker.Instance);
            });
    }
}
