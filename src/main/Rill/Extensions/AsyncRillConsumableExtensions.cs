using System;
using Rill.Operators;

namespace Rill.Extensions
{
    public static class AsyncRillConsumableExtensions
    {
        public static IDisposable Subscribe<T>(
            this IAsyncRillConsumable<T> consumable,
            AsyncNewEventHandler<T> onNew,
            AsyncSuccessfulEventHandler? onAllSucceeded = null,
            AsyncFailedEventHandler? onAnyFailed = null)
            => consumable.Subscribe(ConsumerFactory.AsynchronousConsumer(onNew, onAllSucceeded, onAnyFailed));

        public static IAsyncRillConsumable<Event> Catch<TException>(this IAsyncRillConsumable<Event> consumable, Action<TException> handler)
            where TException : Exception
            => new AsyncCatchOp<Event, TException>(consumable, handler);

        public static IAsyncRillConsumable<Event> CatchAny(this IAsyncRillConsumable<Event> consumable, Action<Exception> handler)
            => consumable.Catch(handler);

        public static IAsyncRillConsumable<Event<TResult>> When<TResult>(this IAsyncRillConsumable<Event> consumable)
            where TResult : class
            => new AsyncOfTypeOp<object, TResult>(consumable);

        public static IAsyncRillConsumable<TResult> Select<TSource, TResult>(this IAsyncRillConsumable<TSource> consumable, Func<TSource, TResult> map)
            // where TSource : class
            // where TResult : class
            => new AsyncMapOp<TSource, TResult>(consumable, map);

        public static IAsyncRillConsumable<Event<T>> Where<T>(this IAsyncRillConsumable<Event> consumable, Func<Event<T>, bool> predicate)
            where T : class
            => new AsyncFilterOp<T>(new AsyncOfTypeOp<object, T>(consumable), predicate);

        public static IAsyncRillConsumable<Event<T>> Where<T>(this IAsyncRillConsumable<Event<T>> consumable, Func<Event<T>, bool> predicate)
            where T : class
            => new AsyncFilterOp<T>(consumable, predicate);
    }
}
