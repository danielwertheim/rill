using System;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;

namespace Rill.Stores.MongoDB
{
    internal class RillCommitDoc
    {
        internal Guid Id { get; }
        internal RillReferenceData Reference { get; }
        internal DateTimeOffset CommittedAt { get; }
        internal RillSequenceRangeData SequenceRange { get; }

        private RillCommitDoc(
            Guid id,
            RillReferenceData reference,
            DateTimeOffset committedAt,
            RillSequenceRangeData sequenceRange)
        {
            Id = id;
            Reference = reference;
            CommittedAt = committedAt;
            SequenceRange = sequenceRange;
        }

        internal static RillCommitDoc From(RillCommit commit)
        {
            var doc = new RillCommitDoc(
                (Guid)commit.Id,
                RillReferenceData.From(commit.Reference),
                commit.Timestamp,
                RillSequenceRangeData.From(commit.SequenceRange));

            // entity.Events.AddRange(commit.Events.Select(e => RillEventEntity.From(commit, e, serializer)));

            return doc;
        }

        // internal RillCommit ToCommit(IEventContentTypeResolver typeResolver, IEventContentSerializer serializer)
        // {
        //     var rillRef = RillReference.From(RillName, RillId);
        //     var id = CommitId.From(Id);
        //     var lower = Sequence.From(SequenceLowerBound);
        //     var upper = Sequence.From(SequenceUpperBound);
        //
        //     return RillCommit.From(
        //         rillRef,
        //         id,
        //         SequenceRange.From(lower, upper),
        //         Timestamp.From(CommittedAt.UtcDateTime),
        //         Events
        //             .Select(e => e.ToEvent(typeResolver, serializer))
        //             .ToImmutableList());
        // }

        internal static void ConfigureSchema() =>
            BsonClassMap.RegisterClassMap<RillCommitDoc>(schema =>
            {
                schema.AutoMap();
                schema
                    .MapIdMember(d => d.Id)
                    .SetIdGenerator(AscendingGuidGenerator.Instance);
            });
    }
}
