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

            rill
                .Where<OrderPlaced>(ev => ev.Content.Amount > 1)
                .Select(ev => ev.Content.Amount)
                .Subscribe(amount => { });
            rill
                .When<OrderPlaced>()
                .Where(ev => ev.Content.Amount > 1)
                .Select(ev => ev.Content.Amount)
                .Subscribe(amount => { });

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

    internal static class OrderViewExtensions
    {
        private static readonly object Sync = new();

        internal static void Dump(this OrderView view, string title)
        {
            lock (Sync)
            {
                Console.WriteLine("**************************");
                Console.WriteLine(title);
                Console.WriteLine("**************************");
                Console.WriteLine($"Ref: {view.Reference}");
                Console.WriteLine($"OrderNumber: {view.OrderNumber}");
                Console.WriteLine($"CustomerRef: {view.CustomerRef}");
                Console.WriteLine($"Amount: {view.Amount}");
                Console.WriteLine($"PlacedAt: {view.PlacedAt}");
                Console.WriteLine($"ApprovedAt: {view.ApprovedAt}");
                Console.WriteLine($"ShippedAt: {view.ShippedAt}");
                Console.WriteLine("**************************");
                Console.WriteLine();
            }
        }
    }
}
