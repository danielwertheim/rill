using System;
using System.Threading.Tasks;

namespace Rill.Operators
{
    internal sealed class AsyncOfTypeOp<TSrc, TResult> : IAsyncRillConsumable<TResult>
    {
        private readonly IAsyncRillConsumable<TSrc> _src;

        public AsyncOfTypeOp(IAsyncRillConsumable<TSrc> src)
            => _src = src;

        public void Dispose()
            => _src.Dispose();

        public IDisposable Subscribe(IAsyncRillConsumer<TResult> consumer)
            => _src.Subscribe(new AsyncOfTypeConsumer(consumer));

        private sealed class AsyncOfTypeConsumer : IAsyncRillConsumer<TSrc>
        {
            private readonly IAsyncRillConsumer<TResult> _consumer;

            public AsyncOfTypeConsumer(IAsyncRillConsumer<TResult> consumer)
                => _consumer = consumer;

            public async ValueTask OnNewAsync(Event<TSrc> ev)
            {
                if (ev.TryCast<TResult>(out var cev) && cev != null)
                    await _consumer.OnNewAsync(cev).ConfigureAwait(false);
            }

            public ValueTask OnAllSucceededAsync(EventId eventId)
                => _consumer.OnAllSucceededAsync(eventId);

            public ValueTask OnAnyFailedAsync(EventId eventId, Exception error)
                => _consumer.OnAnyFailedAsync(eventId, error);

            public ValueTask OnCompletedAsync()
                => _consumer.OnCompletedAsync();
        }
    }
}
