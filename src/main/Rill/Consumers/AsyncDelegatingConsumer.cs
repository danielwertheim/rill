using System.Threading.Tasks;

namespace Rill.Consumers
{
    internal class AsyncDelegatingConsumer<T> : IAsyncRillConsumer<T>
    {
        private readonly AsyncNewEventHandler<T> _onNew;
        private readonly AsyncSuccessfulEventHandler? _onAllSucceeded;
        private readonly AsyncFailedEventHandler? _onAnyFailed;

        internal AsyncDelegatingConsumer(
            AsyncNewEventHandler<T> onNew,
            AsyncSuccessfulEventHandler? onAllSucceeded = null,
            AsyncFailedEventHandler? onAnyFailed = null)
        {
            _onNew = onNew;
            _onAllSucceeded = onAllSucceeded;
            _onAnyFailed = onAnyFailed;
        }

        public ValueTask OnNewAsync(T value)
            => _onNew(value);

        public ValueTask OnAllSucceededAsync(EventId eventId)
            => _onAllSucceeded?.Invoke(eventId) ?? ValueTask.CompletedTask;

        public ValueTask OnAnyFailedAsync(EventId eventId)
            => _onAnyFailed?.Invoke(eventId) ?? ValueTask.CompletedTask;
    }
}
