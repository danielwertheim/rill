using System;
using System.Collections.Generic;
using FluentAssertions;
using Rill;
using Rill.Extensions;
using Xunit;

namespace UnitTests.Rills
{
    public class SynchronousRillTests
    {
        private static IRill NewSut()
            => RillFactory.Synchronous(RillReference.New("sync-rill-tests"));

        [Fact]
        public void Returns_event_after_emitting()
        {
            var content = Fake.Strings.Random();
            var id = EventId.New();
            var seq = Sequence.First;
            var sut = NewSut();

            var ev = sut.Emit(content, id, seq);

            ev.Id.Should().Be(id);
            ev.Content.Should().Be(content);
            ev.Sequence.Should().Be(seq);
            sut.Sequence.Should().Be(seq);
        }

        [Fact]
        public void Requires_event_sequence_to_be_in_order()
        {
            var sut = NewSut();
            var ev1 = sut.Emit("first");

            Action historicSequence = () => sut.Emit("second", sequence: Sequence.First);
            Action futureSequence = () => sut.Emit("second", sequence: ev1.Sequence.Add(2));

            historicSequence
                .Should()
                .Throw<EventOutOfOrderException>()
                .Where(ex => ex.Actual == Sequence.First && ex.Expected == ev1.Sequence.Increment());
            futureSequence
                .Should()
                .Throw<EventOutOfOrderException>()
                .Where(ex => ex.Actual == ev1.Sequence.Add(2) && ex.Expected == ev1.Sequence.Increment());
        }

        [Fact]
        public void Unsubscribed_consumer_is_not_invoked_at_all()
        {
            var unsubscribing = InterceptingConsumer.Behaving();
            var unsubscribingDelegating = new Interceptions();
            var sut = NewSut();
            sut.Subscribe(_ => throw new Exception("FAIL"));
            var sub1 = sut.Subscribe(unsubscribing);
            var sub2 = sut.Subscribe(
                unsubscribingDelegating.InOnNew,
                unsubscribingDelegating.InOnAllSucceeded,
                unsubscribingDelegating.InOnAnyFailed);

            sub1.Dispose();
            sub2.Dispose();

            sut.Emit(Fake.Strings.Random());
            unsubscribing.Ensure().ToBeUnTouched();
            unsubscribingDelegating.Ensure().ToBeUnTouched();
        }

        [Fact]
        public void Emits_events_to_all_active_consumers_even_failed_ones()
        {
            var behaving = InterceptingConsumer.Behaving();
            var behavingDelegating = new Interceptions();
            var misbehaving = InterceptingConsumer.Misbehaving(ev => ev.Sequence > Sequence.First);
            var misbehavingDelegating = new Interceptions();
            var sut = NewSut();
            sut.Subscribe(behaving);
            sut.Subscribe(
                behavingDelegating.InOnNew,
                behavingDelegating.InOnAllSucceeded,
                behavingDelegating.InOnAnyFailed);
            sut.Subscribe(misbehaving);
            sut.Subscribe(
                ev =>
                {
                    misbehavingDelegating.InOnNew(ev);
                    if (ev.Sequence > Sequence.First)
                        throw new Exception(ev.ToString());
                },
                misbehavingDelegating.InOnAllSucceeded,
                misbehavingDelegating.InOnAnyFailed);

            var ev1 = sut.Emit(Fake.Strings.Random());
            var ev2 = sut.Emit(Fake.Strings.Random());

            behaving.Ensure().OnNewOnlyHas(ev1, ev2);
            behaving.Ensure().OnAllSucceededOnlyHasId(ev1.Id);
            behaving.Ensure().OnAnyFailedOnlyHasId(ev2.Id);

            behavingDelegating.Ensure().OnNewOnlyHas(ev1, ev2);
            behavingDelegating.Ensure().OnAllSucceededOnlyHasId(ev1.Id);
            behavingDelegating.Ensure().OnAnyFailedOnlyHasId(ev2.Id);

            misbehaving.Ensure().OnNewOnlyHas(ev1, ev2);
            misbehaving.Ensure().OnAllSucceededOnlyHasId(ev1.Id);
            misbehaving.Ensure().OnAnyFailedOnlyHasId(ev2.Id);

            misbehavingDelegating.Ensure().OnNewOnlyHas(ev1, ev2);
            misbehavingDelegating.Ensure().OnAllSucceededOnlyHasId(ev1.Id);
            misbehavingDelegating.Ensure().OnAnyFailedOnlyHasId(ev2.Id);
        }

        [Fact]
        public void Catch_catches_for_failed_consumer_only_and_does_not_notify_any_consumer_of_failure()
        {
            var behaving = InterceptingConsumer.Behaving();
            var misbehaving = InterceptingConsumer.Misbehaving();
            var behavingGotCatch = false;
            var misbehavingGotCatch = false;
            var sut = NewSut();
            sut
                .Catch<Exception>(_ => behavingGotCatch = true)
                .Subscribe(behaving);
            sut
                .Catch<Exception>(_ => misbehavingGotCatch = true)
                .Subscribe(misbehaving);

            sut.Emit("v1");
            sut.Emit("v2");

            behavingGotCatch.Should().BeFalse();
            misbehavingGotCatch.Should().BeTrue();

            behaving.Interceptions.OnAnyFailed.Should().BeEmpty();
            misbehaving.Interceptions.OnAnyFailed.Should().BeEmpty();
            behaving.Interceptions.OnAllSucceeded.Should().HaveCount(2);
            misbehaving.Interceptions.OnAllSucceeded.Should().HaveCount(2);
        }

        [Fact]
        public void CatchAny_catches_for_failed_consumer_only_and_does_not_notify_any_consumer_of_failure()
        {
            var behaving = InterceptingConsumer.Behaving();
            var misbehaving = InterceptingConsumer.Misbehaving();
            var behavingGotCatch = false;
            var misbehavingGotCatch = false;
            var sut = NewSut();
            sut
                .CatchAny(_ => behavingGotCatch = true)
                .Subscribe(behaving);
            sut
                .CatchAny(_ => misbehavingGotCatch = true)
                .Subscribe(misbehaving);

            sut.Emit("v1");
            sut.Emit("v2");

            behavingGotCatch.Should().BeFalse();
            misbehavingGotCatch.Should().BeTrue();

            behaving.Interceptions.OnAnyFailed.Should().BeEmpty();
            misbehaving.Interceptions.OnAnyFailed.Should().BeEmpty();
            behaving.Interceptions.OnAllSucceeded.Should().HaveCount(2);
            misbehaving.Interceptions.OnAllSucceeded.Should().HaveCount(2);
        }

        [Fact]
        public void Supports_content_type_filtering()
        {
            var consumer = InterceptingConsumer<string>.Behaving();
            var sut = NewSut();
            sut
                .When<string>()
                .Subscribe(consumer);

            sut.Emit("hello world");
            sut.Emit(new Dummy("test"));

            consumer.Interceptions.Ensure().OnNewOnlyHasContents("hello world");
        }

        [Fact]
        public void Supports_content_mapping()
        {
            var interceptions = new List<string>();
            var sut = NewSut();
            sut
                .Select(ev => $"Mapped:{ev.Content}")
                .Subscribe(interceptions.Add);

            sut.Emit("1");
            sut.Emit("2");

            interceptions.Should().BeEquivalentTo("Mapped:1", "Mapped:2");
        }

        [Fact]
        public void Supports_event_filtering()
        {
            var consumer = InterceptingConsumer<string>.Behaving();
            var sut = NewSut();
            sut
                .When<string>()
                .Subscribe(consumer);

            sut.Emit("1");
            sut.Emit(new Dummy("test"));

            consumer.Interceptions.Ensure().OnNewOnlyHasContents("1");
        }

        [Fact]
        public void Supports_content_filtering()
        {
            var consumer = InterceptingConsumer<string>.Behaving();
            var sut = NewSut();
            sut
                .When<string>()
                .Where(ev => ev.Content == "1")
                .Subscribe(consumer);

            sut.Emit("1");
            sut.Emit("2");

            consumer.Interceptions.Ensure().OnNewOnlyHasContents("1");
        }
    }
}
