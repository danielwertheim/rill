using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rill.Extensions;
using Rill.Internals;

namespace Rill.Rills
{
    internal abstract class AsyncRillBase<T> : IAsyncRill<T>, IAsyncRillConsumable<T>
    {
        private const int LockMs = 5000;

        private readonly SemaphoreSlim _sync = new SemaphoreSlim(1, 1);

        private EventSequence _sequence = EventSequence.None;

        private readonly ConcurrentDictionary<int, ObSubscription> _subscriptions
            = new ConcurrentDictionary<int, ObSubscription>();

        private bool _isCompleted;
        private bool _isDisposed;

        public IAsyncRillConsumable<object> ConsumeAny { get; }

        public IAsyncRillConsumable<T> Consume { get; }

        protected IEnumerable<IAsyncRillConsumer<T>> Consumers
            => _subscriptions.Select(kv => kv.Value.Consumer);

        protected AsyncRillBase()
        {
            ConsumeAny = this.OfEventType<T, object>();
            Consume = this;
        }

        public void Dispose()
        {
            var exs = new List<Exception>();

            _sync.Wait(LockMs);

            try
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
            finally
            {
                _sync.Release();
            }

            _sync.Dispose();

            if (exs.Any())
                throw new AggregateException("Failed while disposing Rill. See inner exception(s) for more details.", exs);
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(AsyncRillBase<T>));
        }

        private void ThrowIfCompleted()
        {
            if (_isCompleted)
                throw new InvalidOperationException("Rill has been completed. No further operations other than Dispose will be allowed.");
        }

        public void Complete()
        {
            _sync.Wait(LockMs);

            try
            {
                ThrowIfDisposed();
                ThrowIfCompleted();

                _isCompleted = true;

                foreach (var kv in _subscriptions)
                {
                    var sub = kv.Value;
                    Swallow.Everything(async () =>
                        await sub.Consumer.OnCompletedAsync().ConfigureAwait(false));
                }
            }
            finally
            {
                _sync.Release();
            }
        }

        protected abstract ValueTask<Event<T>> OnEmitAsync(Event<T> ev);

        public async ValueTask<Event<T>> EmitAsync(T content, EventId? id = null, CancellationToken cancellationToken = default)
        {
            await _sync.WaitAsync(LockMs, cancellationToken).ConfigureAwait(false);

            try
            {
                ThrowIfDisposed();
                ThrowIfCompleted();

                _sequence = _sequence.Increment();

                var ev = new Event<T>(
                    id ?? EventId.New(),
                    _sequence,
                    content);

                return await OnEmitAsync(ev).ConfigureAwait(false);
            }
            finally
            {
                _sync.Release();
            }
        }

        private void DisposeSubscription(ObSubscription sub)
            => _subscriptions.TryRemove(sub.Id, out _);

        IDisposable IAsyncRillConsumable<T>.Subscribe(IAsyncRillConsumer<T> consumer)
        {
            _sync.Wait(LockMs);

            try
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
            finally
            {
                _sync.Release();
            }
        }

        private sealed class ObSubscription : IDisposable
        {
            private readonly Action<ObSubscription> _onDispose;

            public readonly int Id;
            public readonly IAsyncRillConsumer<T> Consumer;

            public ObSubscription(IAsyncRillConsumer<T> consumer, Action<ObSubscription> onDispose)
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
