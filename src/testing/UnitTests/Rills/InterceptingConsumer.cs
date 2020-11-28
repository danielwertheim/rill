using System;
using Rill;

namespace UnitTests.Rills
{
    internal static class InterceptingConsumerExtensions
    {
        internal static InterceptionsHas<T> Ensure<T>(this InterceptingConsumer<T> consumer)
            => new InterceptionsHas<T>(consumer.Interceptions);
    }

    internal class InterceptingConsumer<T> : IRillConsumer<T>
    {
        public Interceptions<T> Interceptions { get; } = new Interceptions<T>();

        public Action<Event<T>>? AfterOnNew { get; private set; }

        private InterceptingConsumer() { }

        internal static InterceptingConsumer<T> Behaving()
            => new InterceptingConsumer<T>();

        internal static InterceptingConsumer<T> Misbehaving(Func<Event<T>, bool>? failPredicate = null)
            => new InterceptingConsumer<T>().ConfigureAsMisbehaving(failPredicate);

        private InterceptingConsumer<T> ConfigureAsMisbehaving(Func<Event<T>, bool>? failPredicate = null)
        {
            AfterOnNew = ev =>
            {
                if(failPredicate == null || failPredicate(ev))
                    throw new Exception($"Intentionally failing. Event content: '{ev.Content}'.");
            };

            return this;
        }

        public void OnNew(Event<T> ev)
        {
            Interceptions.InOnNew(ev);
            AfterOnNew?.Invoke(ev);
        }

        public void OnAllSucceeded(EventId eventId)
            => Interceptions.InOnAllSucceeded(eventId);

        public void OnAnyFailed(EventId eventId, Exception ex)
            => Interceptions.InOnAnyFailed(eventId, ex);

        public void OnCompleted()
            => Interceptions.InOnCompleted();
    }

    internal static class InterceptingConsumer
    {
        internal static InterceptingConsumer<string> Behaving()
            => InterceptingConsumer<string>.Behaving();

        internal static InterceptingConsumer<string> Misbehaving(Func<Event<string>, bool>? failPredicate = null)
            => InterceptingConsumer<string>.Misbehaving(failPredicate);
    }
}
