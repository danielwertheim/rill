using System;
using FluentAssertions;
using Rill;
using Xunit;

namespace UnitTests
{
    public class SequenceTests
    {
        [Fact]
        public void None_represents_a_sequence_that_has_not_yet_incremented()
        {
            (Sequence.None == 0).Should().BeTrue();
            Sequence.None.Should().Be(Sequence.None);
            Sequence.None.IsNone().Should().BeTrue();
            Sequence.None.Increment().Should().NotBe(Sequence.None);
        }

        [Fact]
        public void First_represents_a_sequence_that_has_incremented_one_step()
        {
            (Sequence.First == 1).Should().BeTrue();
            Sequence.First.Should().Be(Sequence.First);
            Sequence.First.IsFirst().Should().BeTrue();
            Sequence.First.Increment().Should().NotBe(Sequence.First);
        }

        [Fact]
        public void Max_represents_the_max_value_of_a_sequence()
        {
            (Sequence.Max == long.MaxValue).Should().BeTrue();
            Sequence.Max.Should().Be(Sequence.Max);
            Sequence.Max.IsMax().Should().BeTrue();
        }

        [Fact]
        public void Adding_is_possible_to_all_but_max_sequence()
        {
            Sequence.None.Add(1).Should().Be(Sequence.First);
            Sequence.None.Add(2).Should().Be(Sequence.First.Add(1));
            Sequence.From(40).Add(2).Should().BeEquivalentTo(Sequence.From(42));

            Action failing = () => Sequence.Max.Add(1);
            failing.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Can_determine_if_a_value_is_in_between_inclusive()
        {
            Sequence.From(10).IsBetweenInclusive(9, 11).Should().BeTrue();
            Sequence.From(10).IsBetweenInclusive(10, 11).Should().BeTrue();
            Sequence.From(10).IsBetweenInclusive(9, 10).Should().BeTrue();
            Sequence.From(10).IsBetweenInclusive(10, 10).Should().BeTrue();

            Sequence.From(10).IsBetweenInclusive(1, 9).Should().BeFalse();
            Sequence.From(10).IsBetweenInclusive(11, 20).Should().BeFalse();
        }

        [Fact]
        public void Can_be_compared_against_null()
        {
            Sequence nonNullable = Sequence.First;
            Sequence? nullable = null;

            (nonNullable != null).Should().BeTrue();
            (nullable == null).Should().BeTrue();
        }

        [Fact]
        public void Is_immutable()
        {
            var initial = Sequence.None;

            var seq1 = initial.Increment();
            var seq2 = initial.Add(1);

            initial.Should().NotBe(seq1);
            initial.Should().NotBe(seq2);
            seq1.Should().Be(seq2);
        }

        [Fact]
        public void Copying_produces_a_new_instance()
        {
            var initial = Sequence.None;

            var copy = initial.Copy();

            ReferenceEquals(initial, copy).Should().BeFalse();
            copy.Should().Be(initial);
        }
    }
}
