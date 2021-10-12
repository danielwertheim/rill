using System;
using FluentAssertions;
using Rill;
using Xunit;

namespace UnitTests
{
    public class RillReferenceTests
    {
        [Theory]
        [InlineData("1")]
        [InlineData("!")]
        [InlineData("a1")]
        [InlineData("a!")]
        public void Requires_name_to_be_letters_only(string nonLetterString)
        {
            FluentActions
                .Invoking(() => RillReference.New(nonLetterString))
                .Should().Throw<ArgumentException>()
                .And.ParamName.Should().Be("name");
        }

        [Theory]
        [InlineData("!")]
        public void Requires_id_to_be_letters_or_numbers_only(string nonLetterString)
        {
            FluentActions
                .Invoking(() => RillReference.From(Fake.Strings.RandomLettersUpperAndLowerCase(), nonLetterString))
                .Should().Throw<ArgumentException>()
                .And.ParamName.Should().Be("id");
        }

        [Fact]
        public void Can_be_constructed()
        {
            var name = Fake.Strings.RandomLettersUpperAndLowerCase();

            var constructed = RillReference.New(name);

            constructed.Name.Should().Be(name);
            constructed.Id.Should().NotBeEmpty();
        }

        [Fact]
        public void Can_be_reconstructed()
        {
            var org = RillReference.New(Fake.Strings.RandomLettersUpperAndLowerCase());

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
                .Invoking(() => RillReference.From(Fake.Strings.RandomLettersUpperAndLowerCase(), string.Empty))
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
            var org = RillReference.New(Fake.Strings.RandomLettersUpperAndLowerCase());
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
