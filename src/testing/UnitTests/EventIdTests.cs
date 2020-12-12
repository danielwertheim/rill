using System;
using FluentAssertions;
using Rill;
using Xunit;

namespace UnitTests
{
    public class EventIdTests
    {
        [Fact]
        public void Generates_non_empty_guid_equivalents_by_default()
        {
            (EventId.New() != Guid.Empty).Should().BeTrue();
            (EventId.New() == Guid.Empty).Should().BeFalse();
            ((Guid) EventId.New()).Should().NotBeEmpty();
        }

        [Fact]
        public void Can_be_reconstructed()
        {
            var org = Guid.NewGuid();

            (EventId.From(org) == org).Should().BeTrue();
        }

        [Fact]
        public void Can_be_reconstructed_from_guid_string()
        {
            var org = Guid.NewGuid();

            (EventId.From(org.ToString()) == org).Should().BeTrue();
        }

        [Fact]
        public void Can_not_be_created_from_an_empty_guid()
        {
            Action a = () => EventId.From(Guid.Empty);

            a.Should().Throw<ArgumentException>();
        }
    }
}
