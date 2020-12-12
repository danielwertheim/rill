using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rill.Rills
{
    internal sealed class AsynchronousRill : IAsyncRill
    {
        private const int LockMs = 5000;

        private readonly SemaphoreSlim _sync = new(1, 1);

        private readonly ConcurrentDictionary<int, ObSubscription> _subscriptions = new();

        private bool _isDisposed;

        public RillReference Reference { get; }
        public Sequence Sequence { get; private set; } = Sequence.None;

        internal AsynchronousRill(RillReference reference)
        {
            Reference = reference;
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
                throw new ObjectDisposedException(nameof(AsynchronousRill));
        }

        private async Task OnEmitAsync(Event ev)
        {
            var exceptions = new List<Exception>();
            var consumers = _subscriptions.Select(kv => kv.Value.Consumer).ToList();

            var nextSequence = Sequence.Increment();
            if (ev.Sequence != nextSequence)
                throw Exceptions.EventOutOrOrder(nextSequence, ev.Sequence);

            Sequence = ev.Sequence;

            foreach (var consumer in consumers)
            {
                try
                {
                    await consumer.OnNewAsync(ev).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (!exceptions.Any())
            {
                foreach (var consumer in consumers)
                {
                    try
                    {
                        await consumer.OnAllSucceededAsync(ev.Id).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }

                if (exceptions.Any())
                    throw new AggregateException("Exception(s) while notifying consumers of successful event.", exceptions);
            }
            else
            {
                //TODO: Log new AggregateException("Exception(s) while notifying consumers of new event.", exceptions.ToArray());
                exceptions.Clear();

                foreach (var consumer in consumers)
                {
                    try
                    {
                        await consumer.OnAnyFailedAsync(ev.Id).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }

                if (exceptions.Any())
                    throw new AggregateException("Exception(s) while notifying consumers of failing event.", exceptions);
            }
        }

        public async ValueTask<Event<T>> EmitAsync<T>(T content, EventId? id = null, Sequence? sequence = null, CancellationToken cancellationToken = default)
            where T : class
        {
            await _sync.WaitAsync(LockMs, cancellationToken).ConfigureAwait(false);

            try
            {
                ThrowIfDisposed();

                var ev = Event<T>.New(content, id, sequence ?? Sequence.Increment());

                await OnEmitAsync(ev.AsUntyped()).ConfigureAwait(false);

                return ev;
            }
            finally
            {
                _sync.Release();
            }
        }

        public async Task EmitAsync(Event ev, CancellationToken cancellationToken = default)
        {
            await _sync.WaitAsync(LockMs, cancellationToken).ConfigureAwait(false);

            try
            {
                ThrowIfDisposed();

                await OnEmitAsync(ev).ConfigureAwait(false);
            }
            finally
            {
                _sync.Release();
            }
        }

        public async Task EmitAsync(RillCommit commit, CancellationToken cancellationToken = default)
        {
            await _sync.WaitAsync(LockMs, cancellationToken).ConfigureAwait(false);

            try
            {
                ThrowIfDisposed();

                if (commit.Reference != Reference)
                    throw new ArgumentException($"Commit with Rill Reference '{commit.Reference}' does not belong to Rill with reference '{Reference}'.");

                foreach (var ev in commit.Events)
                    await OnEmitAsync(ev).ConfigureAwait(false);
            }
            finally
            {
                _sync.Release();
            }
        }

        private void DisposeSubscription(ObSubscription sub)
            => _subscriptions.TryRemove(sub.Id, out _);

        public IDisposable Subscribe(IAsyncRillConsumer<Event> consumer)
        {
            _sync.Wait(LockMs);

            try
            {
                ThrowIfDisposed();

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
            public readonly IAsyncRillConsumer<Event> Consumer;

            public ObSubscription(IAsyncRillConsumer<Event> consumer, Action<ObSubscription> onDispose)
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
