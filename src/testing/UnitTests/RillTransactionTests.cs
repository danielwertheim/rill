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
        public async Task Committing_When_empty_does_not_call_store_and_yields_no_commit()
        {
            var (_, store, sut) = NewScenario<string>();

            var commit = await sut.CommitAsync(store);

            commit.Should().BeNull();
            store.HasNoAppends();
        }

        [Fact]
        public async Task Committing_Appends_events_in_sequence()
        {
            var (rill, store, sut) = NewScenario<string>();
            var expectedEvents = new[]
            {
                Event<string>.Create(Fake.Strings.Random(), EventId.New(), Sequence.First),
                Event<string>.Create(Fake.Strings.Random(), EventId.New(), Sequence.First.Increment())
            };
            rill.Emit(expectedEvents.First());
            rill.Emit(expectedEvents.Last());

            await sut.CommitAsync(store);

            store.HasAppendCount(1);
            store.Appended(
                expectedEvents.First(),
                expectedEvents.Last());
        }

        [Fact]
        public async Task Committing_returns_commit()
        {
            var (rill, store, sut) = NewScenario<string>();
            var expectedEvents = new[]
            {
                Event<string>.Create(Fake.Strings.Random(), EventId.New(), Sequence.First),
                Event<string>.Create(Fake.Strings.Random(), EventId.New(), Sequence.First.Add(1)),
                Event<string>.Create(Fake.Strings.Random(), EventId.New(), Sequence.First.Add(2))
            };
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
