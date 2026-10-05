using MediatR;
using OrderFulfillment.Application.Abstractions;
using OrderFulfillment.Application.Common;

namespace OrderFulfillment.Application.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdQueryHandler(
    IOrderReadService readService,
    ICurrentUser currentUser)
    : IRequestHandler<GetOrderByIdQuery, Result<OrderDetailsDto>>
{
    public async Task<Result<OrderDetailsDto>> Handle(
        GetOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var order = await readService.GetByIdAsync(request.OrderId, cancellationToken);

        // Tuđu porudžbinu tretiramo kao nepostojeću, da se ne otkrije koji ID-jevi postoje.
        if (order is null || !OrderAccessPolicy.CanAccess(currentUser, order.CustomerId))
        {
            return Result.Failure<OrderDetailsDto>(OrderErrors.NotFound(request.OrderId));
        }

        return Result.Success(order);
    }
}