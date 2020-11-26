using System;
using System.Threading.Tasks;
using FluentAssertions;
using Rill;
using Rill.Extensions;
using Xunit;

namespace UnitTests.Rills
{
    public class AsynchronousRillTests
    {
        private static IAsyncRill<T> NewSut<T>() => RillFactory.Asynchronous<T>();

        private static string NewStringContent() => Guid.NewGuid().ToString("N");

        [Fact]
        public async Task Returns_event_after_emitting()
        {
            var content = NewStringContent();
            var id = EventId.New();
            var sut = NewSut<string>();

            var ev = await sut.EmitAsync(content, id);

            ev.Id.Should().Be(id);
            ev.Content.Should().Be(content);
        }

        [Fact]
        public async Task Unsubscribed_consumer_is_not_invoked_at_all()
        {
            var unsubscribing = AsyncInterceptingConsumer.Behaving();
            var unsubscribingDelegating = new Interceptions();
            var sut = NewSut<string>();
            sut.Consume.Subscribe(_ => throw new Exception("FAIL"));
            var sub1 = sut.Consume.Subscribe(unsubscribing);
            var sub2 = sut.Consume.Subscribe(
                unsubscribingDelegating.InOnNewAsync,
                unsubscribingDelegating.InOnAllSucceededAsync,
                unsubscribingDelegating.InOnAnyFailedAsync,
                unsubscribingDelegating.InOnCompletedAsync);

            sub1.Dispose();
            sub2.Dispose();

            await sut.EmitAsync(NewStringContent());
            unsubscribing.Ensure().ToBeUnTouched();
            unsubscribingDelegating.Ensure().ToBeUnTouched();

            sut.Complete();
            unsubscribing.Ensure().ToBeUnTouched();
            unsubscribingDelegating.Ensure().ToBeUnTouched();
        }

        [Fact]
        public async Task Completing_completes_all_active_consumers_even_failed_ones()
        {
            var behaving = AsyncInterceptingConsumer.Behaving();
            var behavingDelegating = new Interceptions();
            var misbehaving = AsyncInterceptingConsumer.Misbehaving();
            var misbehavingDelegating = new Interceptions();
            var sut = NewSut<string>();
            sut.Consume.Subscribe(behaving);
            sut.Consume.Subscribe(
                behavingDelegating.InOnNewAsync,
                behavingDelegating.InOnAllSucceededAsync,
                behavingDelegating.InOnAnyFailedAsync,
                behavingDelegating.InOnCompletedAsync);
            sut.Consume.Subscribe(misbehaving);
            sut.Consume.Subscribe(
                ev =>
                {
                    misbehavingDelegating.InOnNew(ev);
                    throw new Exception(ev.Content);
                },
                misbehavingDelegating.InOnAllSucceededAsync,
                misbehavingDelegating.InOnAnyFailedAsync,
                misbehavingDelegating.InOnCompletedAsync);

            await sut.EmitAsync(NewStringContent());
            sut.Complete();

            behaving.Ensure().OnCompletedWasCalled();
            behavingDelegating.Ensure().OnCompletedWasCalled();
            misbehaving.Ensure().OnCompletedWasCalled();
            misbehavingDelegating.Ensure().OnCompletedWasCalled();
        }

        [Fact]
        public async Task Emits_events_to_all_active_consumers_even_failed_ones()
        {
            var behaving = AsyncInterceptingConsumer.Behaving();
            var behavingDelegating = new Interceptions();
            var misbehaving = AsyncInterceptingConsumer.Misbehaving(ev => ev.Sequence > EventSequence.First);
            var misbehavingDelegating = new Interceptions();
            var sut = NewSut<string>();
            sut.Consume.Subscribe(behaving);
            sut.Consume.Subscribe(
                behavingDelegating.InOnNewAsync,
                behavingDelegating.InOnAllSucceededAsync,
                behavingDelegating.InOnAnyFailedAsync,
                behavingDelegating.InOnCompletedAsync);
            sut.Consume.Subscribe(misbehaving);
            sut.Consume.Subscribe(
                ev =>
                {
                    misbehavingDelegating.InOnNew(ev);
                    if (ev.Sequence > EventSequence.First)
                        throw new Exception(ev.Content);

                    return ValueTask.CompletedTask;
                },
                misbehavingDelegating.InOnAllSucceededAsync,
                misbehavingDelegating.InOnAnyFailedAsync,
                misbehavingDelegating.InOnCompletedAsync);

            var ev1 = await sut.EmitAsync(NewStringContent());
            var ev2 = await sut.EmitAsync(NewStringContent());

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

            var sut = NewSut<string>();
            sut
                .Consume
                .Catch<string, Exception>(_ => behavingGotCatch = true)
                .Subscribe(behaving);
            sut
                .Consume
                .Catch<string, Exception>(_ => misbehavingGotCatch = true)
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
            var sut = NewSut<string>();
            sut
                .Consume
                .CatchAny(_ => behavingGotCatch = true)
                .Subscribe(behaving);
            sut
                .Consume
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
        public async Task OfType_allows_for_content_type_filtering()
        {
            var consumer = AsyncInterceptingConsumer<int>.Behaving();
            var sut = NewSut<object>();
            sut
                .Consume
                .OfEventType<object, int>()
                .Subscribe(consumer);

            await sut.EmitAsync("1");
            await sut.EmitAsync(42);

            consumer.Interceptions.Ensure().OnNewOnlyHasContents(42);
        }

        [Fact]
        public async Task Select_allows_for_content_mapping()
        {
            var consumer = AsyncInterceptingConsumer.Behaving();
            var sut = NewSut<string>();
            sut
                .Consume
                .Select(content => $"Mapped:{content}")
                .Subscribe(consumer);

            await sut.EmitAsync("1");
            await sut.EmitAsync("2");

            consumer.Interceptions.Ensure().OnNewOnlyHasContents("Mapped:1", "Mapped:2");
        }

        [Fact]
        public async Task Where_allows_for_event_filtering()
        {
            var consumer = AsyncInterceptingConsumer.Behaving();
            var sut = NewSut<string>();
            sut
                .Consume
                .Where(ev => ev.Content == "1")
                .Subscribe(consumer);

            await sut.EmitAsync("1");
            await sut.EmitAsync("2");

            consumer.Interceptions.Ensure().OnNewOnlyHasContents("1");
        }

        [Fact]
        public async Task Where_allows_for_content_filtering()
        {
            var consumer = AsyncInterceptingConsumer.Behaving();
            var sut = NewSut<string>();
            sut
                .Consume
                .Where(content => content == "1")
                .Subscribe(consumer);

            await sut.EmitAsync("1");
            await sut.EmitAsync("2");

            consumer.Interceptions.Ensure().OnNewOnlyHasContents("1");
        }
    }
}
