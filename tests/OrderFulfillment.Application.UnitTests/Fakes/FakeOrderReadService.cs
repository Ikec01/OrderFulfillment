using OrderFulfillment.Application.Abstractions;
using OrderFulfillment.Application.Orders.Queries;

namespace OrderFulfillment.Application.UnitTests.Fakes;

internal sealed class FakeOrderReadService : IOrderReadService
{
    public List<OrderDetailsDto> Orders { get; } = [];

    public Task<OrderDetailsDto?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Orders.Find(order => order.Id == orderId));

    public Task<IReadOnlyList<OrderSummaryDto>> ListByCustomerAsync(
        Guid customerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<OrderSummaryDto> summaries = Orders
            .Where(order => order.CustomerId == customerId)
            .OrderByDescending(order => order.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(order => new OrderSummaryDto(
                order.Id,
                order.Status,
                order.Currency,
                order.TotalAmount,
                order.Items.Count,
                order.CreatedAtUtc))
            .ToList();

        return Task.FromResult(summaries);
    }
}