using System;
using Rill;

namespace UnitTests.Rills
{
    internal static class InterceptingConsumerExtensions
    {
        internal static InterceptionsHas Ensure<T>(this InterceptingConsumer<T> consumer)
            where T : class
            => new(consumer.Interceptions);
    }

    internal class InterceptingConsumer : InterceptingConsumer<object>
    {
    }

    internal class InterceptingConsumer<T> : IRillConsumer<Event<T>>
        where T : class
    {
        public Interceptions Interceptions { get; } = new();

        public Action<Event<T>>? AfterOnNew { get; private set; }

        protected InterceptingConsumer()
        {
        }

        internal static InterceptingConsumer<T> Behaving() => new();

        internal static InterceptingConsumer<T> Misbehaving(Func<Event<T>, bool>? failPredicate = null)
            => new InterceptingConsumer<T>().ConfigureAsMisbehaving(failPredicate);

        private InterceptingConsumer<T> ConfigureAsMisbehaving(Func<Event<T>, bool>? failPredicate = null)
        {
            AfterOnNew = ev =>
            {
                if (failPredicate == null || failPredicate(ev))
                    throw new Exception($"Intentionally failing. Event content: '{ev.Content}'.");
            };

            return this;
        }

        public void OnNew(Event<T> ev)
        {
            Interceptions.InOnNew(ev.AsUntyped());
            AfterOnNew?.Invoke(ev);
        }

        public void OnAllSucceeded(EventId eventId)
            => Interceptions.InOnAllSucceeded(eventId);

        public void OnAnyFailed(EventId eventId)
            => Interceptions.InOnAnyFailed(eventId);
    }
}
