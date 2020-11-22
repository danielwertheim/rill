using System;
using Rill;
using Rill.Extensions;

namespace ConsoleSample
{
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

    public class Program
    {
        static void Main(string[] args)
        {
            using var rill = RillFactory.Synchronous<IAppEvent>();

            rill
                .OfType<IAppEvent, IOrderEvent>()
                .Subscribe(ev =>
                {
                    Console.WriteLine(ev.Content.GetType().Name);
                    Console.WriteLine($"Order number: {ev.Content.OrderNumber}");
                });

            rill.Emit(new CustomerCreated("Customer#1"));
            rill.Emit(new OrderInitiated("Order#1"));
            rill.Emit(new OrderConfirmed("Order#1"));
        }
    }
}
