using System;
using System.Threading.Tasks;
using ConsoleSample.Events;
using ConsoleSample.Views;
using Rill;
using Rill.Extensions;
using Rill.Stores.InMemory;

namespace ConsoleSample
{
    public static class Program
    {
        public static async Task Main()
        {
            var orderStore = new InMemoryRillStore<IOrderEvent>();

            var rillReference = RillReference.New("order");

            await PlaceAndApproveOrderAsync(orderStore, rillReference);
            await ShipOrderAsync(orderStore, rillReference);
        }

        private static async Task PlaceAndApproveOrderAsync(IRillStore<IOrderEvent> orderStore, RillReference reference)
        {
            using var rill = RillFactory.Synchronous<IOrderEvent>(reference);

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
            Console.WriteLine($"Committed {commit.Id}@{commit.SequenceRange}");
        }

        private static async Task ShipOrderAsync(IRillStore<IOrderEvent> orderStore, RillReference reference)
        {
            using var rill = RillFactory.Synchronous<IOrderEvent>(reference);

            var view = new OrderView(rill);

            foreach (var ev in orderStore.ReadEvents(reference))
                rill.Emit(ev);

            using var transaction = RillTransaction.Begin(rill);

            rill.Emit(new OrderShipped(view.OrderNumber!, DateTime.UtcNow));

            view.Dump("After OrderShipped");

            var commit = await transaction.CommitAsync(orderStore);
            Console.WriteLine($"Committed {commit.Id}@{commit.SequenceRange}");
        }
    }
}
