using System;
using System.Threading.Tasks;
using Rill;

namespace UnitTests.Rills
{
    internal static class AsyncInterceptingConsumerExtensions
    {
        internal static InterceptionsHas Ensure<T>(this AsyncInterceptingConsumer<T> consumer)
            where T : class
            => new(consumer.Interceptions);
    }

    internal class AsyncInterceptingConsumer : AsyncInterceptingConsumer<object>{}

    internal class AsyncInterceptingConsumer<T> : IAsyncRillConsumer<Event<T>>
        where T : class
    {
        public Interceptions Interceptions { get; } = new();

        public Action<Event<T>>? AfterOnNew { get; private set; }

        protected AsyncInterceptingConsumer()
        {
        }

        internal static AsyncInterceptingConsumer<T> Behaving() => new();

        internal static AsyncInterceptingConsumer<T> Misbehaving(Func<Event<T>, bool>? failPredicate = null)
            => new AsyncInterceptingConsumer<T>().ConfigureAsMisbehaving(failPredicate);

        private AsyncInterceptingConsumer<T> ConfigureAsMisbehaving(Func<Event<T>, bool>? failPredicate = null)
        {
            AfterOnNew = ev =>
            {
                if (failPredicate == null || failPredicate(ev))
                    throw new Exception($"Intentionally failing. Event content: '{ev.Content}'.");
            };

            return this;
        }

        public ValueTask OnNewAsync(Event<T> ev)
        {
            Interceptions.InOnNew(ev.AsUntyped());
            AfterOnNew?.Invoke(ev);

            return ValueTask.CompletedTask;
        }

        public ValueTask OnAllSucceededAsync(EventId eventId)
        {
            Interceptions.InOnAllSucceeded(eventId);

            return ValueTask.CompletedTask;
        }

        public ValueTask OnAnyFailedAsync(EventId eventId)
        {
            Interceptions.InOnAnyFailed(eventId);

            return ValueTask.CompletedTask;
        }
    }
}
