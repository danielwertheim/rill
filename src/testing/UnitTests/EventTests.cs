using System;
using FluentAssertions;
using Rill;
using Xunit;

namespace UnitTests
{
    public class EventTests
    {
        [Theory]
        [InlineData("c930c33087f644afa615d65b07f99266", "c930c33087f644afa615d65b07f99266", true)]
        [InlineData("c930c33087f644afa615d65b07f99266", "3bda4395240d4f62a90c60237d35e97c", false)]
        public void Supports_value_equality_via_Id(string id1, string id2, bool expectedToBeEqual)
        {
            var ev1 = Event<string>.New("123", EventId.From(id1));
            var ev2 = Event<string>.New("321", EventId.From(id2));

            (ev1 == ev2).Should().Be(expectedToBeEqual);
        }

        [Fact]
        public void Can_cast()
        {
            var orgEv = Event<string>.New("test");

            orgEv.TryCast<object>(out var objEv).Should().BeTrue();
            objEv!.TryCast<string>(out var strEv).Should().BeTrue();
            (strEv == orgEv).Should().BeTrue();
        }

        [Fact]
        public void Indicates_if_cast_fails()
        {
            var orgEv = Event<string>.New("test");

            orgEv.TryCast<Exception>(out _).Should().BeFalse();
        }
    }
}
