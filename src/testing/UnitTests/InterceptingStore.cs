using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Rill;

namespace UnitTests
{
    internal class InterceptingStore : IRillStore
    {
        private readonly List<RillCommit> _appends = new();

        public Task<RillDetails?> GetDetailsAsync(RillReference reference, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteAsync(RillReference reference, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<RillCommit> ReadCommits(RillReference reference, SequenceRange? sequenceRange = default)
        {
            throw new System.NotImplementedException();
        }

        public IAsyncEnumerable<RillCommit> ReadCommitsAsync(RillReference reference, SequenceRange? sequenceRange = default, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task AppendAsync(RillCommit commit, CancellationToken cancellationToken = default)
        {
            _appends.Add(commit);

            return Task.CompletedTask;
        }

        internal void HasAppendCount(int c)
            => _appends.Should().HaveCount(c);

        internal void Appended(params Event[] events)
        {
            var appendedEvents = _appends.SelectMany(i => i.Events).ToArray();

            appendedEvents.Should().HaveSameCount(events);
            appendedEvents.Should().Contain(events);
            appendedEvents.Should().BeInAscendingOrder(e => e.Sequence);
        }
    }
}
