﻿using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rill.Core;
using Rill.Stores.EfCore.Serialization;
using Rill.Stores.EfCore.Extensions;

namespace Rill.Stores.EfCore
{
    public class EfCoreRillStore : IRillStore
    {
        private readonly DbContextOptions<RillDbContext> _options;
        private readonly IEventContentSerializer _eventContentSerializer;
        private readonly IEventContentTypeResolver _eventContentTypeResolver;

        public EfCoreRillStore(
            DbContextOptions<RillDbContext> options,
            IEventContentSerializer? eventSerializer = default,
            IEventContentTypeResolver? typeResolver = default)
        {
            //https://docs.microsoft.com/en-us/ef/core/dbcontext-configuration/#simple-dbcontext-initialization-with-new
            _options = options;
            _eventContentSerializer = eventSerializer ?? new JsonEventContentSerializer();
            _eventContentTypeResolver = typeResolver ?? new DefaultEventContentTypeResolver();
        }

        private RillDbContext GetContext()
            => new(_options);

        public async Task<RillDetails?> GetDetailsAsync(RillReference reference, CancellationToken cancellationToken = default)
        {
            await using var dbContext = GetContext();

            var rill = await dbContext.Rills
                .IgnoreAutoIncludes()
                .AsNoTracking()
                .SingleOrDefaultAsync(r => r.Name == reference.Name && r.Id == reference.Id, cancellationToken)
                .ConfigureAwait(false);

            return rill?.ToDetails();
        }

        public async Task AppendAsync(RillCommit commit, CancellationToken cancellationToken = default)
        {
            var commitEntity = RillCommitEntity.From(commit, _eventContentSerializer);

            await using var dbContext = GetContext();

            var rillEntity = await dbContext.Rills
                .IgnoreAutoIncludes()
                .SingleOrDefaultAsync(r => r.Name == commitEntity.RillName && r.Id == commitEntity.RillId, cancellationToken)
                .ConfigureAwait(false);

            if (rillEntity == null)
            {
                if (commit.SequenceRange.Lower != Sequence.First)
                    throw Exceptions.StoreIsMissingRillWhenWriting(commit.Reference);

                rillEntity = RillEntity.New(commit.Reference, commit.SequenceRange, commit.Timestamp);

                await dbContext.Rills.AddAsync(rillEntity, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var expectedNextSeq = rillEntity.Sequence + 1;
                if (commit.SequenceRange.Lower != expectedNextSeq)
                    throw Exceptions.StoreConcurrency(commit.Reference, Sequence.From(rillEntity.Sequence), commit.SequenceRange.Lower);

                rillEntity.Register(commitEntity);
            }

            await dbContext.Commits.AddAsync(commitEntity, cancellationToken).ConfigureAwait(false);

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(RillReference reference, CancellationToken cancellationToken = default)
        {
            await using var dbContext = GetContext();

            var rill = await dbContext.Rills
                .SingleOrDefaultAsync(r => r.Name == reference.Name && r.Id == reference.Id, cancellationToken)
                .ConfigureAwait(false);

            if (rill == null)
                return;

            dbContext.Rills.Remove(rill);

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public IEnumerable<RillCommit> ReadCommits(RillReference reference, SequenceRange? sequenceRange = default)
        {
            //https://docs.microsoft.com/en-us/ef/core/performance/efficient-querying#project-only-properties-you-need
            using var dbContext = GetContext();

            var commits = dbContext.Commits
                .AsNoTracking()
                .Matching(reference, sequenceRange ?? SequenceRange.Any)
                .Include(c => c.Events.OrderBy(e => e.Sequence))
                .AsSplitQuery()
                .Select(c => c.ToCommit(_eventContentTypeResolver, _eventContentSerializer));

            foreach (var commit in commits)
                yield return commit;
        }

        public async IAsyncEnumerable<RillCommit> ReadCommitsAsync(
            RillReference reference,
            SequenceRange? sequenceRange = default,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await using var dbContext = GetContext();

            var commits = dbContext.Commits
                .AsNoTracking()
                .Matching(reference, sequenceRange ?? SequenceRange.Any)
                .Include(c => c.Events.OrderBy(e => e.Sequence))
                .AsSplitQuery()
                .AsAsyncEnumerable()
                .WithCancellation(cancellationToken)
                .ConfigureAwait(false);

            await using var enumerator = commits.GetAsyncEnumerator();
            while (await enumerator.MoveNextAsync())
            {
                yield return enumerator.Current.ToCommit(_eventContentTypeResolver, _eventContentSerializer);
            }
        }
    }
}
