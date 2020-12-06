using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Rill;

namespace UnitTests
{
    internal class InterceptingStore<T> : IRillStore<T>
    {
        private readonly List<IRillCommit<T>> _appends
            = new List<IRillCommit<T>>();

        public Task<RillHeader?> GetHeaderAsync(RillReference reference, CancellationToken? cancellationToken = null)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteAsync(RillReference reference, CancellationToken? cancellationToken = null)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<Event<T>> ReadEvents(RillReference reference, SequenceRange? revision = null)
        {
            throw new System.NotImplementedException();
        }

        public Task AppendAsync(IRillCommit<T> commit, CancellationToken? cancellationToken = null)
        {
            _appends.Add(commit);

            return Task.CompletedTask;
        }

        internal void HasAppendCount(int c)
            => _appends.Should().HaveCount(c);

        internal void Appended(params Event<string>[] events)
        {
            var appendedEvents = _appends.SelectMany(i => i.Events).ToArray();

            appendedEvents.Should().HaveSameCount(events);
            appendedEvents.Should().Contain(events);
            appendedEvents.Should().BeInAscendingOrder(e => e.Sequence);
        }
    }
}
