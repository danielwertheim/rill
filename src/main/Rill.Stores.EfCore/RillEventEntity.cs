using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rill.Core;
using Rill.Stores.EfCore.Serialization;

namespace Rill.Stores.EfCore
{
    internal class RillEventEntity
    {
        internal Guid Id { get; }
        internal Guid CommitId { get; }
        internal string TypeAssemblyName { get; }
        internal string TypeNamespace { get; }
        internal string TypeName { get; }
        internal long Sequence { get; }
        internal DateTimeOffset Timestamp { get; }
        internal string Content { get; }

        private RillEventEntity(
            Guid id,
            Guid commitId,
            string typeAssemblyName,
            string typeNamespace,
            string typeName,
            long sequence,
            DateTimeOffset timestamp,
            string content)
        {
            Id = id;
            CommitId = commitId;
            Sequence = sequence;
            Timestamp = timestamp;
            TypeAssemblyName = typeAssemblyName;
            TypeNamespace = typeNamespace;
            TypeName = typeName;
            Content = content;
        }

        internal static RillEventEntity From(RillCommit commit, Event e, IEventContentSerializer contentSerializer)
        {
            var ct = EventContentType.From(e.Content.GetType());

            return new RillEventEntity(
                (Guid) e.Id,
                (Guid) commit.Id,
                ct.AssemblyName,
                ct.Namespace,
                ct.Name,
                (long) e.Sequence,
                (DateTime) e.Timestamp,
                contentSerializer.Serialize(e.Content));
        }

        internal Event ToEvent(IEventContentTypeResolver eventContentTypeResolver, IEventContentSerializer contentSerializer)
        {
            var ct = new EventContentType(TypeAssemblyName, TypeNamespace, TypeName);
            var t = eventContentTypeResolver.Resolve(ct);

            return Event.From(
                EventId.From(Id),
                Rill.Sequence.From(Sequence),
                Rill.Timestamp.From(Timestamp.UtcDateTime),
                contentSerializer.Deserialize(Content, t));
        }
    }

    internal class RillEventEntityConfiguration : IEntityTypeConfiguration<RillEventEntity>
    {
        public void Configure(EntityTypeBuilder<RillEventEntity> builder)
        {
            builder
                .ToTable("RillEvent");
            builder
                .HasKey(i => i.Id)
                .HasName("PK_RillEvent");
            builder
                .HasIndex(i => i.TypeName);
            builder
                .HasIndex(i => i.Sequence);
            // builder
            //     .HasOne<RillEntity>()
            //     .WithMany()
            //     .HasForeignKey(i => i.RillId)
            //     .HasConstraintName("FK_RillEvent_Rill")
            //     .OnDelete(DeleteBehavior.NoAction);
            builder
                .HasOne<RillCommitEntity>()
                .WithMany(i => i.Events)
                .HasForeignKey(i => i.CommitId)
                .HasConstraintName("FK_RillEvent_RillCommit")
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .Property(i => i.Id)
                .IsRequired()
                .ValueGeneratedNever();
            // builder
            //     .Property(i => i.RillId)
            //     .IsRequired()
            //     .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder
                .Property(i => i.CommitId)
                .IsRequired()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder
                .Property(i => i.TypeAssemblyName)
                .IsUnicode(false)
                .HasMaxLength(128)
                .IsRequired()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder
                .Property(i => i.TypeNamespace)
                .IsUnicode(false)
                .HasMaxLength(128)
                .IsRequired()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder
                .Property(i => i.TypeName)
                .IsUnicode(false)
                .HasMaxLength(32)
                .IsRequired()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder
                .Property(i => i.Sequence)
                .IsRequired()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder
                .Property(i => i.Timestamp)
                .IsRequired()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder
                .Property(i => i.Content)
                .IsUnicode()
                .IsRequired()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        }
    }
}
