using MediatR;
using OrderFulfillment.Application.Abstractions;
using OrderFulfillment.Domain.Orders;
using OrderFulfillment.Domain.ValueObjects;

namespace OrderFulfillment.Application.Orders.Commands.PlaceOrder;

public sealed class PlaceOrderCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : IRequestHandler<PlaceOrderCommand, Guid>
{
    public async Task<Guid> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var customerId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("Korisnik nije prijavljen.");

        var shippingAddress = Address.Create(
            request.ShippingAddress.Street,
            request.ShippingAddress.City,
            request.ShippingAddress.PostalCode,
            request.ShippingAddress.Country);

        var order = Order.Create(new CustomerId(customerId), shippingAddress, request.Currency);

        foreach (var item in request.Items)
        {
            order.AddItem(
                new ProductId(item.ProductId),
                item.ProductName,
                Money.Create(item.UnitPrice, request.Currency),
                item.Quantity);
        }

        order.Place();

        await orderRepository.AddAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return order.Id.Value;
    }
}