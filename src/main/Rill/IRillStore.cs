using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rill
{
    public interface IRillStore<T>
    {
        Task<RillHeader?> GetHeaderAsync(RillReference reference, CancellationToken? cancellationToken = null);

        Task DeleteAsync(RillReference reference, CancellationToken? cancellationToken = null);

        IEnumerable<Event<T>> ReadEvents(RillReference reference, SequenceRange? revision = null);

        Task AppendAsync(IRillCommit<T> commit, CancellationToken? cancellationToken = null);
    }
}
