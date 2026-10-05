using MediatR;

namespace OrderFulfillment.Application.Orders.Queries.ListMyOrders;

public sealed record ListMyOrdersQuery(int Page = 1, int PageSize = 20)
    : IRequest<IReadOnlyList<OrderSummaryDto>>;