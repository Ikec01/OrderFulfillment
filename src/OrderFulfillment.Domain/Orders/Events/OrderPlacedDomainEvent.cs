using System;
using System.Collections.Generic;
using System.Text;
using OrderFulfillment.Domain.Common;
using OrderFulfillment.Domain.ValueObjects;

namespace OrderFulfillment.Domain.Orders.Events
{
    public sealed record OrderPlacedDomainEvent(
        OrderId OrderId,
        CustomerId CustomerId,
        Money TotalAmount,
        IReadOnlyCollection<OrderPlacedItem> Items) : DomainEvent;
    
}
