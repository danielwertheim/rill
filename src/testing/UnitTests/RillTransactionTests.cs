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
            rill.Emit(Fake.Events.Single());

            Func<Task> failing = async () => await sut.CommitAsync(store);

            failing.Should().ThrowExactly<InvalidOperationException>().WithMessage("Can not commit when there's knowledge about a failed event.");
        }

        [Fact]
        public async Task Committing_Appends_events_in_sequence()
        {
            var (rill, store, sut) = NewScenario<string>();
            var expectedEvents = Fake.Events.Many();
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
            var expectedEvents = Fake.Events.Many();
            rill.Emit(expectedEvents[0]);
            rill.Emit(expectedEvents[1]);
            rill.Emit(expectedEvents[^1]);

            var commit = await sut.CommitAsync(store);

            commit.Reference.Should().Be(rill.Reference);
            commit.SequenceRange.Should().Be(SequenceRange.From(expectedEvents.First().Sequence, expectedEvents.Last().Sequence));
            commit.Events.Should().BeInAscendingOrder(e => e.Sequence);
            commit.Events.Should().Contain(expectedEvents);
        }

        [Fact]
        public async Task Can_handle_multiple_commits()
        {
            var (rill, store, sut) = NewScenario<string>();
            var expectedEvent1 = Fake.Events.Single();
            var expectedEvent2 = Fake.Events.Single(1);

            rill.Emit(expectedEvent1);
            var commit1 = await sut.CommitAsync(store);

            rill.Emit(expectedEvent2);
            var commit2 = await sut.CommitAsync(store);

            commit1.Reference.Should().Be(rill.Reference);
            commit1.SequenceRange.Should().Be(SequenceRange.From(expectedEvent1.Sequence, expectedEvent1.Sequence));
            commit1.Events.Should().HaveCount(1);
            commit1.Events.Should().BeInAscendingOrder(e => e.Sequence);
            commit1.Events.Should().Contain(expectedEvent1);

            commit2.Reference.Should().Be(rill.Reference);
            commit2.SequenceRange.Should().Be(SequenceRange.From(expectedEvent2.Sequence, expectedEvent2.Sequence));
            commit2.Events.Should().HaveCount(1);
            commit2.Events.Should().BeInAscendingOrder(e => e.Sequence);
            commit2.Events.Should().Contain(expectedEvent2);
        }
    }
}
