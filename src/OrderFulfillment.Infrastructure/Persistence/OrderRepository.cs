using Microsoft.EntityFrameworkCore;
using OrderFulfillment.Application.Abstractions;
using OrderFulfillment.Domain.Orders;

namespace OrderFulfillment.Infrastructure.Persistence;

public sealed class OrderRepository(OrderFulfillmentDbContext dbContext) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken = default) =>
        await dbContext.Orders
            .Include(order => order.Items)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(order);

        await dbContext.Orders.AddAsync(order, cancellationToken);
    }
}