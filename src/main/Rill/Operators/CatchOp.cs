using System;

namespace Rill.Operators
{
    internal sealed class CatchOp<T, TException> : IRillConsumable<T>
        where T : class
        where TException : Exception
    {
        private readonly IRillConsumable<T> _src;
        private readonly Action<TException> _handler;

        public CatchOp(IRillConsumable<T> src, Action<TException> handler)
        {
            _src = src;
            _handler = handler;
        }

        public void Dispose()
            => _src.Dispose();

        public IDisposable Subscribe(IRillConsumer<T> consumer)
            => _src.Subscribe(new CatchingConsumer(consumer, _handler));

        private sealed class CatchingConsumer : IRillConsumer<T>
        {
            private readonly IRillConsumer<T> _consumer;
            private readonly Action<TException> _handler;

            public CatchingConsumer(IRillConsumer<T> consumer, Action<TException> handler)
            {
                _consumer = consumer;
                _handler = handler;
            }

            public void OnNew(Event<T> ev)
            {
                try
                {
                    _consumer.OnNew(ev);
                }
                catch (TException ex)
                {
                    _handler(ex);
                }
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
