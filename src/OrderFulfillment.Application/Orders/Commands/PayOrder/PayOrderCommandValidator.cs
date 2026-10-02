using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace OrderFulfillment.Application.Orders.Commands.PayOrder
{
    public sealed class PayOrderCommandValidator : AbstractValidator<PayOrderCommand>
    {
        public PayOrderCommandValidator()
        {
            RuleFor(command => command.OrderId).NotEmpty();
            RuleFor(command => command.Amount).GreaterThanOrEqualTo(0m);
            RuleFor(command => command.Currency)
                .NotEmpty()
                .Matches("^[A-Za-z]{3}$")
                .WithMessage("Valuta mora biti ISO 4217 kod od tri slova (npr. EUR).");
        }
    }
}
