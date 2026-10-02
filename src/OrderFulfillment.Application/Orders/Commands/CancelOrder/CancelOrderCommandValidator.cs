using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using OrderFulfillment.Domain.Orders;

namespace OrderFulfillment.Application.Orders.Commands.CancelOrder
{
    public sealed class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
    {
        public CancelOrderCommandValidator()
        {
            RuleFor(command => command.OrderId).NotEmpty();
            RuleFor(command => command.Reason)
                .NotEmpty()
                .MaximumLength(Order.MaxCancellationReasonLength);
        }
    }
}
