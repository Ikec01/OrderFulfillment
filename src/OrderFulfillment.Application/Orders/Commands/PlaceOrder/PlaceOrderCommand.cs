using MediatR;

namespace OrderFulfillment.Application.Orders.Commands.PlaceOrder;

public sealed record PlaceOrderCommand(
    string Currency,
    AddressDto ShippingAddress,
    IReadOnlyCollection<PlaceOrderItemDto> Items) : IRequest<Guid>;

public sealed record AddressDto(string Street, string City, string PostalCode, string Country);

public sealed record PlaceOrderItemDto(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);