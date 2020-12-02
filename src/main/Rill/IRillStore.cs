using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Rill
{
    public interface IRillStore<T>
    {
        // void EnsureRevisionIsOkForUpdate(RillHeader header, RillRevision expectedRevision);
        //
        // Task<RillHeader> GetHeaderAsync(RillAddress address, CancellationToken? cancellationToken = null);

        Task AppendAsync(
            IRillCommit<T> commit,
            CancellationToken? cancellationToken = null);

        //
        // IEnumerable<Event<T>> ReadEvents(RillAddress address, RillRevisionRange revisionRange = null);
        // Task<RillHeader> DeleteAsync(RillAddress address, RillRevision rev, CancellationToken? cancellationToken = null);
    }
}
