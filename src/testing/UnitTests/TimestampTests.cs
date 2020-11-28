using System;
using FluentAssertions;
using Rill;
using Xunit;

namespace UnitTests
{
    public class TimestampTests
    {
        [Fact]
        public void Generates_an_utc_timestamp()
        {
            var dt = (DateTime) Timestamp.New();

            dt.Should().BeCloseTo(DateTime.UtcNow);
        }

        [Fact]
        public void Can_be_reconstructed()
        {
            var org = DateTime.UtcNow;

            (Timestamp.From(org) == org).Should().BeTrue();
        }

        [Fact]
        public void Must_be_non_default_and_utc()
        {
            Action a1 = () => Timestamp.From(DateTime.MinValue);
            Action a2 = () => Timestamp.From(default);
            Action a3 = () => Timestamp.From(DateTime.Now);

            a1.Should().Throw<ArgumentException>();
            a2.Should().Throw<ArgumentException>();
            a3.Should().Throw<ArgumentException>();
        }
    }
}
