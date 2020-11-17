using System;
using System.Threading.Tasks;
using Rill.Consumers;

namespace Rill
{
    public static class ConsumerFactory
    {
        public static IRillConsumer<T> SynchronousConsumer<T>(
            Action<Event<T>> onNew,
            Action<EventId>? onSucceeded = null,
            Action<EventId, Exception>? onFailed = null,
            Action? onCompleted = null)
            => new DelegatingConsumer<T>(onNew, onSucceeded, onFailed, onCompleted);

        public static IAsyncRillConsumer<T> AsynchronousConsumer<T>(
            Func<Event<T>, ValueTask> onNew,
            Func<EventId, ValueTask>? onSucceeded = null,
            Func<EventId, Exception, ValueTask>? onFailed = null,
            Func<ValueTask>? onCompleted = null)
            => new AsyncDelegatingConsumer<T>(onNew, onSucceeded, onFailed, onCompleted);
    }
}
