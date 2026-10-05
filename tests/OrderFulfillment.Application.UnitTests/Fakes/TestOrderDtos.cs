using OrderFulfillment.Application.Orders.Queries;
using OrderFulfillment.Domain.Orders;

namespace OrderFulfillment.Application.UnitTests.Fakes;

internal static class TestOrderDtos
{
    public static OrderDetailsDto Create(
        Guid customerId,
        OrderStatus status = OrderStatus.Placed,
        DateTime? createdAtUtc = null) =>
        new(
            Id: Guid.NewGuid(),
            CustomerId: customerId,
            Status: status,
            Currency: "EUR",
            TotalAmount: 20m,
            CreatedAtUtc: createdAtUtc ?? DateTime.UtcNow,
            ShippingAddress: new OrderAddressDto("Knez Mihailova 1", "Beograd", "11000", "Srbija"),
            Items: [new OrderItemDto(Guid.NewGuid(), "Laptop stand", 10m, 2, 20m)]);
}