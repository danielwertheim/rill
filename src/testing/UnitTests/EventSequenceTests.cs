using System;
using FluentAssertions;
using Rill;
using Xunit;

namespace UnitTests
{
    public class EventSequenceTests
    {
        [Fact]
        public void None_represents_a_sequence_that_has_not_yet_incremented()
        {
            (EventSequence.None == 0).Should().BeTrue();
            EventSequence.None.Should().Be(EventSequence.None);
            EventSequence.None.IsNone().Should().BeTrue();
            EventSequence.None.Increment().Should().NotBe(EventSequence.None);
        }

        [Fact]
        public void First_represents_a_sequence_that_has_incremented_one_step()
        {
            (EventSequence.First == 1).Should().BeTrue();
            EventSequence.First.Should().Be(EventSequence.First);
            EventSequence.First.IsFirst().Should().BeTrue();
            EventSequence.First.Increment().Should().NotBe(EventSequence.First);
        }

        [Fact]
        public void Max_represents_the_max_value_of_a_sequence()
        {
            (EventSequence.Max == long.MaxValue).Should().BeTrue();
            EventSequence.Max.Should().Be(EventSequence.Max);
            EventSequence.Max.IsMax().Should().BeTrue();
        }

        [Fact]
        public void Adding_is_possible_to_all_but_max_sequence()
        {
            EventSequence.None.Add(1).Should().Be(EventSequence.First);
            EventSequence.None.Add(2).Should().Be(EventSequence.First.Add(1));
            EventSequence.From(40).Add(2).Should().BeEquivalentTo(EventSequence.From(42));

            Action failing = () => EventSequence.Max.Add(1);
            failing.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Can_determine_if_a_value_is_in_between_inclusive()
        {
            EventSequence.From(10).IsBetweenInclusive(9, 11).Should().BeTrue();
            EventSequence.From(10).IsBetweenInclusive(10, 11).Should().BeTrue();
            EventSequence.From(10).IsBetweenInclusive(9, 10).Should().BeTrue();
            EventSequence.From(10).IsBetweenInclusive(10, 10).Should().BeTrue();

            EventSequence.From(10).IsBetweenInclusive(1, 9).Should().BeFalse();
            EventSequence.From(10).IsBetweenInclusive(11, 20).Should().BeFalse();
        }
    }
}
