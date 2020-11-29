using Rill.Rills;

namespace Rill
{
    public static class RillFactory
    {
        public static IAsyncRill<T> Asynchronous<T>(RillReference reference) => new AsynchronousRill<T>(reference);
        public static IRill<T> Synchronous<T>(RillReference reference) => new SynchronousRill<T>(reference);
    }
}
