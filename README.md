# Rill
[![NuGet](https://img.shields.io/nuget/v/Rill.svg?cacheSeconds=3600)](https://www.nuget.org/packages/Rill)
[![License MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://choosealicense.com/licenses/mit/)
[![Build Status](https://dev.azure.com/daniel-wertheim/os/_apis/build/status/danielwertheim.rill-CI?branchName=main)](https://dev.azure.com/daniel-wertheim/os/_build/latest?definitionId=9&branchName=main)

`/rɪl/` - noun: *rill*; plural noun: *rills*; **a small stream.** which in this repo translates to: A `Rill` is a "small" stream of events.

`Rill` is inspired by observables but uses the concept of `Consumable` and `Consumer` and adds more members to the `Consumer`, allowing you to get a more fine grained interaction between them.

## `IAsyncRill<T>` vs `IRill<T>`
There are two main tracks of an `Rill`:

- AsynchronousRill: extends `IAsyncRill<T>`
- SynchronousRill: extends `IRill<T>`

You create them via `RillFactory.Asynchronous<T>()` and `RillFactory.Synchronous<T>()`. The former uses consumers that has asynchronous members, while the later uses consumers with synchronous members.

## `Subscribe(IRillConsumer)` & `Subscribe(IAsyncRillConsumer)`
In order to react on events in a consumable Rill, you have to subscribe one or more consumers. This is done via `consumable.Subscribe(...):IDisposable`.

## `Subscription.Dispose()` unsubscribes
The `Subscribe` member returns an `IDisposable`. If you invoke `Dispose` the consumer will be disposed and removed from the consumable's list of subscribed consumers and no further interaction will take place.

## `Emit(T content):Event<T>` & `EmitAsync(T content):ValueTask<Event<T>>`
When emitting a value, the value will reach each consumer as an `Event<T>`. The Consumable Rill will invoke the following members on each subscribed consumer:

### `OnNew(Event<T>):void` & `OnNewAsync(Event<T>):ValueTask`
**Required**. Invoked on the consumer (as long as it is subscribed) each time a new event gets emitted.

### `OnSucceeded(EventId):void` & `OnSucceededAsync(EventId):ValueTask`
**Optional**. Invoked when the event has been successfully dispatched (no event has occurred) to ALL consumers.

### `OnFailed(EventId):void` & `OnFailedAsync(EventId):ValueTask`
**Optional**. Invoked if the event causes ANY observer to throw an Exception.

### `OnCompleted():void` & `OnCompletedAsync()`
**Optional**. Invoked when the Rill is marked as completed.

## `Event<T>` is just an envelope
`Rill` does NOT enforce any constraints on your events. This is entirely up to the application/domain that uses `Rill`. Instead, all events are wrapped and decorated with data useful to represent the event occurrence in `Rill`. The envelope adds e.g: `EventId` and `EventSequence`.

## Delegating Consumers
Instead of working with an actual implementation of `IAsyncRillConsumer<T>` or `IRillConsumer<T>` you can use a "delegating consumer". You can use it by subscribing using `Action` and `Func` members via `Subscribe(...)` requires import of the `Rill.Extensions` namespace.

```csharp
rill.Subscribe(ev => {...});
```

There are some optional members:

```csharp
rill.Subscribe(
    onNew: ev => {...},
    onSuceeded: id => {...},   //optional
    onFailed: id => {...},     //optional
    onCompleted: () => {...}); //optional
```

You can also create them via `ConsumerFactory`, e.g:

```csharp
ConsumerFactory.AsynchronousConsumer(
    onNew: ev => {...},
    onSuceeded: id => {...},   //optional
    onFailed: id => {...},     //optional
    onCompleted: () => {...}); //optional
```

## Consumable Operators
There are some extensions (`Rill.Extensions`) that you can use to customize your stream. E.g. `Map` and `Filter` events.

```csharp
rill
  .Where(ev => ev.Sequenece > EventSequence.Create(10))
  .OfType<MyEvent>()
  .Where(evContent => evContent.SomeMember == 42)
  .Select(ev|evContent => new SomeOtherThing(...))
  .Subscribe(ev => {...})
```
