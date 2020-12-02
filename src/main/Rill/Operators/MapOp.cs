using System;

namespace Rill.Operators
{
    internal sealed class MapOp<TFrom, TTo> : IRillConsumable<TTo>
    {
        private readonly IRillConsumable<TFrom> _src;
        private readonly Func<TFrom, TTo> _map;

        public MapOp(IRillConsumable<TFrom> src, Func<TFrom, TTo> map)
        {
            _src = src;
            _map = map;
        }

        public void Dispose()
            => _src.Dispose();

        public IDisposable Subscribe(IRillConsumer<TTo> consumer)
            => _src.Subscribe(new MappingConsumer(consumer, _map));

        private sealed class MappingConsumer : IRillConsumer<TFrom>
        {
            private readonly IRillConsumer<TTo> _consumer;
            private readonly Func<TFrom, TTo> _map;

            public MappingConsumer(IRillConsumer<TTo> consumer, Func<TFrom, TTo> map)
            {
                _consumer = consumer;
                _map = map;
            }

            public void OnNew(Event<TFrom> value)
                => _consumer.OnNew(value.Map(_map));

            public void OnAllSucceeded(EventId eventId)
                => _consumer.OnAllSucceeded(eventId);

            public void OnAnyFailed(EventId eventId)
                => _consumer.OnAnyFailed(eventId);

            public void OnCompleted()
                => _consumer.OnCompleted();
        }
    }
}
