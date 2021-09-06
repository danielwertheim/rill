using System;
using FluentAssertions;
using Rill;
using Xunit;

namespace UnitTests
{
    public class RillReferenceTests
    {
        [Fact]
        public void Can_be_constructed()
        {
            var name = Fake.Strings.Random();

            var constructed = RillReference.New(name);

            constructed.Name.Should().Be(name);
            constructed.Id.Should().NotBeEmpty();
        }

        [Fact]
        public void Can_be_reconstructed()
        {
            var org = RillReference.New(Fake.Strings.Random());

            var reconstructed = RillReference.From(org.Name, org.Id);

            org.Should().Be(reconstructed);
        }

        [Fact]
        public void Can_not_be_constructed_if_parts_are_missing() =>
            FluentActions
                .Invoking(() => RillReference.New(string.Empty))
                .Should().Throw<ArgumentException>()
                .And.ParamName.Should().Be("name");

        [Fact]
        public void Can_not_be_reconstructed_if_parts_are_missing()
        {
            FluentActions
                .Invoking(() => RillReference.From(string.Empty, string.Empty))
                .Should().Throw<ArgumentException>()
                .And.ParamName.Should().Be("name");
            FluentActions
                .Invoking(() => RillReference.From(Fake.Strings.Random(), string.Empty))
                .Should().Throw<ArgumentException>()
                .And.ParamName.Should().Be("id");
        }

        [Fact]
        public void Uses_value_equality()
        {
            var a = RillReference.From("a", "b");
            var b = RillReference.From("A", "B");

            a.Equals(b).Should().BeTrue();
            a.Should().Be(b);
        }

        [Fact]
        public void Can_be_handled_as_a_string()
        {
            var org = RillReference.New(Fake.Strings.Random());
            var orgAsString = (string)org;

            orgAsString.Should().Be($"{org.Name}:{org.Id}");
            org.ToString().Should().Be(orgAsString);

            var copy = RillReference.From(orgAsString);
            copy.Should().Be(org);
        }

        [Theory]
        [InlineData("a")]
        [InlineData("a:b:c")]
        public void Can_not_be_reconstructed_from_single_string_If_not_exactly_two_parts(string value) =>
            FluentActions
                .Invoking(() => RillReference.From(value))
                .Should().Throw<ArgumentException>()
                .And.ParamName.Should().Be("value");
    }
}
