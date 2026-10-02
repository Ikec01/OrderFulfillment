using MediatR;
using OrderFulfillment.Application.Abstractions;
using OrderFulfillment.Application.Common;
using OrderFulfillment.Domain.Orders;

namespace OrderFulfillment.Application.Orders.Commands.ShipOrder;

public sealed class ShipOrderCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ShipOrderCommand, Result>
{
    public async Task<Result> Handle(ShipOrderCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var order = await orderRepository.GetByIdAsync(new OrderId(request.OrderId), cancellationToken);

        if (order is null)
        {
            return Result.Failure(OrderErrors.NotFound(request.OrderId));
        }

        order.Ship();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}