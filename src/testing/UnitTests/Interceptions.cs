using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Rill;

namespace UnitTests
{
    internal static class InterceptionsExtensions
    {
        internal static InterceptionsHas<T> Ensure<T>(this Interceptions<T> interceptions)
            => new InterceptionsHas<T>(interceptions);
    }

    internal class Interceptions<T>
    {
        private readonly List<Event<T>> _inOnNew = new List<Event<T>>();
        private readonly List<EventId> _inOnAllSucceeded = new List<EventId>();
        private readonly List<(EventId, Exception)> _inOnAnyFailed = new List<(EventId, Exception)>();

        public IReadOnlyList<Event<T>> OnNew => _inOnNew;
        public IReadOnlyList<EventId> OnAllSucceeded => _inOnAllSucceeded;
        public IReadOnlyList<(EventId Id, Exception Ex)> OnAnyFailed => _inOnAnyFailed;
        public bool OnCompletedIntercepted { get; private set; }

        internal void InOnNew(Event<T> ev)
            => _inOnNew.Add(ev);

        internal ValueTask InOnNewAsync(Event<T> ev)
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

        internal void InOnAnyFailed(EventId id, Exception ex)
            => _inOnAnyFailed.Add((id, ex));

        internal ValueTask InOnAnyFailedAsync(EventId id, Exception ex)
        {
            _inOnAnyFailed.Add((id, ex));

            return ValueTask.CompletedTask;
        }

        internal void InOnCompleted()
            => OnCompletedIntercepted = true;

        internal ValueTask InOnCompletedAsync()
        {
            OnCompletedIntercepted = true;

            return ValueTask.CompletedTask;
        }
    }

    internal class Interceptions : Interceptions<string>
    {
    }

    internal class InterceptionsHas<T>
    {
        private readonly Interceptions<T> _interceptions;

        internal InterceptionsHas(Interceptions<T> interceptions)
            => _interceptions = interceptions;

        public void ToBeUnTouched()
        {
            OnNewIsEmpty();
            OnAllSucceededIsEmpty();
            OnAnyFailedIsEmpty();
            OnCompletedWasNotCalled();
        }

        public void OnNewIsEmpty()
            => _interceptions.OnNew.Should().BeEmpty();

        internal void OnNewOnlyHas(params Event<T>[] events)
        {
            _interceptions.OnNew.Should().HaveCount(events.Length);
            _interceptions.OnNew.Should().Contain(events);
        }

        internal void OnNewOnlyHasContents(params T[] contents)
        {
            _interceptions.OnNew.Should().HaveCount(contents.Length);
            _interceptions.OnNew.Select(ev => ev.Content).Should().Contain(contents);
        }

        public void OnAllSucceededIsEmpty()
            => _interceptions.OnAllSucceeded.Should().BeEmpty();

        public void OnAllSucceededOnlyHasId(EventId id)
            => _interceptions.OnAllSucceeded.Should().OnlyContain(i => i == id);

        public void OnAnyFailedIsEmpty()
            => _interceptions.OnAnyFailed.Should().BeEmpty();

        public void OnAnyFailedOnlyHasId(EventId id)
            => _interceptions.OnAnyFailed.Should().OnlyContain(i => i.Id == id);

        public void OnCompletedWasCalled()
            => _interceptions.OnCompletedIntercepted.Should().BeTrue();

        public void OnCompletedWasNotCalled()
            => _interceptions.OnCompletedIntercepted.Should().BeFalse();
    }
}
