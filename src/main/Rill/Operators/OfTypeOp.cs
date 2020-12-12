using System;

namespace Rill.Operators
{
    internal sealed class OfTypeOp<TSrc, TResult> : IRillConsumable<Event<TResult>>
        where TSrc : class
        where TResult : class
    {
        private readonly IRillConsumable<Event<TSrc>> _src;

        public OfTypeOp(IRillConsumable<Event<TSrc>> src)
            => _src = src;

        public void Dispose()
            => _src.Dispose();

        public IDisposable Subscribe(IRillConsumer<Event<TResult>> consumer)
            => _src.Subscribe(new OfTypeConsumer(consumer));

        private sealed class OfTypeConsumer : IRillConsumer<Event<TSrc>>
        {
            private readonly IRillConsumer<Event<TResult>> _consumer;

            public OfTypeConsumer(IRillConsumer<Event<TResult>> consumer)
                => _consumer = consumer;

            public void OnNew(Event<TSrc> ev)
            {
                if (ev.TryCast<TResult>(out var cev) && cev != null)
                    _consumer.OnNew(cev);
            }

            public void OnAllSucceeded(EventId eventId)
                => _consumer.OnAllSucceeded(eventId);

            public void OnAnyFailed(EventId eventId)
                => _consumer.OnAnyFailed(eventId);
        }
    }
}
