namespace OrderFulfillment.Contracts.Orders;

public sealed record PlacedOrderItem(Guid ProductId, int Quantity);

public sealed record OrderPlacedIntegrationEvent(
    Guid EventId,
    DateTime OccurredOnUtc,
    Guid OrderId,
    Guid CustomerId,
    string Currency,
    decimal TotalAmount,
    IReadOnlyCollection<PlacedOrderItem> Items);

public sealed record OrderPaidIntegrationEvent(
    Guid EventId,
    DateTime OccurredOnUtc,
    Guid OrderId,
    string Currency,
    decimal Amount);

public sealed record OrderShippedIntegrationEvent(
    Guid EventId,
    DateTime OccurredOnUtc,
    Guid OrderId);

public sealed record OrderDeliveredIntegrationEvent(
    Guid EventId,
    DateTime OccurredOnUtc,
    Guid OrderId);

public sealed record OrderCancelledIntegrationEvent(
    Guid EventId,
    DateTime OccurredOnUtc,
    Guid OrderId,
    string PreviousStatus,
    string Reason);