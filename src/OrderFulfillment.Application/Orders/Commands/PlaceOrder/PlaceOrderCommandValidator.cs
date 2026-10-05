using FluentValidation;
using OrderFulfillment.Domain.Orders;
using OrderFulfillment.Domain.ValueObjects;

namespace OrderFulfillment.Application.Orders.Commands.PlaceOrder;

public sealed class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(command => command.Currency)
            .NotEmpty()
            .Matches("^[A-Za-z]{3}$")
            .WithMessage("Valuta mora biti ISO 4217 kod od tri slova (npr. EUR).");

        RuleFor(command => command.ShippingAddress)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .ChildRules(address =>
            {
                address.RuleFor(a => a.Street).NotEmpty().MaximumLength(Address.MaxStreetLenght);
                address.RuleFor(a => a.City).NotEmpty().MaximumLength(Address.MaxCityLength);
                address.RuleFor(a => a.PostalCode).NotEmpty().MaximumLength(Address.MaxPostalCodeLength);
                address.RuleFor(a => a.Country).NotEmpty().MaximumLength(Address.MaxCountryLength);
            });

        RuleFor(command => command.Items)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(items => items.Count <= Order.MaxItems)
            .WithMessage($"Porudžbina ne može imati više od {Order.MaxItems} stavki.");

        RuleForEach(command => command.Items)
            .ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId).NotEmpty();
                item.RuleFor(i => i.ProductName).NotEmpty().MaximumLength(OrderItem.MaxProductNameLength);
                item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0m);
                item.RuleFor(i => i.Quantity).InclusiveBetween(1, OrderItem.MaxQuantity);
            });
    }
}