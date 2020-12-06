using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Rill;
using Rill.Extensions;
using Xunit;

namespace UnitTests
{
    public class RillTransactionTests
    {
        private static (IRill<T> Rill, InterceptingStore<T>, IRillTransaction<T> Transaction) NewScenario<T>()
        {
            var rill = RillFactory.Synchronous<T>(RillReference.New("rill-tran-tests"));
            var tran = RillTransaction.Begin(rill);

            return (rill, new InterceptingStore<T>(), tran);
        }

        private static Event<string> CreateEvent(int n = 0)
            => Event<string>.Create(Fake.Strings.Random(), EventId.New(), n == 0 ? Sequence.First : Sequence.First.Add(n));

        private static Event<string>[] CreateEvents()
            => new[]
            {
                CreateEvent(),
                CreateEvent(1),
                CreateEvent(2)
            };

        [Fact]
        public void Requires_at_least_one_event()
        {
            var (_, store, sut) = NewScenario<string>();

            Func<Task> failing = async () => await sut.CommitAsync(store);

            failing.Should().ThrowExactly<InvalidOperationException>().WithMessage("Can not commit when no events has been intercepted.");
        }

        [Fact]
        public void Requires_that_no_event_has_failed()
        {
            var (rill, store, sut) = NewScenario<string>();
            rill.Consume.Subscribe(e => throw new Exception("FAIL!"));
            rill.Emit(CreateEvent());

            Func<Task> failing = async () => await sut.CommitAsync(store);

            failing.Should().ThrowExactly<InvalidOperationException>().WithMessage("Can not commit when there's knowledge about a failed event.");
        }

        [Fact]
        public async Task Committing_Appends_events_in_sequence()
        {
            var (rill, store, sut) = NewScenario<string>();
            var expectedEvents = CreateEvents();
            rill.Emit(expectedEvents[0]);
            rill.Emit(expectedEvents[1]);
            rill.Emit(expectedEvents[^1]);

            await sut.CommitAsync(store);

            store.HasAppendCount(1);
            store.Appended(expectedEvents);
        }

        [Fact]
        public async Task Committing_returns_commit()
        {
            var (rill, store, sut) = NewScenario<string>();
            var expectedEvents = CreateEvents();
            rill.Emit(expectedEvents[0]);
            rill.Emit(expectedEvents[1]);
            rill.Emit(expectedEvents[^1]);

            var commit = await sut.CommitAsync(store) ?? throw new Exception("Should have returned commit!");

            commit.Reference.Should().Be(rill.Reference);
            commit.Revision.Should().Be(Revision.From(expectedEvents.First().Sequence, expectedEvents.Last().Sequence));
            commit.Events.Should().BeInAscendingOrder(e => e.Sequence);
            commit.Events.Should().Contain(expectedEvents);
        }
    }
}
