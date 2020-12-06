using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rill.Stores.InMemory
{
    public class InMemoryRillStore<T> : IRillStore<T>
    {
        private readonly ConcurrentDictionary<RillReference, RillState> _rills = new ConcurrentDictionary<RillReference, RillState>();

        private RillState? GetRillState(RillReference reference)
            => _rills.TryGetValue(reference, out var header)
                ? header
                : null;

        private RillState GetOrAddRill(RillReference reference, Timestamp firstTimestamp)
            => _rills.GetOrAdd(reference, key => new RillState(key, firstTimestamp));

        public Task<RillHeader?> GetHeaderAsync(
            RillReference reference,
            CancellationToken? cancellationToken = null)
        {
            var headerSync = GetRillState(reference);

            return Task.FromResult(headerSync?.Header);
        }

        public Task AppendAsync(
            IRillCommit<T> commit,
            CancellationToken? cancellationToken = null)
        {
            var rill = GetOrAddRill(commit.Reference, commit.Timestamp);

            rill.Add(commit);

            return Task.CompletedTask;
        }

        public Task DeleteAsync(
            RillReference reference,
            CancellationToken? cancellationToken = null)
        {
            _rills.TryRemove(reference, out _);

            return Task.CompletedTask;
        }

        public IEnumerable<Event<T>> ReadEvents(RillReference reference, SequenceRange? sequenceRange = default)
        {
            var rill = GetRillState(reference);
            if (rill == null)
                return Enumerable.Empty<Event<T>>();

            return sequenceRange == null
                ? rill.Commits.SelectMany(c => c.Events)
                : rill.Commits.SelectMany(c => c.Events.Where(e => sequenceRange.Includes(e.Sequence)));
        }

        private sealed class RillState
        {
            private readonly object _sync = new object();
            private ImmutableList<IRillCommit<T>> _commits = ImmutableList<IRillCommit<T>>.Empty;

            internal RillHeader Header { get; private set; }

            internal IEnumerable<IRillCommit<T>> Commits => _commits;

            internal RillState(RillReference reference, Timestamp timestamp)
                => Header = RillHeader.New(reference, timestamp);

            internal void Add(IRillCommit<T> commit)
            {
                lock (_sync)
                {
                    if (Header.Sequence != Sequence.None && Header.Sequence.Increment() != commit.SequenceRange.Lower)
                        throw Exceptions.StoreConcurrency(Header.Reference, Header.Sequence, commit.SequenceRange.Lower);

                    Header = RillHeader.From(
                        Header.Reference,
                        commit.SequenceRange.Upper,
                        Header.CreatedAt,
                        commit.Timestamp);

                    _commits = _commits.Add(commit);
                }
            }
        }
    }
}
