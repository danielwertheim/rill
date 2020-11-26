using System;
using Rill;
using Rill.Extensions;

namespace ConsoleSample
{
    public class Program
    {
        static void Main(string[] args)
        {
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

    public interface IAppEvent
    {
    }

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

        public CustomerCreated(
            string customerNumber)
        {
            CustomerNumber = customerNumber;
        }
    }

    public class OrderInitiated : IOrderEvent
    {
        public string OrderNumber { get; }

        public OrderInitiated(
            string orderNumber)
        {
            OrderNumber = orderNumber;
        }
    }

    public class OrderConfirmed : IOrderEvent
    {
        public string OrderNumber { get; }

        public OrderConfirmed(
            string orderNumber)
        {
            OrderNumber = orderNumber;
        }
    }
}
