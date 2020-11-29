using System;
using System.Threading.Tasks;

namespace Rill.Consumers
{
    internal class AsyncDelegatingConsumer<T> : IAsyncRillConsumer<T>
    {
        private readonly AsyncNewEventHandler<T> _onNew;
        private readonly AsyncSuccessfulEventHandler? _onAllSucceeded;
        private readonly AsyncFailedEventHandler? _onAnyFailed;
        private readonly Func<ValueTask>? _onCompleted;

        internal AsyncDelegatingConsumer(
            AsyncNewEventHandler<T> onNew,
            AsyncSuccessfulEventHandler? onAllSucceeded = null,
            AsyncFailedEventHandler? onAnyFailed = null,
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
