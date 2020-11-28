using System;
using System.Threading.Tasks;
using Rill;

namespace UnitTests.Rills
{
    internal static class AsyncInterceptingConsumerExtensions
    {
        internal static InterceptionsHas<T> Ensure<T>(this AsyncInterceptingConsumer<T> consumer)
            => new InterceptionsHas<T>(consumer.Interceptions);
    }

    internal class AsyncInterceptingConsumer<T> : IAsyncRillConsumer<T>
    {
        public Interceptions<T> Interceptions { get; } = new Interceptions<T>();

        public Action<Event<T>>? AfterOnNew { get; private set; }

        private AsyncInterceptingConsumer() { }

        internal static AsyncInterceptingConsumer<T> Behaving()
            => new AsyncInterceptingConsumer<T>();

        internal static AsyncInterceptingConsumer<T> Misbehaving(Func<Event<T>, bool>? failPredicate = null)
            => new AsyncInterceptingConsumer<T>().ConfigureAsMisbehaving(failPredicate);

        private AsyncInterceptingConsumer<T> ConfigureAsMisbehaving(Func<Event<T>, bool>? failPredicate = null)
        {
            AfterOnNew = ev =>
            {
                if(failPredicate == null || failPredicate(ev))
                    throw new Exception($"Intentionally failing. Event content: '{ev.Content}'.");
            };

            return this;
        }

        public ValueTask OnNewAsync(Event<T> ev)
        {
            Interceptions.InOnNew(ev);
            AfterOnNew?.Invoke(ev);

            return ValueTask.CompletedTask;
        }

        public ValueTask OnAllSucceededAsync(EventId eventId)
        {
            Interceptions.InOnAllSucceeded(eventId);

            return ValueTask.CompletedTask;
        }

        public ValueTask OnAnyFailedAsync(EventId eventId, Exception ex)
        {
            Interceptions.InOnAnyFailed(eventId, ex);

            return ValueTask.CompletedTask;
        }

        public ValueTask OnCompletedAsync()
        {
            Interceptions.InOnCompleted();

            return ValueTask.CompletedTask;
        }
    }

    internal static class AsyncInterceptingConsumer
    {
        internal static AsyncInterceptingConsumer<string> Behaving()
            => AsyncInterceptingConsumer<string>.Behaving();

        internal static AsyncInterceptingConsumer<string> Misbehaving(Func<Event<string>, bool>? failPredicate = null)
            => AsyncInterceptingConsumer<string>.Misbehaving(failPredicate);
    }
}
