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
        public void Can_not_be_constructed_if_parts_are_missing()
        {
            Action missingName = () => RillReference.New(string.Empty);

            missingName.Should().Throw<ArgumentException>().And.ParamName.Should().Be("name");
        }

        [Fact]
        public void Can_not_be_reconstructed_if_parts_are_missing()
        {
            Action missingName = () => RillReference.From(string.Empty, string.Empty);
            Action missingId = () => RillReference.From(Fake.Strings.Random(), string.Empty);

            missingName.Should().Throw<ArgumentException>().And.ParamName.Should().Be("name");
            missingId.Should().Throw<ArgumentException>().And.ParamName.Should().Be("id");
        }
    }
}
