using System;
using System.Collections.Generic;
using System.Text;
using OrderFulfillment.Domain.Common;

namespace OrderFulfillment.Domain.Orders.Events
{
    public sealed record OrderDeliveredDomainEvent(OrderId OrderId) : DomainEvent;
    
}
