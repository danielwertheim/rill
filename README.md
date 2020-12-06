# Rill
[![NuGet](https://img.shields.io/nuget/v/Rill.svg?cacheSeconds=3600)](https://www.nuget.org/packages/Rill)
[![License MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://choosealicense.com/licenses/mit/)
[![Build Status](https://dev.azure.com/daniel-wertheim/os/_apis/build/status/danielwertheim.rill-CI?branchName=main)](https://dev.azure.com/daniel-wertheim/os/_build/latest?definitionId=9&branchName=main)

`/rɪl/` - noun: *rill*; plural noun: *rills*; **a small stream.** which in this repo translates to: A `Rill` is a "small" stream of events.

`Rill` is inspired by observables but uses the concept of `Consumable` and `Consumer` and adds more members to the `Consumer`, allowing you to get a more fine grained interaction between them.

## `IAsyncRill<T>` vs `IRill<T>`
There are two main tracks of an `Rill`: Asynchronous Rill (`IAsyncRill<T>`) and Synchronous Rill (`IRill<T>`).

You create them via `RillFactory.Asynchronous<T>()` and `RillFactory.Synchronous<T>()`. The former uses consumers that has asynchronous members, while the later uses consumers with synchronous members.

### `IRillCommit<T>` & `IRillStore<T>`
A commit defines the result of a persisted sequential batch of events against an `IRillStore<T>`. A Rill accepts sequential emits of events so the sequencing is used as an optimistic concurrency check.

```
[Rill:Sequence]
R:0
R:1 ev1
R:2 ev2
Commit:1 { R:1 |->| R:2}

R:2
R:3 ev3
R:4 ev4
Commit:2 { R:3 |->| R:4}
```

## Subscribe
In order to react on events in a consumable `Rill`, you have to subscribe one or more consumers. This is done via:

```csharp
//Exposes the Rill as a stream of T.
rill.Consume.Subscribe(...)
```

```csharp
//ConsumeAny: Exposes the Rill as a stream of anything.
rill.ConsumeAny.Subscribe(...)
```

## Unsubscribe
The `Subscribe` member returns an `IDisposable`. If you invoke `Dispose` the consumer will be disposed and removed from the consumable's list of subscribed consumers and no further interaction will take place.

## Emit
When emitting an event via `IAsyncRill<T>.EmitAsync(...)` or `IRill<T>.Emit(...)`, the event will reach each consumer as an `Event<T>`.

The Consumable Rill will invoke the following members on each subscribed consumer:

`IAsyncRill<T>`:
- `OnNewAsync(Event<T>)`: **Required** Invoked each time a new event gets emitted.
- `OnAllSucceededAsync(EventId)`: **Optional** Invoked when the event has been successfully dispatched (no event has occurred) to ALL consumers.
- `OnAnyFailedAsync(EventId)`: **Optional** Invoked if the event causes ANY observer to throw an Exception.
- `OnCompletedAsync()`: **Optional** Invoked when the Rill is marked as completed.

`IRill<T>`:
- `OnNew(Event<T>)`: **Required** Invoked each time a new event gets emitted.
- `OnAllSucceeded(EventId)`: **Optional** Invoked when the event has been successfully dispatched (no event has occurred) to ALL consumers.
- `OnAnyFailed(EventId)`: **Optional** Invoked if the event causes ANY observer to throw an Exception.
- `OnCompleted()`: **Optional** Invoked when the Rill is marked as completed.

## `Event<T>` is just an envelope
`Rill` does NOT enforce any constraints on your events. This is entirely up to the application/domain that uses `Rill`. Instead, all events are wrapped and decorated with data useful to represent the event occurrence in `Rill`. The envelope adds e.g: `EventId` and `EventSequence`.

## Delegating Consumers
Instead of working with an actual implementation of `IAsyncRillConsumer<T>` or `IRillConsumer<T>` you can use a "delegating consumer". You can use it by subscribing using `Action` and `Func` members via `Subscribe(...)` which requires import of the `Rill.Extensions` namespace.

```csharp
rill.Consume.Subscribe(ev => {...});
```

There are some optional members:

```csharp
rill.Consume.Subscribe(
    onNew: ev => {...},
    onSuceeded: id => {...},   //optional
    onFailed: id => {...},     //optional
    onCompleted: () => {...}); //optional
```

You can also create them via `ConsumerFactory`, e.g:

```csharp
var consumer = ConsumerFactory.AsynchronousConsumer(
    onNew: ev => {...},
    onSuceeded: id => {...},   //optional
    onFailed: id => {...},     //optional
    onCompleted: () => {...}); //optional

rill.Consume.Subscribe(consumer);
```

## Consumable Operators
There are some extensions (`Rill.Extensions`) that you can use to customize your stream. E.g. `Map` and `Filter` events.

```csharp
rill.Consume
  .OfEventType<IAppEvent, IOrderEvent>()
  .Where(ev => ev.Sequenece > EventSequence.Create(10))
  .Where(evContent => evContent.OrderNumber == "42")
  .Select(ev|evContent => new SomeOtherThing(...))
  .Subscribe(ev => {...})
```

```csharp
rill.ConsumeAny
  .OfEventType<IOrderEvent>()
  .Where(ev => ev.Sequenece > EventSequence.Create(10))
  .Where(evContent => evContent.OrderNumber == "42")
  .Select(ev|evContent => new SomeOtherThing(...))
  .Subscribe(ev => {...})
```

## Sample
For more extensive samples, have a look [here in the repo](src/samples).
```csharp
using System;
using System.Threading.Tasks;
using ConsoleSample.Events;
using ConsoleSample.Views;
using Rill;

namespace ConsoleSample
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            //A Rill store is used to persist and read events
            var orderStore = new FakeStore<IOrderEvent>();

            //A Reference identifies a certain Rill.
            //Via its name and id
            var rillReference = RillReference.New("order");

            using var rill = RillFactory.Synchronous<IOrderEvent>(rillReference);

            //Order view is e.g. an application specific implementation.
            //In this case, a simple aggregation of an order view.
            var view = new OrderView(rill);

            //A transaction monitors the events dispatched on a Rill
            //and is used to commit batches of events to a store.
            using var transaction = RillTransaction.Begin(rill);

            rill.Emit(new OrderPlaced(
                GenerateOrderNumber(),
                "customer#1",
                100M,
                DateTime.UtcNow));

            view.Dump("After OrderPlaced");

            rill.Emit(new OrderApproved(view.OrderNumber!, DateTime.UtcNow));

            view.Dump("After OrderApproved");

            rill.Emit(new OrderShipped(view.OrderNumber!, DateTime.UtcNow));

            view.Dump("After OrderShipped");

            commit = await transaction.CommitAsync(orderStore);

            Console.WriteLine($"Committed {commit.Id}@{commit.Revision}");
        }
    }
}
```

The `OrderView` in the sample is just a simple aggregation representing an order:

```csharp
public class OrderView
{
    public RillReference Reference { get; }

    public string? OrderNumber { get; private set; }
    public string? CustomerRef { get; private set; }
    public decimal? Amount { get; private set; }
    public DateTime? PlacedAt { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public DateTime? ShippedAt { get; private set; }

    public OrderView(IRill<IOrderEvent> rill)
    {
        Reference = rill.Reference;
        rill.OfOrderEvent<OrderPlaced>().Subscribe(ev =>
        {
            OrderNumber = ev.Content.OrderNumber;
            PlacedAt = ev.Content.PlacedAt;
            CustomerRef = ev.Content.CustomerRef;
            Amount = ev.Content.Amount;
        });
        rill.OfOrderEvent<OrderApproved>().Subscribe(
            ev => ApprovedAt = ev.Content.ApprovedAt);
        rill.OfOrderEvent<OrderShipped>().Subscribe(
            ev => ShippedAt = ev.Content.ShippedAt);
    }
}
```

To simplify the order event filtering above, you could do a simple extension method:

```csharp
internal static class OrderRillExtensions
{
    internal static IRillConsumable<T> OfOrderEvent<T>(
        this IRill<IOrderEvent> rill) where T : IOrderEvent
        => rill.Consume.OfEventType<IOrderEvent, T>();
}
```
