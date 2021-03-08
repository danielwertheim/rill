using Rill.Consumers;

namespace Rill
{
    public static class ConsumerFactory
    {
        public static IRillConsumer<T> SynchronousConsumer<T>(
            NewEventHandler<T> onNew,
            SuccessfulEventHandler? onAllSucceeded = null,
            FailedEventHandler? onAnyFailed = null)
            => new DelegatingConsumer<T>(onNew, onAllSucceeded, onAnyFailed);

        public static IAsyncRillConsumer<T> AsynchronousConsumer<T>(
            AsyncNewEventHandler<T> onNew,
            AsyncSuccessfulEventHandler? onAllSucceeded = null,
            AsyncFailedEventHandler? onAnyFailed = null)
            => new AsyncDelegatingConsumer<T>(onNew, onAllSucceeded, onAnyFailed);
    }
}
