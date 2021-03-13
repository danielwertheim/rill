# Rill
[![NuGet](https://img.shields.io/nuget/v/Rill.svg?cacheSeconds=3600)](https://www.nuget.org/packages/Rill)
[![License MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://choosealicense.com/licenses/mit/)
[![Build Status](https://dev.azure.com/daniel-wertheim/os/_apis/build/status/danielwertheim.rill-CI?branchName=main)](https://dev.azure.com/daniel-wertheim/os/_build/latest?definitionId=9&branchName=main)

`/rɪl/` - noun: *rill*; plural noun: *rills*; **a small stream.** which in this repo translates to: A `Rill` is a "small" stream of events.

## `IAsyncRill` vs `IRill`
There are two main tracks of an `Rill`: Asynchronous Rill (`IAsyncRill`) and Synchronous Rill (`IRill`).

You create them via `RillFactory.Asynchronous()` and `RillFactory.Synchronous()`. The former uses consumers that has asynchronous members, while the later uses consumers with synchronous members.

### `IRillCommit` & `IRillStore`
A commit defines the result of a persisted sequential batch of events against an `IRillStore`. A Rill accepts **sequential** emits of events so the sequencing is used as an optimistic concurrency check.

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
rill.Subscribe(...)
```

## Unsubscribe
The `Subscribe` member returns an `IDisposable`. If you invoke `Dispose` the consumer will be disposed and removed from the consumable's list of subscribed consumers and no further interaction will take place.

## Emit
When emitting an event via `IAsyncRill.EmitAsync(...)` or `IRill.Emit(...)`, the event will reach each consumer as an `Event<T>`.

The Consumable Rill will invoke the following members on each subscribed consumer:

`IAsyncRill`:
- `OnNewAsync(Event<T>)`: **Required** Invoked each time a new event gets emitted.
- `OnAllSucceededAsync(EventId)`: **Optional** Invoked when the event has been successfully dispatched (no event has occurred) to ALL consumers.
- `OnAnyFailedAsync(EventId)`: **Optional** Invoked if the event causes ANY observer to throw an Exception.

`IRill`:
- `OnNew(Event<T>)`: **Required** Invoked each time a new event gets emitted.
- `OnAllSucceeded(EventId)`: **Optional** Invoked when the event has been successfully dispatched (no event has occurred) to ALL consumers.
- `OnAnyFailed(EventId)`: **Optional** Invoked if the event causes ANY observer to throw an Exception.

## `Event<T>` is just an envelope
`Rill` wraps your application event in an envelope which is decorated with data useful to represent the event occurrence in `Rill`. The envelope adds e.g: `Id`, `Sequence` and `Timestamp`.

## Delegating Consumers
Instead of working with an actual implementation of `IAsyncRillConsumer<T>` or `IRillConsumer<T>` you can use a "delegating consumer". You can use it by subscribing using `Action` and `Func` members via `Subscribe(...)` which requires import of the `Rill.Extensions` namespace.

```csharp
rill.Subscribe(ev => {...});
```

There are some optional members:

```csharp
rill.Consume.Subscribe(
    onNew: ev => {...},
    onSuceeded: id => {...},   //optional
    onFailed: id => {...}); //optional
```

You can also create them via the `ConsumerFactory`, e.g:

```csharp
var consumer = ConsumerFactory.AsynchronousConsumer(
    onNew: ev => {...},
    onSuceeded: id => {...},   //optional
    onFailed: id => {...}); //optional

rill.Consume.Subscribe(consumer);
```

## Consumable Operators
There are some extensions (`Rill.Extensions`) that you can use to customize your stream. E.g. `Map` and `Filter` events.

```csharp
rill.Consume
  .When<OrderPlaced>()
  .Where(ev => ev.Sequenece > EventSequence.Create(10))
  .Where(ev => ev.Content.OrderNumber == "42")
  .Select(ev => new SomeOtherThing(...))
  .Subscribe(someOtherThing => {...});
```

## Sample
For more extensive samples, have a look [here in the repo](src/samples).

```csharp
using System;
using System.Threading.Tasks;
using ConsoleSample.Events;
using ConsoleSample.Views;
using Rill;
using Rill.Stores.InMemory;

namespace ConsoleSample
{
    public static class Program
    {
        public static async Task Main()
        {
            var orderStore = new InMemoryRillStore();

            var rillReference = RillReference.New("order");

            await PlaceAndApproveOrderAsync(orderStore, rillReference);

            await ShipOrderAsync(orderStore, rillReference);

            Console.WriteLine("**************************");
            Console.WriteLine("All commits:");
            Console.WriteLine("**************************");
            await foreach (var commit in orderStore.ReadCommitsAsync(rillReference))
                Console.WriteLine(commit);
            Console.WriteLine("**************************");
        }

        private static async Task PlaceAndApproveOrderAsync(IRillStore orderStore, RillReference reference)
        {
            using var rill = RillFactory.Synchronous(reference);

            var view = new OrderView(rill);

            using var transaction = RillTransaction.Begin(rill);

            rill.Emit(new OrderPlaced(
                "order#1",
                "customer#1",
                100M,
                DateTime.UtcNow));

            view.Dump("After OrderPlaced");

            rill.Emit(new OrderApproved(view.OrderNumber!, DateTime.UtcNow));

            view.Dump("After OrderApproved");

            var commit = await transaction.CommitAsync(orderStore);
            Console.WriteLine($"Committed {commit}");
        }

        private static async Task ShipOrderAsync(IRillStore orderStore, RillReference reference)
        {
            using var rill = RillFactory.Synchronous(reference);

            var view = new OrderView(rill);

            foreach (var c in orderStore.ReadCommits(reference))
                rill.Emit(c);

            using var transaction = RillTransaction.Begin(rill);

            rill.Emit(new OrderShipped(view.OrderNumber!, DateTime.UtcNow));

            view.Dump("After OrderShipped");

            var commit = await transaction.CommitAsync(orderStore);
            Console.WriteLine($"Committed {commit}");
        }
    }
}

```

The `OrderView` in the sample is just a simple aggregation representing an order:

```csharp
using System;
using ConsoleSample.Events;
using Rill;
using Rill.Extensions;

namespace ConsoleSample.Views
{
    public class OrderView
    {
        public RillReference Reference { get; }

        public string? OrderNumber { get; private set; }
        public string? CustomerRef { get; private set; }
        public decimal? Amount { get; private set; }
        public DateTime? PlacedAt { get; private set; }
        public DateTime? ApprovedAt { get; private set; }
        public DateTime? ShippedAt { get; private set; }

        public OrderView(IRill rill)
        {
            Reference = rill.Reference;

            rill.Where<OrderPlaced>(ev => ev.Content.Amount > 1).Select(ev => ev.Content.Amount).Subscribe(amount => { });
            rill.When<OrderPlaced>().Where(ev => ev.Content.Amount > 1).Select(ev => ev.Content.Amount).Subscribe(amount => { });

            rill.When<OrderPlaced>().Subscribe(ev =>
            {
                OrderNumber = ev.Content.OrderNumber;
                PlacedAt = ev.Content.PlacedAt;
                CustomerRef = ev.Content.CustomerRef;
                Amount = ev.Content.Amount;
            });
            rill.When<OrderApproved>().Subscribe(ev => ApprovedAt = ev.Content.ApprovedAt);
            rill.When<OrderShipped>().Subscribe(ev => ShippedAt = ev.Content.ShippedAt);
        }
    }
}
```

## Stores
- Rill.Stores.InMemory
- Rill.Stores.EfCore
- Rill.Stores.MongoDB (*Coming soon!*)

### Rill.Stores.InMemory
Simple in process storage for fiddling and testing.

```csharp
var rillStore = new InMemoryRillStore();
```

### Rill.Stores.EfCore
Uses/depends on the `Microsoft.EntityFrameworkCore.Relational` package, but the intention is to **target SQL-Server** but SQLite *could* work as well.

Install the package `Rill.Stores.EfCore` and `Microsoft.EntityFrameworkCore.SqlServer`, then harvest:

```csharp
var dbContextOptions = new DbContextOptionsBuilder<RillDbContext>()
    .UseSqlServer(@"Server=.,1401;Database=Rill;User=foo;Password=bar;MultipleActiveResultSets=True;TrustServerCertificate=true")
    .Options;

var rillStore = new EfCoreRillStore(dbContextOptions);
```

## Development
Run

```bash
$ . init-local-env.sh
```

and edit the `.env` file and `src/rill-appsettings.local.json` file. After that you can use `docker-compose up` to spin up resources like e.g. SQL-Server.