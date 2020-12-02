using System;
using FluentAssertions;
using Rill;
using Xunit;

namespace UnitTests
{
    public class RevisionTests
    {
        [Fact]
        public void Requires_logic_order_of_range()
        {
            Action failing = () => Revision.From(Sequence.Max, Sequence.First);

            failing.Should().ThrowExactly<ArgumentException>().WithMessage("Range must be from low to high.");
        }

        [Fact]
        public void Range_can_represent_a_sequence_of_one_item()
        {
            var r = Revision.From(Sequence.First, Sequence.First);

            r.Lower.Should().Be(Sequence.First);
            r.Upper.Should().Be(Sequence.First);
        }

        [Fact]
        public void Range_can_represent_a_sequence_of_many_items()
        {
            var lower = Sequence.First;
            var upper = lower.Add(10);

            var r = Revision.From(lower, upper);

            r.Lower.Should().Be(lower);
            r.Upper.Should().Be(upper);
        }

        [Fact]
        public void Can_determine_if_sequence_is_in_range()
        {
            var r = Revision.From(Sequence.First, Sequence.First.Add(2));

            r.Includes(Sequence.First).Should().BeTrue();
            r.Includes(Sequence.First.Add(1)).Should().BeTrue();
            r.Includes(Sequence.First.Add(2)).Should().BeTrue();
            r.Includes(Sequence.First.Add(3)).Should().BeFalse();
        }

        [Fact]
        public void Supports_value_equality()
        {
            const int lower = 5;
            const int upper = 10;
            var r = Revision.From(Sequence.First.Add(lower), Sequence.First.Add(upper));
            var same = Revision.From(Sequence.First.Add(lower), Sequence.First.Add(upper));
            var toHighUpper = Revision.From(Sequence.First, Sequence.First.Add(upper+1));
            var toHighLower = Revision.From(Sequence.First.Add(lower+1), Sequence.First.Add(upper));
            var toLowLower = Revision.From(Sequence.First.Add(lower-1), Sequence.First.Add(upper));
            var toLowUpper = Revision.From(Sequence.First.Add(lower-1), Sequence.First.Add(upper));

            bool Compare(Revision s) => r == s;

            Compare(same).Should().BeTrue();
            Compare(toHighLower).Should().BeFalse();
            Compare(toHighUpper).Should().BeFalse();
            Compare(toLowLower).Should().BeFalse();
            Compare(toLowUpper).Should().BeFalse();
        }
    }
}
