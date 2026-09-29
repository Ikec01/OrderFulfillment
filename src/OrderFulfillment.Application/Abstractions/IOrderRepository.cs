using System;
using System.Collections.Generic;
using System.Text;
using OrderFulfillment.Domain.Orders;

namespace OrderFulfillment.Application.Abstractions
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellation = default);

        Task AddAsync(Order order, CancellationToken cancellation = default);
    }
}
