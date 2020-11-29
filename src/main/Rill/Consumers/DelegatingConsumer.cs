using System;

namespace Rill.Consumers
{
    internal class DelegatingConsumer<T> : IRillConsumer<T>
    {
        private readonly NewEventHandler<T> _onNew;
        private readonly SuccessfulEventHandler? _onAllSucceeded;
        private readonly FailedEventHandler? _onAnyFailed;
        private readonly Action? _onCompleted;

        internal DelegatingConsumer(
            NewEventHandler<T> onNew,
            SuccessfulEventHandler? onAllSucceeded = null,
            FailedEventHandler? onAnyFailed = null,
            Action? onCompleted = null)
        {
            _onNew = onNew;
            _onAllSucceeded = onAllSucceeded;
            _onAnyFailed = onAnyFailed;
            _onCompleted = onCompleted;
        }

        public void OnNew(Event<T> ev)
            => _onNew(ev);

        public void OnAllSucceeded(EventId eventId)
            => _onAllSucceeded?.Invoke(eventId);

        public void OnAnyFailed(EventId eventId, Exception ex)
            => _onAnyFailed?.Invoke(eventId, ex);

        public void OnCompleted()
            => _onCompleted?.Invoke();
    }
}
