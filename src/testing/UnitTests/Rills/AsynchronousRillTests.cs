using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Rill;
using Rill.Extensions;
using Xunit;

namespace UnitTests.Rills
{
    public class AsynchronousRillTests
    {
        private static IAsyncRill NewSut()
            => RillFactory.Asynchronous(RillReference.New("async-rill-tests"));

        [Fact]
        public async Task Returns_event_after_emitting()
        {
            var content = Fake.Strings.Random();
            var id = EventId.New();
            var seq = Sequence.First;
            var sut = NewSut();

            var ev = await sut.EmitAsync(content, id);

            ev.Id.Should().Be(id);
            ev.Content.Should().Be(content);
            ev.Sequence.Should().Be(seq);
            sut.Sequence.Should().Be(seq);
        }

        [Fact]
        public async Task Requires_event_sequence_to_be_in_order()
        {
            var sut = NewSut();
            var ev1 = await sut.EmitAsync("first");

            Func<Task<Event<string>>> historicSequence = async () => await sut.EmitAsync("second", sequence: Sequence.First);
            Func<Task<Event<string>>> futureSequence = async () => await sut.EmitAsync("second", sequence: ev1.Sequence.Add(2));

            (await historicSequence
                .Should()
                .ThrowAsync<EventOutOfOrderException>())
                .Where(ex => ex.Actual == Sequence.First && ex.Expected == ev1.Sequence.Increment());

            (await futureSequence
                .Should()
                .ThrowAsync<EventOutOfOrderException>())
                .Where(ex => ex.Actual == ev1.Sequence.Add(2) && ex.Expected == ev1.Sequence.Increment());
        }

        [Fact]
        public async Task Unsubscribed_consumer_is_not_invoked_at_all()
        {
            var unsubscribing = AsyncInterceptingConsumer.Behaving();
            var unsubscribingDelegating = new Interceptions();
            var sut = NewSut();
            sut.Subscribe(_ => throw new Exception("FAIL"));
            var sub1 = sut.Subscribe(unsubscribing);
            var sub2 = sut.Subscribe(
                unsubscribingDelegating.InOnNewAsync,
                unsubscribingDelegating.InOnAllSucceededAsync,
                unsubscribingDelegating.InOnAnyFailedAsync);

            sub1.Dispose();
            sub2.Dispose();

            await sut.EmitAsync(Fake.Strings.Random());
            unsubscribing.Ensure().ToBeUnTouched();
            unsubscribingDelegating.Ensure().ToBeUnTouched();
        }

        [Fact]
        public async Task Emits_events_to_all_active_consumers_even_failed_ones()
        {
            var behaving = AsyncInterceptingConsumer.Behaving();
            var behavingDelegating = new Interceptions();
            var misbehaving = AsyncInterceptingConsumer.Misbehaving(ev => ev.Sequence > Sequence.First);
            var misbehavingDelegating = new Interceptions();
            var sut = NewSut();
            sut.Subscribe(behaving);
            sut.Subscribe(
                behavingDelegating.InOnNewAsync,
                behavingDelegating.InOnAllSucceededAsync,
                behavingDelegating.InOnAnyFailedAsync);
            sut.Subscribe(misbehaving);
            sut.Subscribe(
                ev =>
                {
                    misbehavingDelegating.InOnNew(ev);
                    if (ev.Sequence > Sequence.First)
                        throw new Exception(ev.ToString());

                    return ValueTask.CompletedTask;
                },
                misbehavingDelegating.InOnAllSucceededAsync,
                misbehavingDelegating.InOnAnyFailedAsync);

            var ev1 = await sut.EmitAsync(Fake.Strings.Random());
            var ev2 = await sut.EmitAsync(Fake.Strings.Random());

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
        public async Task Catch_catches_for_failed_consumer_only_and_does_not_notify_any_consumer_of_failure()
        {
            var behaving = AsyncInterceptingConsumer.Behaving();
            var misbehaving = AsyncInterceptingConsumer.Misbehaving();
            var behavingGotCatch = false;
            var misbehavingGotCatch = false;

            var sut = NewSut();
            sut
                .Catch<Exception>(_ => behavingGotCatch = true)
                .Subscribe(behaving);
            sut
                .Catch<Exception>(_ => misbehavingGotCatch = true)
                .Subscribe(misbehaving);

            await sut.EmitAsync("v1");
            await sut.EmitAsync("v2");

            behavingGotCatch.Should().BeFalse();
            misbehavingGotCatch.Should().BeTrue();

            behaving.Interceptions.OnAnyFailed.Should().BeEmpty();
            misbehaving.Interceptions.OnAnyFailed.Should().BeEmpty();
            behaving.Interceptions.OnAllSucceeded.Should().HaveCount(2);
            misbehaving.Interceptions.OnAllSucceeded.Should().HaveCount(2);
        }

        [Fact]
        public async Task CatchAny_catches_for_failed_consumer_only_and_does_not_notify_any_consumer_of_failure()
        {
            var behaving = AsyncInterceptingConsumer.Behaving();
            var misbehaving = AsyncInterceptingConsumer.Misbehaving();
            var behavingGotCatch = false;
            var misbehavingGotCatch = false;
            var sut = NewSut();
            sut
                .CatchAny(_ => behavingGotCatch = true)
                .Subscribe(behaving);
            sut
                .CatchAny(_ => misbehavingGotCatch = true)
                .Subscribe(misbehaving);

            await sut.EmitAsync("v1");
            await sut.EmitAsync("v2");

            behavingGotCatch.Should().BeFalse();
            misbehavingGotCatch.Should().BeTrue();

            behaving.Interceptions.OnAnyFailed.Should().BeEmpty();
            misbehaving.Interceptions.OnAnyFailed.Should().BeEmpty();
            behaving.Interceptions.OnAllSucceeded.Should().HaveCount(2);
            misbehaving.Interceptions.OnAllSucceeded.Should().HaveCount(2);
        }

        [Fact]
        public async Task Supports_content_type_filtering()
        {
            var consumer = AsyncInterceptingConsumer<string>.Behaving();
            var sut = NewSut();
            sut
                .When<string>()
                .Subscribe(consumer);

            await sut.EmitAsync("hello world");
            await sut.EmitAsync(new Dummy("test"));

            consumer.Interceptions.Ensure().OnNewOnlyHasContents("hello world");
        }

        [Fact]
        public async Task Supports_content_mapping()
        {
            var interceptions = new List<string>();
            var sut = NewSut();
            sut
                .Select(ev => $"Mapped:{ev.Content}")
                .Subscribe(v =>
                {
                    interceptions.Add(v);
                    return ValueTask.CompletedTask;
                });

            await sut.EmitAsync("1");
            await sut.EmitAsync("2");

            interceptions.Should().BeEquivalentTo("Mapped:1", "Mapped:2");
        }

        [Fact]
        public async Task Supports_event_filtering()
        {
            var consumer = AsyncInterceptingConsumer<string>.Behaving();
            var sut = NewSut();
            sut
                .When<string>()
                .Subscribe(consumer);

            await sut.EmitAsync("1");
            await sut.EmitAsync(new Dummy("test"));

            consumer.Interceptions.Ensure().OnNewOnlyHasContents("1");
        }

        [Fact]
        public async Task Supports_content_filtering()
        {
            var consumer = AsyncInterceptingConsumer<string>.Behaving();
            var sut = NewSut();
            sut
                .When<string>()
                .Where(ev => ev.Content == "1")
                .Subscribe(consumer);

            await sut.EmitAsync("1");
            await sut.EmitAsync("2");

            consumer.Interceptions.Ensure().OnNewOnlyHasContents("1");
        }
    }
}
