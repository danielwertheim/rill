using System;

namespace ConsoleSample.Events
{
    public class OrderShipped : IOrderEvent
    {
        public string OrderNumber { get; }
        public DateTime ShippedAt { get; }

        public OrderShipped(string orderNumber, DateTime shippedAt)
        {
            OrderNumber = orderNumber;
            ShippedAt = shippedAt;
        }
    }
}
