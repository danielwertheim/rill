using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Rill.Stores.EfCore
{
    internal class RillEntity
    {
        internal string Name { get; }
        internal Guid Id { get; }
        internal long LastSequence { get; private set; }
        internal DateTimeOffset CreatedAt { get; }
        internal DateTimeOffset LastChangedAt { get; private set; }

        private RillEntity(
            string name,
            Guid id,
            long lastSequence,
            DateTimeOffset createdAt,
            DateTimeOffset lastChangedAt)
        {
            Name = name;
            Id = id;
            LastSequence = lastSequence;
            CreatedAt = createdAt;
            LastChangedAt = lastChangedAt;
        }

        internal static RillEntity From(RillReference reference, SequenceRange sequenceRange, Timestamp createdAt, Timestamp lastChangedAt)
            => new(reference.Name, reference.Id, (long) sequenceRange.Upper, createdAt, lastChangedAt);

        internal static RillEntity New(RillReference reference, SequenceRange sequenceRange, Timestamp createdAt)
            => From(reference, sequenceRange, createdAt, createdAt);

        internal void SetSequence(long value)
            => LastSequence = value;

        internal void SetLastChangedAt(DateTimeOffset value)
            => LastChangedAt = value;

        internal RillDetails ToDetails() =>
            RillDetails.From(
                RillReference.From(Name, Id),
                Sequence.From(LastSequence),
                Timestamp.From(CreatedAt.UtcDateTime),
                Timestamp.From(LastChangedAt.UtcDateTime));
    }

    internal class RillEntityConfiguration : IEntityTypeConfiguration<RillEntity>
    {
        public void Configure(EntityTypeBuilder<RillEntity> builder)
        {
            builder
                .ToTable("Rill");
            builder
                .HasKey(i => new {i.Name, i.Id})
                .HasName("PK_Rill");
            builder
                .HasIndex(i => i.Name);
            builder
                .HasIndex(i => i.LastSequence);

            builder
                .Property(i => i.Id)
                .IsRequired()
                .ValueGeneratedNever();
            builder
                .Property(i => i.Name)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(32);
            builder
                .Property(i => i.LastSequence)
                .IsRequired()
                .IsConcurrencyToken();
            builder
                .Property(i => i.CreatedAt)
                .IsRequired()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder
                .Property(i => i.LastChangedAt)
                .IsRequired();
        }
    }
}
