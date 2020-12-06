using System;
using ConsoleSample.Events;
using Rill;
using Rill.Extensions;

namespace ConsoleSample.Views
{
    internal static class OrderRillExtensions
    {
        internal static IRillConsumable<T> OfOrderEvent<T>(this IRill<IOrderEvent> rill) where T : IOrderEvent
            => rill.Consume.OfEventType<IOrderEvent, T>();
    }

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

            //ALTERNATIVE (A)

            // rill.Consume.OfEventType<IOrderEvent, OrderPlaced>().Subscribe(ev =>
            // {
            //     OrderNumber = ev.Content.OrderNumber;
            //     PlacedAt = ev.Content.PlacedAt;
            //     CustomerRef = ev.Content.CustomerRef;
            //     Amount = ev.Content.Amount;
            // });
            // rill.Consume.OfEventType<IOrderEvent, OrderApproved>().Subscribe(ev => ApprovedAt = ev.Content.ApprovedAt);
            // rill.Consume.OfEventType<IOrderEvent, OrderShipped>().Subscribe(ev => ShippedAt = ev.Content.ShippedAt);


            //ALTERNATIVE (B)

            // rill.ConsumeAny.OfEventType<OrderPlaced>().Subscribe(ev =>
            // {
            //     OrderNumber = ev.Content.OrderNumber;
            //     PlacedAt = ev.Content.PlacedAt;
            //     CustomerRef = ev.Content.CustomerRef;
            //     Amount = ev.Content.Amount;
            // });
            // rill.ConsumeAny.OfEventType<OrderApproved>().Subscribe(ev => ApprovedAt = ev.Content.ApprovedAt);
            // rill.ConsumeAny.OfEventType<OrderShipped>().Subscribe(ev => ShippedAt = ev.Content.ShippedAt);


            //SIMPLIFIED VIA CUSTOM EXTENSION METHOD

            rill.OfOrderEvent<OrderPlaced>().Subscribe(ev =>
            {
                OrderNumber = ev.Content.OrderNumber;
                PlacedAt = ev.Content.PlacedAt;
                CustomerRef = ev.Content.CustomerRef;
                Amount = ev.Content.Amount;
            });
            rill.OfOrderEvent<OrderApproved>().Subscribe(ev => ApprovedAt = ev.Content.ApprovedAt);
            rill.OfOrderEvent<OrderShipped>().Subscribe(ev => ShippedAt = ev.Content.ShippedAt);
        }
    }

    internal static class OrderViewExtensions
    {
        private static readonly object Sync = new object();

        internal static void Dump(this OrderView view, string title)
        {
            lock (Sync)
            {
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
            }
        }
    }
}
