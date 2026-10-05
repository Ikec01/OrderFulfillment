using OrderFulfillment.Domain.Orders;

namespace OrderFulfillment.Application.Orders.Queries;

public sealed record OrderDetailsDto(
    Guid Id,
    Guid CustomerId,
    OrderStatus Status,
    string Currency,
    decimal TotalAmount,
    DateTime CreatedAtUtc,
    OrderAddressDto ShippingAddress,
    IReadOnlyCollection<OrderItemDto> Items);

public sealed record OrderAddressDto(string Street, string City, string PostalCode, string Country);

public sealed record OrderItemDto(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice);

public sealed record OrderSummaryDto(
    Guid Id,
    OrderStatus Status,
    string Currency,
    decimal TotalAmount,
    int ItemCount,
    DateTime CreatedAtUtc);