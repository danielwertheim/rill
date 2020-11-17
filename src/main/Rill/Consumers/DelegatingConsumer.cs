using System;

namespace Rill.Consumers
{
    internal class DelegatingConsumer<T> : IRillConsumer<T>
    {
        private readonly Action<Event<T>> _onNew;
        private readonly Action<EventId>? _onAllSucceeded;
        private readonly Action<EventId, Exception>? _onAnyFailed;
        private readonly Action? _onCompleted;

        internal DelegatingConsumer(
            Action<Event<T>> onNew,
            Action<EventId>? onAllSucceeded = null,
            Action<EventId, Exception>? onAnyFailed = null,
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
