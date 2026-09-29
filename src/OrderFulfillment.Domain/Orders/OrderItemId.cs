using System;
using System.Collections.Generic;
using System.Text;

namespace OrderFulfillment.Domain.Orders
{
    public readonly record struct OrderItemId(Guid Value)
    {
        public static OrderItemId New() => new(Guid.NewGuid());
        public override string ToString() => Value.ToString();
               
    }
}
