using System;
using System.Collections.Generic;
using System.Text;
using OrderFulfillment.Application.Common;

namespace OrderFulfillment.Application.Orders
{
    public static class OrderErrors
    {
        public static ApplicationError NotFound(Guid orderId) =>
            new("Orders.NotFound", $"Porudzbina '{orderId}' nije pronadjena.", ErrorType.NotFound);
    }
}
