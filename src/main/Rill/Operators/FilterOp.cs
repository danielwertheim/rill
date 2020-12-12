using System;

namespace Rill.Operators
{
    internal sealed class FilterOp<T> : IRillConsumable<Event<T>>
        where T : class
    {
        private readonly IRillConsumable<Event<T>> _src;
        private readonly Func<Event<T>, bool> _predicate;

        public FilterOp(IRillConsumable<Event<T>> src, Func<Event<T>, bool> predicate)
        {
            _src = src;
            _predicate = predicate;
        }

        public void Dispose() => _src.Dispose();

        public IDisposable Subscribe(IRillConsumer<Event<T>> consumer)
            => _src.Subscribe(new FilteringConsumer(consumer, _predicate));

        private sealed class FilteringConsumer : IRillConsumer<Event<T>>
        {
            private readonly IRillConsumer<Event<T>> _consumer;
            private readonly Func<Event<T>, bool> _predicate;

            public FilteringConsumer(IRillConsumer<Event<T>> consumer, Func<Event<T>, bool> predicate)
            {
                _consumer = consumer;
                _predicate = predicate;
            }

            public void OnNew(Event<T> ev)
            {
                if (_predicate(ev))
                    _consumer.OnNew(ev);
            }

            public void OnAllSucceeded(EventId eventId)
                => _consumer.OnAllSucceeded(eventId);

            public void OnAnyFailed(EventId eventId)
                => _consumer.OnAnyFailed(eventId);
        }
    }
}
