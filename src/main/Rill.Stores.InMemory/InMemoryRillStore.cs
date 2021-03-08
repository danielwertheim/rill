using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Rill.Stores.InMemory
{
    public class InMemoryRillStore : IRillStore
    {
        private readonly ConcurrentDictionary<RillReference, RillState> _rills = new();

        private RillState? GetRillState(RillReference reference)
            => _rills.TryGetValue(reference, out var header)
                ? header
                : null;

        private RillState GetOrAddRill(RillReference reference, Timestamp firstTimestamp)
            => _rills.GetOrAdd(reference, key => new RillState(key, firstTimestamp));

        public Task<RillDetails?> GetDetailsAsync(
            RillReference reference,
            CancellationToken cancellationToken = default)
        {
            var headerSync = GetRillState(reference);

            return Task.FromResult(headerSync?.Details);
        }

        public Task AppendAsync(RillCommit commit, CancellationToken cancellationToken = default)
        {
            var rill = GetOrAddRill(commit.Reference, commit.Timestamp);

            rill.Add(commit);

            return Task.CompletedTask;
        }

        public Task DeleteAsync(RillReference reference, CancellationToken cancellationToken = default)
        {
            _rills.TryRemove(reference, out _);

            return Task.CompletedTask;
        }

        public IEnumerable<RillCommit> ReadCommits(RillReference reference, SequenceRange? sequenceRange = default)
        {
            var rill = GetRillState(reference);
            if (rill == null)
                return Enumerable.Empty<RillCommit>();

            return sequenceRange == null || sequenceRange == SequenceRange.Any
                ? rill.Commits
                : rill.Commits.Where(c => c.SequenceRange.Includes(sequenceRange.Lower) || c.SequenceRange.Includes(sequenceRange.Upper));
        }

        public async IAsyncEnumerable<RillCommit> ReadCommitsAsync(RillReference reference, SequenceRange? sequenceRange = default, [EnumeratorCancellation]CancellationToken cancellationToken = default)
        {
            var rill = GetRillState(reference);
            if (rill == null)
            {
                await ValueTask.CompletedTask;
                yield break;
            }

            var enumerable = sequenceRange == null || sequenceRange == SequenceRange.Any
                ? rill.Commits
                : rill.Commits.Where(c => c.SequenceRange.Includes(sequenceRange.Lower) || c.SequenceRange.Includes(sequenceRange.Upper));

            foreach (var e in enumerable)
                yield return e;
        }

        private sealed class RillState
        {
            private readonly object _sync = new();
            private ImmutableList<RillCommit> _commits = ImmutableList<RillCommit>.Empty;

            internal RillDetails Details { get; private set; }

            internal IEnumerable<RillCommit> Commits => _commits;

            internal RillState(RillReference reference, Timestamp timestamp)
                => Details = RillDetails.New(reference, timestamp);

            internal void Add(RillCommit commit)
            {
                lock (_sync)
                {
                    if (Details.Sequence != Sequence.None && Details.Sequence.Increment() != commit.SequenceRange.Lower)
                        throw Exceptions.StoreConcurrency(Details.Reference, Details.Sequence, commit.SequenceRange.Lower);

                    Details = RillDetails.From(
                        Details.Reference,
                        commit.SequenceRange.Upper,
                        Details.CreatedAt,
                        commit.Timestamp);

                    _commits = _commits.Add(commit);
                }
            }
        }
    }
}
