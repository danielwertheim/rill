using System;

namespace ConsoleSample.Events
{
    public class OrderApproved : IOrderEvent
    {
        public string OrderNumber { get; }
        public DateTime ApprovedAt { get; }

        public OrderApproved(string orderNumber, DateTime approvedAt)
        {
            OrderNumber = orderNumber;
            ApprovedAt = approvedAt;
        }
    }
}
