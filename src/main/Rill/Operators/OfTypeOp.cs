using System;

namespace Rill.Operators
{
    internal sealed class OfTypeOp<TResult> : IRillConsumable<TResult>
    {
        private readonly IRillConsumable<object> _src;

        public OfTypeOp(IRillConsumable<object> src)
            => _src = src;

        public void Dispose()
            => _src.Dispose();

        public IDisposable Subscribe(IRillConsumer<TResult> consumer)
            => _src.Subscribe(new OfTypeConsumer(consumer));

        private sealed class OfTypeConsumer : IRillConsumer<object>
        {
            private readonly IRillConsumer<TResult> _consumer;

            public OfTypeConsumer(IRillConsumer<TResult> consumer)
                => _consumer = consumer;

            public void OnNew(Event<object> ev)
            {
                if (ev.TryDownCast<TResult>(out var cev) && cev != null)
                    _consumer.OnNew(cev);
            }

            public void OnAllSucceeded(EventId eventId)
                => _consumer.OnAllSucceeded(eventId);

            public void OnAnyFailed(EventId eventId, Exception error)
                => _consumer.OnAnyFailed(eventId, error);

            public void OnCompleted()
                => _consumer.OnCompleted();
        }
    }
}
