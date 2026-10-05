using MediatR;
using OrderFulfillment.Application.Abstractions;
using OrderFulfillment.Application.Common;
using OrderFulfillment.Domain.Orders;

namespace OrderFulfillment.Application.Orders.Commands.CancelOrder;

public sealed class CancelOrderCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<CancelOrderCommand, Result>
{
    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var order = await orderRepository.GetByIdAsync(new OrderId(request.OrderId), cancellationToken);

        // Tuđu porudžbinu tretiramo kao nepostojeću, da napadač ne može da otkrije koji ID-jevi postoje.
        if (order is null || !OrderAccessPolicy.CanAccess(currentUser, order))
        {
            return Result.Failure(OrderErrors.NotFound(request.OrderId));
        }

        order.Cancel(request.Reason);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}