using Rill.Rills;

namespace Rill
{
    public static class RillFactory
    {
        public static IAsyncRill<T> Asynchronous<T>() => new AsynchronousRill<T>();
        public static IRill<T> Synchronous<T>() => new SynchronousRill<T>();
    }
}
