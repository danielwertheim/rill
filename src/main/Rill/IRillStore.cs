using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rill
{
    public interface IRillStore
    {
        Task<RillDetails?> GetDetailsAsync(RillReference reference, CancellationToken cancellationToken = default);
        Task AppendAsync(RillCommit commit, CancellationToken cancellationToken = default);
        Task DeleteAsync(RillReference reference, CancellationToken cancellationToken = default);
        IEnumerable<RillCommit> ReadCommits(RillReference reference, SequenceRange? sequenceRange = default);
        IAsyncEnumerable<RillCommit> ReadCommitsAsync(RillReference reference, SequenceRange? sequenceRange = default, CancellationToken cancellationToken = default);
    }
}
