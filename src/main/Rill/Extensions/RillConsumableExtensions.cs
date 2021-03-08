using System;
using Rill.Operators;

namespace Rill.Extensions
{
    public static class RillConsumableExtensions
    {
        public static IDisposable Subscribe<T>(
            this IRillConsumable<T> consumable,
            NewEventHandler<T> onNew,
            SuccessfulEventHandler? onAllSucceeded = null,
            FailedEventHandler? onAnyFailed = null)
            => consumable.Subscribe(ConsumerFactory.SynchronousConsumer(onNew, onAllSucceeded, onAnyFailed));

        public static IRillConsumable<Event> Catch<TException>(this IRillConsumable<Event> consumable, Action<TException> handler)
            where TException : Exception
            => new CatchOp<Event, TException>(consumable, handler);

        public static IRillConsumable<Event> CatchAny(this IRillConsumable<Event> consumable, Action<Exception> handler)
            => consumable.Catch(handler);

        public static IRillConsumable<Event<TResult>> When<TResult>(this IRillConsumable<Event> consumable)
            where TResult : class
            => new OfTypeOp<object, TResult>(consumable);

        public static IRillConsumable<TResult> Select<TSource, TResult>(this IRillConsumable<TSource> consumable, Func<TSource, TResult> map)
            // where TSource : class
            // where TResult : class
            => new MapOp<TSource, TResult>(consumable, map);

        public static IRillConsumable<Event<T>> Where<T>(this IRillConsumable<Event> consumable, Func<Event<T>, bool> predicate)
            where T : class
            => new FilterOp<T>(new OfTypeOp<object, T>(consumable), predicate);

        public static IRillConsumable<Event<T>> Where<T>(this IRillConsumable<Event<T>> consumable, Func<Event<T>, bool> predicate)
            where T : class
            => new FilterOp<T>(consumable, predicate);
    }
}
