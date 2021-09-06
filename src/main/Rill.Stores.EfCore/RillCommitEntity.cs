using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rill.Core;
using Rill.Stores.EfCore.Serialization;

namespace Rill.Stores.EfCore
{
    internal class RillCommitEntity
    {
        internal Guid Id { get; }
        internal string RillName { get; }
        internal string RillId { get; }
        internal DateTimeOffset CommittedAt { get; }
        internal long SequenceLowerBound { get; }
        internal long SequenceUpperBound { get; }
        internal List<RillEventEntity> Events { get; set; } = new();

        private RillCommitEntity(
            Guid id,
            string rillName,
            string rillId,
            DateTimeOffset committedAt,
            long sequenceLowerBound,
            long sequenceUpperBound)
        {
            Id = id;
            RillName = rillName;
            RillId = rillId;
            CommittedAt = committedAt;
            SequenceLowerBound = sequenceLowerBound;
            SequenceUpperBound = sequenceUpperBound;
        }

        internal static RillCommitEntity From(RillCommit commit, IEventContentSerializer serializer)
        {
            var entity = new RillCommitEntity(
                (Guid) commit.Id,
                commit.Reference.Name,
                commit.Reference.Id,
                commit.Timestamp,
                (long) commit.SequenceRange.Lower,
                (long) commit.SequenceRange.Upper);

            entity.Events.AddRange(commit.Events.Select(e => RillEventEntity.From(commit, e, serializer)));

            return entity;
        }

        internal RillCommit ToCommit(IEventContentTypeResolver typeResolver, IEventContentSerializer serializer)
        {
            var rillRef = RillReference.From(RillName, RillId);
            var id = CommitId.From(Id);
            var lower = Sequence.From(SequenceLowerBound);
            var upper = Sequence.From(SequenceUpperBound);

            return RillCommit.From(
                rillRef,
                id,
                SequenceRange.From(lower, upper),
                Timestamp.From(CommittedAt.UtcDateTime),
                Events
                    .Select(e => e.ToEvent(typeResolver, serializer))
                    .ToImmutableList());
        }
    }

    internal class RillCommitEntityConfiguration : IEntityTypeConfiguration<RillCommitEntity>
    {
        public void Configure(EntityTypeBuilder<RillCommitEntity> builder)
        {
            builder.ToTable("RillCommit");

            builder
                .HasKey(i => i.Id)
                .HasName("PK_RillCommit");

            builder.HasIndex(i => i.SequenceLowerBound);
            builder.HasIndex(i => i.SequenceUpperBound);
            builder.HasIndex(i => new { i.RillName, i.RillId });

            builder
                .HasOne<RillEntity>()
                .WithMany()
                .HasForeignKey(i => new { i.RillName, i.RillId})
                .HasConstraintName("FK_RillCommit_Rill")
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .Property(i => i.Id)
                .IsRequired()
                .ValueGeneratedNever()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            builder
                .Property(i => i.RillName)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(32)
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            builder
                .Property(i => i.RillId)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(32)
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            builder
                .Property(i => i.CommittedAt)
                .IsRequired()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            builder
                .Property(i => i.SequenceLowerBound)
                .IsRequired()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            builder
                .Property(i => i.SequenceUpperBound)
                .IsRequired()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        }
    }
}
