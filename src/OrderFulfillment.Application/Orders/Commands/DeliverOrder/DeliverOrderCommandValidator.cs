using FluentValidation;

namespace OrderFulfillment.Application.Orders.Commands.DeliverOrder;

public sealed class DeliverOrderCommandValidator : AbstractValidator<DeliverOrderCommand>
{
    public DeliverOrderCommandValidator()
    {
        RuleFor(command => command.OrderId).NotEmpty();
    }
}