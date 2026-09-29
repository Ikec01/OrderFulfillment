using System;
using System.Collections.Generic;
using System.Text;

namespace OrderFulfillment.Domain.Orders
{
    public readonly record struct CustomerId(Guid Value)
    {
        public static CustomerId New() => new(Guid.NewGuid());

        public override string ToString() => Value.ToString();
    }
}
