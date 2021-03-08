using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Rill;

namespace UnitTests.Rills
{
    internal static class InterceptionsExtensions
    {
        internal static InterceptionsHas Ensure(this Interceptions interceptions) => new(interceptions);
    }

    internal class Interceptions
    {
        private readonly List<Event> _inOnNew = new();
        private readonly List<EventId> _inOnAllSucceeded = new();
        private readonly List<EventId> _inOnAnyFailed = new();

        public IReadOnlyList<Event> OnNew => _inOnNew;
        public IReadOnlyList<EventId> OnAllSucceeded => _inOnAllSucceeded;
        public IReadOnlyList<EventId> OnAnyFailed => _inOnAnyFailed;

        internal void InOnNew(Event ev)
            => _inOnNew.Add(ev);

        internal ValueTask InOnNewAsync(Event ev)
        {
            _inOnNew.Add(ev);

            return ValueTask.CompletedTask;
        }

        internal void InOnAllSucceeded(EventId id)
            => _inOnAllSucceeded.Add(id);

        internal ValueTask InOnAllSucceededAsync(EventId id)
        {
            _inOnAllSucceeded.Add(id);

            return ValueTask.CompletedTask;
        }

        internal void InOnAnyFailed(EventId id)
            => _inOnAnyFailed.Add(id);

        internal ValueTask InOnAnyFailedAsync(EventId id)
        {
            _inOnAnyFailed.Add((id));

            return ValueTask.CompletedTask;
        }
    }

    internal class InterceptionsHas
    {
        private readonly Interceptions _interceptions;

        internal InterceptionsHas(Interceptions interceptions)
            => _interceptions = interceptions;

        public void ToBeUnTouched()
        {
            OnNewIsEmpty();
            OnAllSucceededIsEmpty();
            OnAnyFailedIsEmpty();
        }

        public void OnNewIsEmpty()
            => _interceptions.OnNew.Should().BeEmpty();

        internal void OnNewOnlyHas<T>(params Event<T>[] events) where T : class
        {
            var untypedEv = events.Select(ev => ev.AsUntyped()).ToArray();

            _interceptions.OnNew.Should().HaveCount(untypedEv.Length);
            _interceptions.OnNew.Should().Contain(untypedEv);
        }

        internal void OnNewOnlyHasContents(params object[] contents)
        {
            _interceptions.OnNew.Should().HaveCount(contents.Length);
            _interceptions.OnNew.Select(ev => ev.Content).Should().Contain(contents);
        }

        public void OnAllSucceededIsEmpty()
            => _interceptions.OnAllSucceeded.Should().BeEmpty();

        public void OnAllSucceededOnlyHasId(EventId id)
            => _interceptions.OnAllSucceeded.Should().OnlyContain(interceptedId => interceptedId == id);

        public void OnAnyFailedIsEmpty()
            => _interceptions.OnAnyFailed.Should().BeEmpty();

        public void OnAnyFailedOnlyHasId(EventId id)
            => _interceptions.OnAnyFailed.Should().OnlyContain(interceptedId => interceptedId == id);
    }
}
