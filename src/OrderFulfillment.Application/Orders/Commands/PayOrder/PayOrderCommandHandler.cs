using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using OrderFulfillment.Application.Abstractions;
using OrderFulfillment.Application.Common;
using OrderFulfillment.Domain.Orders;
using OrderFulfillment.Domain.ValueObjects;

namespace OrderFulfillment.Application.Orders.Commands.PayOrder
{
    public sealed class PayOrderCommandHandler(IOrderRepository orderRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<PayOrderCommand, Result>
    {
        public async Task<Result> Handle(PayOrderCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var order = await orderRepository.GetByIdAsync(new OrderId(request.OrderId), cancellationToken);

            if (order is null)
            {
                return Result.Failure(OrderErrors.NotFound(request.OrderId));
            }
            order.MarkAsPaid(Money.Create(request.Amount, request.Currency));

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
