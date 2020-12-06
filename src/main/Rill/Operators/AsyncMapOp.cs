using System;
using System.Threading.Tasks;

namespace Rill.Operators
{
    internal sealed class AsyncMapOp<TFrom, TTo> : IAsyncRillConsumable<TTo>
    {
        private readonly IAsyncRillConsumable<TFrom> _src;
        private readonly Func<TFrom, TTo> _map;

        public AsyncMapOp(IAsyncRillConsumable<TFrom> src, Func<TFrom, TTo> map)
        {
            _src = src;
            _map = map;
        }

        public void Dispose()
            => _src.Dispose();

        public IDisposable Subscribe(IAsyncRillConsumer<TTo> consumer)
            => _src.Subscribe(new AsyncMappingConsumer(consumer, _map));

        private sealed class AsyncMappingConsumer : IAsyncRillConsumer<TFrom>
        {
            private readonly IAsyncRillConsumer<TTo> _consumer;
            private readonly Func<TFrom, TTo> _map;

            public AsyncMappingConsumer(IAsyncRillConsumer<TTo> consumer, Func<TFrom, TTo> map)
            {
                _consumer = consumer;
                _map = map;
            }

            public ValueTask OnNewAsync(Event<TFrom> ev)
                => _consumer.OnNewAsync(ev.Map(_map));

            public ValueTask OnAllSucceededAsync(EventId eventId)
                => _consumer.OnAllSucceededAsync(eventId);

            public ValueTask OnAnyFailedAsync(EventId eventId)
                => _consumer.OnAnyFailedAsync(eventId);

            public ValueTask OnCompletedAsync()
                => _consumer.OnCompletedAsync();
        }
    }
}
