using System;
using System.Collections.Generic;
using System.Text;
using OrderFulfillment.Domain.Orders;

namespace OrderFulfillment.Domain.Orders.Events
{
    public sealed record OrderPlacedItem(ProductId ProductId, int Quantity);
    
}
