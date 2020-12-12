using System;
using System.Threading.Tasks;

namespace Rill.Operators
{
    internal sealed class AsyncOfTypeOp<TSrc, TResult> : IAsyncRillConsumable<Event<TResult>>
        where TSrc : class
        where TResult : class
    {
        private readonly IAsyncRillConsumable<Event<TSrc>> _src;

        public AsyncOfTypeOp(IAsyncRillConsumable<Event<TSrc>> src)
            => _src = src;

        public void Dispose()
            => _src.Dispose();

        public IDisposable Subscribe(IAsyncRillConsumer<Event<TResult>> consumer)
            => _src.Subscribe(new AsyncOfTypeConsumer(consumer));

        private sealed class AsyncOfTypeConsumer : IAsyncRillConsumer<Event<TSrc>>
        {
            private readonly IAsyncRillConsumer<Event<TResult>> _consumer;

            public AsyncOfTypeConsumer(IAsyncRillConsumer<Event<TResult>> consumer)
                => _consumer = consumer;

            public async ValueTask OnNewAsync(Event<TSrc> ev)
            {
                if (ev.TryCast<TResult>(out var cev) && cev != null)
                    await _consumer.OnNewAsync(cev).ConfigureAwait(false);
            }

            public ValueTask OnAllSucceededAsync(EventId eventId)
                => _consumer.OnAllSucceededAsync(eventId);

            public ValueTask OnAnyFailedAsync(EventId eventId)
                => _consumer.OnAnyFailedAsync(eventId);
        }
    }
}
