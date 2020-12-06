using System;
using FluentAssertions;
using Rill;
using Xunit;

namespace UnitTests
{
    public class SequenceRangeTests
    {
        [Fact]
        public void Requires_logic_order_of_range()
        {
            Action failing = () => SequenceRange.From(Sequence.Max, Sequence.First);

            failing.Should().ThrowExactly<ArgumentException>().WithMessage("Range must be from low to high.");
        }

        [Fact]
        public void Any_covers_full_spectrum()
        {
            SequenceRange.Any.Lower.Should().Be(Sequence.First);
            SequenceRange.Any.Upper.Should().Be(Sequence.Max);
        }

        [Fact]
        public void From_first_to_accepts_upper_bound_and_uses_first_as_lower()
        {
            var r = SequenceRange.FirstTo(Sequence.First.Add(2));

            r.Lower.Should().Be(Sequence.First);
            r.Upper.Should().Be(Sequence.First.Add(2));
        }

        [Fact]
        public void Range_can_represent_a_sequence_of_one_item()
        {
            var r = SequenceRange.From(Sequence.First, Sequence.First);

            r.Lower.Should().Be(Sequence.First);
            r.Upper.Should().Be(Sequence.First);
        }

        [Fact]
        public void Range_can_represent_a_sequence_of_many_items()
        {
            var lower = Sequence.First;
            var upper = lower.Add(10);

            var r = SequenceRange.From(lower, upper);

            r.Lower.Should().Be(lower);
            r.Upper.Should().Be(upper);
        }

        [Fact]
        public void Can_determine_if_sequence_is_in_range()
        {
            var r = SequenceRange.From(Sequence.First, Sequence.First.Add(2));

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
            var r = SequenceRange.From(Sequence.First.Add(lower), Sequence.First.Add(upper));
            var same = SequenceRange.From(Sequence.First.Add(lower), Sequence.First.Add(upper));
            var toHighUpper = SequenceRange.From(Sequence.First, Sequence.First.Add(upper+1));
            var toHighLower = SequenceRange.From(Sequence.First.Add(lower+1), Sequence.First.Add(upper));
            var toLowLower = SequenceRange.From(Sequence.First.Add(lower-1), Sequence.First.Add(upper));
            var toLowUpper = SequenceRange.From(Sequence.First.Add(lower-1), Sequence.First.Add(upper));

            bool Compare(SequenceRange s) => r == s;

            Compare(same).Should().BeTrue();
            Compare(toHighLower).Should().BeFalse();
            Compare(toHighUpper).Should().BeFalse();
            Compare(toLowLower).Should().BeFalse();
            Compare(toLowUpper).Should().BeFalse();
        }
    }
}
