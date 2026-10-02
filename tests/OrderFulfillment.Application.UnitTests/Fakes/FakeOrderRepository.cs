using OrderFulfillment.Application.Abstractions;
using OrderFulfillment.Domain.Orders;

namespace OrderFulfillment.Application.UnitTests.Fakes;

internal sealed class FakeOrderRepository : IOrderRepository
{
    public List<Order> Orders { get; } = [];

    public Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Orders.Find(order => order.Id == id));

    public Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        Orders.Add(order);

        return Task.CompletedTask;
    }
}