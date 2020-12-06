using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Rill;
using Rill.Stores.InMemory;
using Xunit;

namespace UnitTests.Stores
{
    public class InMemoryRillStoreTests
    {
        private static IRillStore<string> NewSut()
            => new InMemoryRillStore<string>();

        private static IRillCommit<string> NewCommit(RillReference? reference = default, int sequenceSeed = 0)
            => RillCommit.New(reference ?? RillReference.New("test"), Fake.Events.Many(sequenceSeed).ToImmutableList());

        [Fact]
        public async Task Returns_null_When_getting_a_non_existing_header()
        {
            var sut = NewSut();
            var reference = RillReference.New("test");

            var header = await sut.GetHeaderAsync(reference);

            header.Should().BeNull();
        }

        [Fact]
        public async Task Can_store_and_retrieve_events()
        {
            var sut = NewSut();
            var reference = RillReference.New("test");
            var commit1 = NewCommit(reference);
            var commit2 = NewCommit(reference, commit1.Events.Count);

            await sut.AppendAsync(commit1);
            await sut.AppendAsync(commit2);

            var header = await sut.GetHeaderAsync(reference);
            var events = sut.ReadEvents(reference);

            header.Should().NotBeNull();
            header!.Reference.Should().Be(reference);
            header!.Sequence.Should().Be(commit2.SequenceRange.Upper);
            ((DateTime)header!.CreatedAt).Should().BeCloseTo(DateTime.UtcNow);
            header!.LastChangedAt.Should().Be(commit2.Timestamp);

            events.Should().BeEquivalentTo(commit1.Events.Union(commit2.Events));
        }

        [Fact]
        public async Task Can_not_store_a_commit_with_an_outdated_sequence()
        {
            const int sequenceSeed = 0;
            var sut = NewSut();
            var reference = RillReference.New("test");
            var commit1 = NewCommit(reference, sequenceSeed);
            var commit2 = NewCommit(reference, sequenceSeed);

            await sut.AppendAsync(commit1);

            Func<Task> failing = async () => await sut.AppendAsync(commit2);

            failing.Should()
                .ThrowExactly<RillStoreConcurrencyException>()
                .Where(ex => ex.Reference == reference &&
                             ex.CurrentSequence == commit1.SequenceRange.Upper &&
                             ex.ExpectedSequence == commit2.SequenceRange.Lower);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(42)]
        public async Task Can_not_store_a_commit_with_a_gap_between_sequences(int gap)
        {
            var sut = NewSut();
            var reference = RillReference.New("test");
            var commit1 = NewCommit(reference, sequenceSeed: 0);
            var commit2 = NewCommit(reference, sequenceSeed: commit1.Events.Count + gap);

            await sut.AppendAsync(commit1);

            Func<Task> failing = async () => await sut.AppendAsync(commit2);

            failing.Should()
                .ThrowExactly<RillStoreConcurrencyException>()
                .Where(ex => ex.Reference == reference &&
                             ex.CurrentSequence == commit1.SequenceRange.Upper &&
                             ex.ExpectedSequence == commit2.SequenceRange.Lower);
        }

        [Fact]
        public async Task Can_retrieve_events_using_sequence_range_filtering()
        {
            var sut = NewSut();
            var reference = RillReference.New("test");
            var events = Fake.Events.Many(0, 10).ToImmutableList();
            await sut.AppendAsync(RillCommit.New(reference, events));

            sut
                .ReadEvents(reference, SequenceRange.Any)
                .Should().BeEquivalentTo(events);
            sut
                .ReadEvents(reference, SequenceRange.From(Sequence.From(3), Sequence.From(6)))
                .Should().BeEquivalentTo(events.Skip(2).Take(4));
        }
    }
}
