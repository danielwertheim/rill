# Rill
[![NuGet](https://img.shields.io/nuget/v/Rill.svg?cacheSeconds=3600)](https://www.nuget.org/packages/Rill)
[![License MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://choosealicense.com/licenses/mit/)
[![Build Status](https://dev.azure.com/daniel-wertheim/os/_apis/build/status/danielwertheim.rill-CI?branchName=main)](https://dev.azure.com/daniel-wertheim/os/_build/latest?definitionId=9&branchName=main)

`/rɪl/` - noun: *rill*; plural noun: *rills*; **a small stream.** which in this repo translates to: A `Rill` is a "small" stream of events.

`Rill` is inspired by observables but uses the concept of `Consumable` and `Consumer` and adds more members to the `Consumer`, allowing you to get a more fine grained interaction between them.

## `IAsyncRill<T>` vs `IRill<T>`
There are two main tracks of an `Rill`: Asynchronous Rill (`IAsyncRill<T>`) and Synchronous Rill (`IRill<T>`).

You create them via `RillFactory.Asynchronous<T>()` and `RillFactory.Synchronous<T>()`. The former uses consumers that has asynchronous members, while the later uses consumers with synchronous members.

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
```csharp
using System;
using Rill;
using Rill.Extensions;

namespace ConsoleSample
{
    public class Program
    {
        static void Main(string[] args)
        {
            //A Rill reference acts as an unique-identifier for the Rill
            //by exposing a Name and an Id.
            var rillReference = RillReference.New("app-events");

            using var rill = RillFactory.Synchronous<IAppEvent>();

            var orderEvents1 = rill
                .ConsumeAny
                .OfEventType<IOrderEvent>();

            var orderEvents2 = rill
                .Consume
                .OfEventType<IAppEvent, IOrderEvent>();

            var customerEvents = rill
                .Consume
                .OfEventType<IAppEvent, ICustomerEvent>();

            orderEvents1
                .Where(ev => ev.Sequence % 2 != 0)
                .Subscribe(ev
                    => Console.WriteLine($"Odd seq order handler: Order: {ev.Content.OrderNumber}"));

            orderEvents2
                .Where(ev => ev.Sequence % 2 == 0)
                .Subscribe(ev
                    => Console.WriteLine($"Even seq Order handler: Order: {ev.Content.OrderNumber}"));

            customerEvents
                .Subscribe(ev
                    => Console.WriteLine($"Customer handler: Customer: {ev.Content.CustomerNumber}"));

            rill.Emit(new CustomerCreated("Customer#1"));

            for (var i = 1; i <= 5; i++)
            {
                rill.Emit(new OrderInitiated($"Order#{i}"));
                rill.Emit(new OrderConfirmed($"Order#{i}"));
            }

            rill.Emit(new CustomerCreated("Customer#2"));
        }
    }

    public interface IAppEvent { }

    public interface ICustomerEvent : IAppEvent
    {
        public string CustomerNumber { get; }
    }

    public interface IOrderEvent : IAppEvent
    {
        string OrderNumber { get; }
    }

    public class CustomerCreated : ICustomerEvent
    {
        public string CustomerNumber { get; }

        public CustomerCreated(string customerNumber)
        {
            CustomerNumber = customerNumber;
        }
    }

    public class OrderInitiated : IOrderEvent
    {
        public string OrderNumber { get; }

        public OrderInitiated(string orderNumber)
        {
            OrderNumber = orderNumber;
        }
    }

    public class OrderConfirmed : IOrderEvent
    {
        public string OrderNumber { get; }

        public OrderConfirmed(string orderNumber)
        {
            OrderNumber = orderNumber;
        }
    }
}

```
