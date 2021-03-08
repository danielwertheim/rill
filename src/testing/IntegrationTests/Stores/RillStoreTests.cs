using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentAssertions;
using Rill;
using Xunit;

namespace IntegrationTests.Stores
{
    internal static class AsyncEnumerableExtensions
    {
        internal static async Task<IList<T>> ToListAsync<T>(this IAsyncEnumerable<T> asyncEnumerable)
        {
            var l = new List<T>();

            await foreach (var i in asyncEnumerable)
                l.Add(i);

            return l;
        }
    }

    public abstract class RillStoreTests<T> where T : IRillStore
    {
        private Func<T> NewSut { get; }

        protected RillStoreTests(Func<T> newSut)
            => NewSut = newSut;

        private static RillCommit NewCommit(RillReference? reference = default, int sequenceSeed = 0)
            => RillCommit.New(reference ?? RillReference.New("test"), Fake.Events.Many(sequenceSeed).ToImmutableList());

        [Fact]
        public async Task Returns_null_When_getting_a_non_existing_rill()
        {
            var sut = NewSut();
            var reference = RillReference.New("test");

            var header = await sut.GetDetailsAsync(reference);

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

            var header = await sut.GetDetailsAsync(reference);
            var commits = sut.ReadCommits(reference);

            header.Should().NotBeNull();
            header!.Reference.Should().Be(reference);
            header!.Sequence.Should().Be(commit2.SequenceRange.Upper);
            ((DateTime)header!.CreatedAt).Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(500));
            header!.LastChangedAt.Should().Be(commit2.Timestamp);

            commits.Should().BeEquivalentTo(commit1, commit2);
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
        public async Task Can_retrieve_commits_using_sequence_range_filtering()
        {
            var sut = NewSut();
            var reference = RillReference.New("test");
            var commit1 = RillCommit.New(
                reference,
                Fake.Events.Many(0, 10).ToImmutableList());
            var commit2 = RillCommit.New(
                reference,
                Fake.Events.Many(10, 10).ToImmutableList());
            await sut.AppendAsync(commit1);
            await sut.AppendAsync(commit2);

            sut
                .ReadCommits(reference, SequenceRange.Any)
                .Should().BeEquivalentTo(commit1, commit2);
            sut
                .ReadCommits(reference, SequenceRange.From(Sequence.From(8), Sequence.From(11)))
                .Should().BeEquivalentTo(commit1, commit2);
            sut
                .ReadCommits(reference, SequenceRange.From(Sequence.From(3), Sequence.From(6)))
                .Should().ContainEquivalentOf(commit1);
            sut
                .ReadCommits(reference, SequenceRange.From(Sequence.From(11), Sequence.From(11)))
                .Should().ContainEquivalentOf(commit2);
            sut
                .ReadCommits(reference, SequenceRange.From(Sequence.From(100), Sequence.From(200)))
                .Should().BeEmpty();
        }

        [Fact]
        public async Task Can_retrieve_commits_asynchronously_using_sequence_range_filtering()
        {
            var sut = NewSut();
            var reference = RillReference.New("test");
            var commit1 = RillCommit.New(
                reference,
                Fake.Events.Many(0, 10).ToImmutableList());
            var commit2 = RillCommit.New(
                reference,
                Fake.Events.Many(10, 10).ToImmutableList());
            await sut.AppendAsync(commit1);
            await sut.AppendAsync(commit2);

            (await sut
                .ReadCommitsAsync(reference, SequenceRange.Any)
                .ToListAsync())
                .Should().BeEquivalentTo(commit1, commit2);
            (await sut
                .ReadCommitsAsync(reference, SequenceRange.From(Sequence.From(8), Sequence.From(11)))
                .ToListAsync())
                .Should().BeEquivalentTo(commit1, commit2);
            (await sut
                .ReadCommitsAsync(reference, SequenceRange.From(Sequence.From(3), Sequence.From(6)))
                .ToListAsync())
                .Should().ContainEquivalentOf(commit1);
            (await sut
                .ReadCommitsAsync(reference, SequenceRange.From(Sequence.From(11), Sequence.From(11)))
                .ToListAsync())
                .Should().ContainEquivalentOf(commit2);
            (await sut
                .ReadCommitsAsync(reference, SequenceRange.From(Sequence.From(100), Sequence.From(200)))
                .ToListAsync())
                .Should().BeEmpty();
        }

        [Fact]
        public async Task Can_delete_a_Rill()
        {
            var sut = NewSut();
            var ref1 = RillReference.New("test");
            var ref2 = RillReference.New("test");
            var commitAgainstRef1 = RillCommit.New(ref1, Fake.Events.Many(0, 10).ToImmutableList());
            var commitAgainstRef2 = RillCommit.New(ref2, Fake.Events.Many(0, 10).ToImmutableList());
            await sut.AppendAsync(commitAgainstRef1);
            await sut.AppendAsync(commitAgainstRef2);

            await sut.DeleteAsync(ref1);

            (await sut.GetDetailsAsync(ref1)).Should().BeNull();
            (await sut.GetDetailsAsync(ref2)).Should().NotBeNull();
        }

        [Fact]
        public void Can_handle_deletes_When_no_rill_exists()
        {
            NewSut()
                .Invoking(async sut => await sut.DeleteAsync(RillReference.New("test")))
                .Should().NotThrow();
        }
    }
}
