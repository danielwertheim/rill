using System;

namespace ConsoleSample.Events
{
    public class OrderPlaced : IOrderEvent
    {
        public string OrderNumber { get; }
        public string CustomerRef { get; }
        public decimal Amount { get; }
        public DateTime PlacedAt { get; }

        public OrderPlaced(
            string orderNumber,
            string customerRef,
            decimal amount,
            DateTime placedAt)
        {
            OrderNumber = orderNumber;
            CustomerRef = customerRef;
            Amount = amount;
            PlacedAt = placedAt;
        }
    }
}
