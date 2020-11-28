using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Rill.Extensions;
using Rill.Internals;

namespace Rill.Rills
{
    internal abstract class SyncRillBase<T> : IRill<T>, IRillConsumable<T>
    {
        private readonly object _sync = new object();

        private Sequence _sequence = Sequence.None;

        private readonly ConcurrentDictionary<int, ObSubscription> _subscriptions
            = new ConcurrentDictionary<int, ObSubscription>();

        private bool _isCompleted;
        private bool _isDisposed;

        public IRillConsumable<object> ConsumeAny { get; }

        public IRillConsumable<T> Consume { get; }

        protected IEnumerable<IRillConsumer<T>> Consumers
            => _subscriptions.Select(kv => kv.Value.Consumer);

        protected SyncRillBase()
        {
            ConsumeAny = this.OfEventType<T, object>();
            Consume = this;
        }

        public void Dispose()
        {
            var exs = new List<Exception>();

            lock (_sync)
            {
                if (_isDisposed)
                    return;

                _isDisposed = true;

                foreach (var kv in _subscriptions)
                {
                    var sub = kv.Value;

                    try
                    {
                        sub.Dispose();
                    }
                    catch (Exception e)
                    {
                        exs.Add(e);
                    }
                }
            }

            if (exs.Any())
                throw new AggregateException("Failed while disposing Rill. See inner exception(s) for more details.", exs);
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(SyncRillBase<T>));
        }

        private void ThrowIfCompleted()
        {
            if (_isCompleted)
                throw new InvalidOperationException("Rill has been completed. No further operations other than Dispose will be allowed.");
        }

        public void Complete()
        {
            lock (_sync)
            {
                ThrowIfDisposed();
                ThrowIfCompleted();

                _isCompleted = true;

                foreach (var kv in _subscriptions)
                {
                    var sub = kv.Value;
                    Swallow.Everything(() => sub.Consumer.OnCompleted());
                }
            }
        }

        protected abstract Event<T> OnEmit(Event<T> ev);

        public Event<T> Emit(T content, EventId? id = null, Sequence? sequence = null)
        {
            lock (_sync)
            {
                ThrowIfDisposed();
                ThrowIfCompleted();

                var nextSequence = _sequence.Increment();

                if(sequence != null && sequence != nextSequence)
                    throw Exceptions.EventOutOrOrder(nextSequence, sequence);

                _sequence = nextSequence;

                var ev = Event.Create(content, id, nextSequence);

                return OnEmit(ev);
            }
        }

        private void DisposeSubscription(ObSubscription sub)
            => _subscriptions.TryRemove(sub.Id, out _);

        IDisposable IRillConsumable<T>.Subscribe(IRillConsumer<T> consumer)
        {
            lock (_sync)
            {
                ThrowIfDisposed();
                ThrowIfCompleted();

                var sub = new ObSubscription(consumer, DisposeSubscription);

                if (!_subscriptions.TryAdd(sub.Id, sub))
                    throw new ArgumentException(
                        "Can not subscribe consumer. Each consumer can only be subscribed once (determined by Consumer.GetHashCode).",
                        nameof(consumer));

                return sub;
            }
        }

        private sealed class ObSubscription : IDisposable
        {
            private readonly Action<ObSubscription> _onDispose;

            public readonly int Id;
            public readonly IRillConsumer<T> Consumer;

            public ObSubscription(IRillConsumer<T> consumer, Action<ObSubscription> onDispose)
            {
                Id = consumer.GetHashCode();
                Consumer = consumer;
                _onDispose = onDispose;
            }

            public void Dispose()
                => _onDispose(this);
        }
    }
}
