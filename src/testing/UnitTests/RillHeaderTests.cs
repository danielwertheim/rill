using System;
using FluentAssertions;
using Rill;
using Xunit;

namespace UnitTests
{
    public class RillHeaderTests
    {
        [Fact]
        public void New_uses_sensible_defaults()
        {
            var reference = RillReference.New("test");

            var header = RillHeader.New(reference);

            header.Reference.Should().Be(reference);
            header.Sequence.Should().Be(Sequence.None);
            header.LastChangedAt.Should().Be(header.CreatedAt);
            ((DateTime)header.CreatedAt).Should().BeCloseTo(DateTime.UtcNow);
            ((DateTime)header.LastChangedAt).Should().BeCloseTo(DateTime.UtcNow);
        }

        [Fact]
        public void Can_not_be_constructed_When_last_changed_takes_presence_before_created()
        {
            var ts = DateTime.UtcNow;

            Action failing = () => RillHeader.From(
                RillReference.New("test"),
                Sequence.First,
                Timestamp.From(ts),
                Timestamp.From(ts.AddMilliseconds(-1)));

            failing.Should()
                .ThrowExactly<ArgumentException>()
                .WithMessage("Last changed can not take presence before Created timestamp.");
        }
    }
}
