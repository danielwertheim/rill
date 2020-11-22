using System;
using System.Threading.Tasks;

namespace Rill.Consumers
{
    internal class AsyncDelegatingConsumer<T> : IAsyncRillConsumer<T>
    {
        private readonly Func<Event<T>, ValueTask> _onNew;
        private readonly Func<EventId, ValueTask>? _onAllSucceeded;
        private readonly Func<EventId, Exception, ValueTask>? _onAnyFailed;
        private readonly Func<ValueTask>? _onCompleted;

        internal AsyncDelegatingConsumer(
            Func<Event<T>, ValueTask> onNew,
            Func<EventId, ValueTask>? onAllSucceeded = null,
            Func<EventId, Exception, ValueTask>? onAnyFailed = null,
            Func<ValueTask>? onCompleted = null)
        {
            _onNew = onNew;
            _onAllSucceeded = onAllSucceeded;
            _onAnyFailed = onAnyFailed;
            _onCompleted = onCompleted;
        }

        public ValueTask OnNewAsync(Event<T> ev)
            => _onNew(ev);

        public ValueTask OnAllSucceededAsync(EventId eventId)
            => _onAllSucceeded?.Invoke(eventId) ?? ValueTask.CompletedTask;

        public ValueTask OnAnyFailedAsync(EventId eventId, Exception ex)
            => _onAnyFailed?.Invoke(eventId, ex) ?? ValueTask.CompletedTask;

        public ValueTask OnCompletedAsync()
            => _onCompleted?.Invoke() ?? ValueTask.CompletedTask;
    }
}
