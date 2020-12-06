using System;
using System.Threading;
using System.Threading.Tasks;
using Rill;

namespace ConsoleSample
{
    public class FakeStore<T> : IRillStore<T>
    {
        public Task AppendAsync(IRillCommit<T> commit, CancellationToken? cancellationToken = null)
        {
            Console.WriteLine($"Storing ref: {commit.Reference}@{commit.Revision} eventCount:{commit.Events.Count}");

            return Task.CompletedTask;
        }
    }
}
