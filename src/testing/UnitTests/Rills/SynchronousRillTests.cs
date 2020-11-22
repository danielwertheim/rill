using System;
using FluentAssertions;
using Rill;
using Rill.Extensions;
using Xunit;

namespace UnitTests.Rills
{
    public class SynchronousRillTests
    {
        private static IRill<T> NewSut<T>()
            => RillFactory.Synchronous<T>();

        private static string NewStringContent()
            => Guid.NewGuid().ToString("N");

        [Fact]
        public void Returns_event_after_emitting()
        {
            var content = NewStringContent();
            var id = EventId.New();
            var sut = NewSut<string>();

            var ev = sut.Emit(content, id);

            ev.Id.Should().Be(id);
            ev.Content.Should().Be(content);
        }

        [Fact]
        public void Unsubscribed_consumer_is_not_invoked_at_all()
        {
            var unsubscribing = InterceptingConsumer.Behaving();
            var unsubscribingDelegating = new Interceptions();
            var sut = NewSut<string>();
            sut.Subscribe(_ => throw new Exception("FAIL"));
            var sub1 = sut.Subscribe(unsubscribing);
            var sub2 = sut.Subscribe(
                unsubscribingDelegating.InOnNew,
                unsubscribingDelegating.InOnAllSucceeded,
                unsubscribingDelegating.InOnAnyFailed,
                unsubscribingDelegating.InOnCompleted);

            sub1.Dispose();
            sub2.Dispose();

            sut.Emit(NewStringContent());
            unsubscribing.Ensure().ToBeUnTouched();
            unsubscribingDelegating.Ensure().ToBeUnTouched();

            sut.Complete();
            unsubscribing.Ensure().ToBeUnTouched();
            unsubscribingDelegating.Ensure().ToBeUnTouched();
        }

        [Fact]
        public void Completing_completes_all_active_consumers_even_failed_ones()
        {
            var behaving = InterceptingConsumer.Behaving();
            var behavingDelegating = new Interceptions();
            var misbehaving = InterceptingConsumer.Misbehaving();
            var misbehavingDelegating = new Interceptions();
            var sut = NewSut<string>();
            sut.Subscribe(behaving);
            sut.Subscribe(
                behavingDelegating.InOnNew,
                behavingDelegating.InOnAllSucceeded,
                behavingDelegating.InOnAnyFailed,
                behavingDelegating.InOnCompleted);
            sut.Subscribe(misbehaving);
            sut.Subscribe(
                ev =>
                {
                    misbehavingDelegating.InOnNew(ev);
                    throw new Exception(ev.Content);
                },
                misbehavingDelegating.InOnAllSucceeded,
                misbehavingDelegating.InOnAnyFailed,
                misbehavingDelegating.InOnCompleted);

            sut.Emit(NewStringContent());
            sut.Complete();

            behaving.Ensure().OnCompletedWasCalled();
            behavingDelegating.Ensure().OnCompletedWasCalled();
            misbehaving.Ensure().OnCompletedWasCalled();
            misbehavingDelegating.Ensure().OnCompletedWasCalled();
        }

        [Fact]
        public void Emits_events_to_all_active_consumers_even_failed_ones()
        {
            var behaving = InterceptingConsumer.Behaving();
            var behavingDelegating = new Interceptions();
            var misbehaving = InterceptingConsumer.Misbehaving(ev => ev.Sequence > EventSequence.First);
            var misbehavingDelegating = new Interceptions();
            var sut = NewSut<string>();
            sut.Subscribe(behaving);
            sut.Subscribe(
                behavingDelegating.InOnNew,
                behavingDelegating.InOnAllSucceeded,
                behavingDelegating.InOnAnyFailed,
                behavingDelegating.InOnCompleted);
            sut.Subscribe(misbehaving);
            sut.Subscribe(
                ev =>
                {
                    misbehavingDelegating.InOnNew(ev);
                    if (ev.Sequence > EventSequence.First)
                        throw new Exception(ev.Content);
                },
                misbehavingDelegating.InOnAllSucceeded,
                misbehavingDelegating.InOnAnyFailed,
                misbehavingDelegating.InOnCompleted);

            var ev1 = sut.Emit(NewStringContent());
            var ev2 = sut.Emit(NewStringContent());

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
            var sut = NewSut<string>();
            sut
                .Catch<string, Exception>(_ => behavingGotCatch = true)
                .Subscribe(behaving);
            sut
                .Catch<string, Exception>(_ => misbehavingGotCatch = true)
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
            var sut = NewSut<string>();
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
        public void OfType_allows_for_content_type_filtering()
        {
            var consumer = InterceptingConsumer<int>.Behaving();
            var sut = NewSut<object>();
            sut
                .OfType<object, int>()
                .Subscribe(consumer);

            sut.Emit("1");
            sut.Emit(42);

            consumer.Interceptions.Ensure().OnNewOnlyHasContents(42);
        }

        [Fact]
        public void Select_allows_for_content_mapping()
        {
            var consumer = InterceptingConsumer.Behaving();
            var sut = NewSut<string>();
            sut
                .Select(content => $"Mapped:{content}")
                .Subscribe(consumer);

            sut.Emit("1");
            sut.Emit("2");

            consumer.Interceptions.Ensure().OnNewOnlyHasContents("Mapped:1", "Mapped:2");
        }

        [Fact]
        public void Where_allows_for_event_filtering()
        {
            var consumer = InterceptingConsumer.Behaving();
            var sut = NewSut<string>();
            sut
                .Where(ev => ev.Content == "1")
                .Subscribe(consumer);

            sut.Emit("1");
            sut.Emit("2");

            consumer.Interceptions.Ensure().OnNewOnlyHasContents("1");
        }

        [Fact]
        public void Where_allows_for_content_filtering()
        {
            var consumer = InterceptingConsumer.Behaving();
            var sut = NewSut<string>();
            sut
                .Where(content => content == "1")
                .Subscribe(consumer);

            sut.Emit("1");
            sut.Emit("2");

            consumer.Interceptions.Ensure().OnNewOnlyHasContents("1");
        }
    }
}
