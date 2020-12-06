using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Rill.Extensions;

namespace Rill
{
    public static class RillTransaction
    {
        public static IRillTransaction<T> Begin<T>(IRill<T> rill)
            => RillTransaction<T>.Begin(rill);
    }

    internal sealed class RillTransaction<T> : IRillTransaction<T>
    {
        private readonly RillReference _rillReference;
        private readonly ConcurrentQueue<Event<T>> _stage;
        private readonly IDisposable _rillSubscription;
        private bool _isDisposed;
        private long _ackCount;
        private long _nackCount;

        private RillTransaction(IRill<T> rill)
        {
            _stage = new ConcurrentQueue<Event<T>>();
            _rillReference = rill.Reference;
            _rillSubscription = rill.Consume.Subscribe(
                ev => _stage.Enqueue(ev),
                successfulId => { _ackCount = Interlocked.Increment(ref _ackCount); },
                failedId =>
                {
                    _nackCount = Interlocked.Increment(ref _nackCount);

                    // ReSharper disable once ConstantConditionalAccessQualifier
                    _rillSubscription?.Dispose();
                });
        }

        internal static IRillTransaction<T> Begin(IRill<T> rill)
            => new RillTransaction<T>(rill);

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            _rillSubscription.Dispose();
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        private bool TryDrainFor(TimeSpan interval)
            => _ackCount + _nackCount == _stage.Count ||
               SpinWait.SpinUntil(() => _ackCount + _nackCount == _stage.Count, interval);

        public async Task<IRillCommit<T>?> CommitAsync(IRillStore<T> store, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            _rillSubscription.Dispose();

            if (!TryDrainFor(TimeSpan.FromMilliseconds(250)))
                throw new InvalidOperationException("Can not commit when staged event count differs from total acked events (positive and negative).");

            if (_nackCount > 0)
                throw new InvalidOperationException("Can not commit when there's knowledge about a failed event.");

            if (_stage.IsEmpty)
                return null;

            var commit = RillCommit.New(
                _rillReference,
                _stage.ToImmutableList());

            await store.AppendAsync(
                commit,
                cancellationToken);

            return commit;
        }
    }
}
