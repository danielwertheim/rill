using System;
using System.Threading.Tasks;
using ConsoleSample.Events;
using ConsoleSample.Views;
using Rill;

namespace ConsoleSample
{
    public class Program
    {
        private static string GenerateOrderNumber()
            => Guid.NewGuid().ToString("N");

        static async Task Main(string[] args)
        {
            var orderStore = new FakeStore<IOrderEvent>();

            var rillReference = RillReference.New("order");

            using var rill = RillFactory.Synchronous<IOrderEvent>(rillReference);

            var view = new OrderView(rill);

            using var transaction = RillTransaction.Begin(rill);

            rill.Emit(new OrderPlaced(
                GenerateOrderNumber(),
                "customer#1",
                100M,
                DateTime.UtcNow));

            view.Dump("After OrderPlaced");

            rill.Emit(new OrderApproved(view.OrderNumber!, DateTime.UtcNow));

            view.Dump("After OrderApproved");

            var commit = await transaction.CommitAsync(orderStore);
            Console.WriteLine($"Committed {commit.Id}@{commit.Revision}");

            rill.Emit(new OrderShipped(view.OrderNumber!, DateTime.UtcNow));

            view.Dump("After OrderShipped");

            commit = await transaction.CommitAsync(orderStore);
            Console.WriteLine($"Committed {commit.Id}@{commit.Revision}");
        }

    }
}
