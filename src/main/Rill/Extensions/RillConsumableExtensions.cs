using System;
using Rill.Operators;

namespace Rill.Extensions
{
    public static class RillConsumableExtensions
    {
        public static IDisposable Subscribe<T>(
            this IRillConsumable<T> consumable,
            Action<Event<T>> onNew,
            Action<EventId>? onAllSucceeded = null,
            Action<EventId, Exception>? onAnyFailed = null,
            Action? onCompleted = null)
            => consumable.Subscribe(ConsumerFactory.SynchronousConsumer(onNew, onAllSucceeded, onAnyFailed, onCompleted));

        public static IRillConsumable<T> Catch<T, TException>(this IRillConsumable<T> consumable, Action<TException> handler)
            where TException : Exception
            where T : class
            => new CatchOp<T, TException>(consumable, handler);

        public static IRillConsumable<T> CatchAny<T>(this IRillConsumable<T> consumable, Action<Exception> handler)
            where T : class
            => consumable.Catch(handler);

        public static IRillConsumable<T> OfEventType<T>(this IRillConsumable<object> consumable)
            => new OfTypeOp<object, T>(consumable);

        public static IRillConsumable<TResult> OfEventType<TSrc, TResult>(this IRillConsumable<TSrc> consumable)
            => new OfTypeOp<TSrc, TResult>(consumable);

        public static IRillConsumable<TResult> Select<TSource, TResult>(this IRillConsumable<TSource> consumable, Func<TSource, TResult> map)
            where TSource : class where TResult : class
            => new MapOp<TSource, TResult>(consumable, map);

        public static IRillConsumable<T> Where<T>(this IRillConsumable<T> consumable, Func<Event<T>, bool> predicate) where T : class
            => new FilterOp<T>(consumable, predicate);

        public static IRillConsumable<T> Where<T>(this IRillConsumable<T> consumable, Func<T, bool> predicate) where T : class
            => new FilterContentOp<T>(consumable, predicate);
    }
}
