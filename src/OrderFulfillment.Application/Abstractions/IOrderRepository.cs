using OrderFulfillment.Domain.Orders;

namespace OrderFulfillment.Application.Abstractions;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken = default);

    Task AddAsync(Order order, CancellationToken cancellationToken = default);
}