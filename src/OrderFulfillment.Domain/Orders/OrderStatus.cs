using System;
using System.Collections.Generic;
using System.Text;

namespace OrderFulfillment.Domain.Orders
{
    public enum OrderStatus
    {
        Draft = 0,
        Placed = 1,
        Paid = 2,
        Shipped = 3,
        Delivered = 4,
        Cancelled = 5,
    }
}
