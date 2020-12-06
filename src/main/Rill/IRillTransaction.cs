using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rill
{
    public interface IRillTransaction<T> : IDisposable
    {
        Task<IRillCommit<T>> CommitAsync(IRillStore<T> store, CancellationToken cancellationToken = default);
    }
}
