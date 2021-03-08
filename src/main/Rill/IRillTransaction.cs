using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rill
{
    public interface IRillTransaction : IDisposable
    {
        /// <summary>
        /// Commits the events produced during the lifetime of the transaction.
        /// </summary>
        /// <param name="store"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<RillCommit> CommitAsync(IRillStore store, CancellationToken cancellationToken = default);
    }
}
