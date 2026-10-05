using MediatR;
using OrderFulfillment.Application.Abstractions;

namespace OrderFulfillment.Application.Orders.Queries.ListMyOrders;

public sealed class ListMyOrdersQueryHandler(
    IOrderReadService readService,
    ICurrentUser currentUser)
    : IRequestHandler<ListMyOrdersQuery, IReadOnlyList<OrderSummaryDto>>
{
    public async Task<IReadOnlyList<OrderSummaryDto>> Handle(
        ListMyOrdersQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var customerId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("Korisnik nije prijavljen.");

        return await readService.ListByCustomerAsync(
            customerId,
            request.Page,
            request.PageSize,
            cancellationToken);
    }
}