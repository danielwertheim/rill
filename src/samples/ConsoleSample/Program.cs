using System;
using System.Threading.Tasks;
using ConsoleSample.Events;
using ConsoleSample.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Rill;
using Rill.Stores.InMemory;
using Rill.Stores.EfCore;

namespace ConsoleSample
{
    public static class Program
    {
        private static async ValueTask<IRillStore> CreateEfRillStoreAsync(IConfiguration configuration)
        {
            var dbContextOptions = new DbContextOptionsBuilder<RillDbContext>()
                .UseSqlServer($@"Server=.,1401;Database=Rill;User=sa;Password={configuration["SqlServer:Pass"]};MultipleActiveResultSets=True;TrustServerCertificate=true")
                .Options;

            await using var dbContext = new RillDbContext(dbContextOptions);
            await dbContext.Database.EnsureCreatedAsync();
            await dbContext.Database.CloseConnectionAsync();

            return new EfCoreRillStore(dbContextOptions);
        }

        private static IConfiguration CreateConfiguration() =>
            new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("rill-appsettings.json", true, false)
                .AddJsonFile("rill-appsettings.local.json", true, false)
                .AddEnvironmentVariables("Rill_")
                .Build();

        public static async Task Main()
        {
            var config = CreateConfiguration();

            var orderStore = await CreateEfRillStoreAsync(config);

            var rillReference = RillReference.New("order");

            await PlaceAndApproveOrderAsync(orderStore, rillReference);

            await ShipOrderAsync(orderStore, rillReference);

            Console.WriteLine("**************************");
            Console.WriteLine("All commits:");
            Console.WriteLine("**************************");
            await foreach (var commit in orderStore.ReadCommitsAsync(rillReference))
                Console.WriteLine(commit);
            Console.WriteLine("**************************");

            await orderStore.DeleteAsync(rillReference);
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
