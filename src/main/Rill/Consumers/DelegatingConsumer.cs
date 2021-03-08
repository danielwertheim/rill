namespace Rill.Consumers
{
    internal class DelegatingConsumer<T> : IRillConsumer<T>
    {
        private readonly NewEventHandler<T> _onNew;
        private readonly SuccessfulEventHandler? _onAllSucceeded;
        private readonly FailedEventHandler? _onAnyFailed;

        internal DelegatingConsumer(
            NewEventHandler<T> onNew,
            SuccessfulEventHandler? onAllSucceeded = null,
            FailedEventHandler? onAnyFailed = null)
        {
            _onNew = onNew;
            _onAllSucceeded = onAllSucceeded;
            _onAnyFailed = onAnyFailed;
        }

        public void OnNew(T value)
            => _onNew(value);

        public void OnAllSucceeded(EventId eventId)
            => _onAllSucceeded?.Invoke(eventId);

        public void OnAnyFailed(EventId eventId)
            => _onAnyFailed?.Invoke(eventId);
    }
}
