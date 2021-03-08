using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Rill.Rills
{
    internal sealed class SynchronousRill : IRill
    {
        private readonly object _sync = new();

        private readonly ConcurrentDictionary<int, ObSubscription> _subscriptions = new();

        private bool _isDisposed;

        public RillReference Reference { get; }
        public Sequence Sequence { get; private set; } = Sequence.None;

        internal SynchronousRill(RillReference reference)
            => Reference = reference;

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
                throw new ObjectDisposedException(nameof(SynchronousRill));
        }

        private void OnEmit(Event ev)
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
                    consumer.OnNew(ev);
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
                        consumer.OnAllSucceeded(ev.Id);
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
                        consumer.OnAnyFailed(ev.Id);
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

        public Event<T> Emit<T>(T content, EventId? id = null, Sequence? sequence = null) where T : class
        {
            lock (_sync)
            {
                ThrowIfDisposed();

                var ev = Event<T>.New(content, id, sequence ?? Sequence.Increment());

                OnEmit(ev.AsUntyped());

                return ev;
            }
        }

        public void Emit(Event ev)
        {
            lock (_sync)
            {
                ThrowIfDisposed();

                OnEmit(ev);
            }
        }

        public void Emit(RillCommit commit)
        {
            lock (_sync)
            {
                ThrowIfDisposed();

                if (commit.Reference != Reference)
                    throw new ArgumentException($"Commit with Rill Reference '{commit.Reference}' does not belong to Rill with reference '{Reference}'.");

                foreach (var ev in commit.Events)
                    OnEmit(ev);
            }
        }

        private void DisposeSubscription(ObSubscription sub)
            => _subscriptions.TryRemove(sub.Id, out _);

        public IDisposable Subscribe(IRillConsumer<Event> consumer)
        {
            lock (_sync)
            {
                ThrowIfDisposed();

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
            public readonly IRillConsumer<Event> Consumer;

            public ObSubscription(IRillConsumer<Event> consumer, Action<ObSubscription> onDispose)
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
