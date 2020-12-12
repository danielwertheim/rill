using Rill.Rills;

namespace Rill
{
    public static class RillFactory
    {
        public static IAsyncRill Asynchronous(RillReference reference) => new AsynchronousRill(reference);
        public static IRill Synchronous(RillReference reference) => new SynchronousRill(reference);
    }
}
