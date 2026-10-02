using MediatR;
using OrderFulfillment.Application.Abstractions;
using OrderFulfillment.Application.Common;
using OrderFulfillment.Domain.Orders;

namespace OrderFulfillment.Application.Orders.Commands.DeliverOrder;

public sealed class DeliverOrderCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeliverOrderCommand, Result>
{
    public async Task<Result> Handle(DeliverOrderCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var order = await orderRepository.GetByIdAsync(new OrderId(request.OrderId), cancellationToken);

        if (order is null)
        {
            return Result.Failure(OrderErrors.NotFound(request.OrderId));
        }

        order.Deliver();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}