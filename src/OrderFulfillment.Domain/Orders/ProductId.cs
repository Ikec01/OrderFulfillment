using System;
using System.Collections.Generic;
using System.Text;

namespace OrderFulfillment.Domain.Orders
{
    public readonly record struct ProductId(Guid Value)
    {
        public static ProductId New() => new(Guid.NewGuid());

        public override string ToString() => Value.ToString();
    }
}
