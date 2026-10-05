using OrderFulfillment.Application.Orders.Queries;

namespace OrderFulfillment.Application.Abstractions;

public interface IOrderReadService
{
    Task<OrderDetailsDto?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderSummaryDto>> ListByCustomerAsync(
        Guid customerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}