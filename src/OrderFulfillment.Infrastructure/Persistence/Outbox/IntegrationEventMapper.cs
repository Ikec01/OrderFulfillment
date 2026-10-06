using OrderFulfillment.Contracts.Orders;
using OrderFulfillment.Domain.Common;
using OrderFulfillment.Domain.Orders.Events;

namespace OrderFulfillment.Infrastructure.Persistence.Outbox;

public static class IntegrationEventMapper
{
    public static object Map(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        return domainEvent switch
        {
            OrderPlacedDomainEvent placed => new OrderPlacedIntegrationEvent(
                placed.EventId,
                placed.OccurredOnUtc,
                placed.OrderId.Value,
                placed.CustomerId.Value,
                placed.TotalAmount.Currency,
                placed.TotalAmount.Amount,
                placed.Items
                    .Select(item => new PlacedOrderItem(item.ProductId.Value, item.Quantity))
                    .ToList()),

            OrderPaidDomainEvent paid => new OrderPaidIntegrationEvent(
                paid.EventId,
                paid.OccurredOnUtc,
                paid.OrderId.Value,
                paid.Amount.Currency,
                paid.Amount.Amount),

            OrderShippedDomainEvent shipped => new OrderShippedIntegrationEvent(
                shipped.EventId,
                shipped.OccurredOnUtc,
                shipped.OrderId.Value),

            OrderDeliveredDomainEvent delivered => new OrderDeliveredIntegrationEvent(
                delivered.EventId,
                delivered.OccurredOnUtc,
                delivered.OrderId.Value),

            OrderCancelledDomainEvent cancelled => new OrderCancelledIntegrationEvent(
                cancelled.EventId,
                cancelled.OccurredOnUtc,
                cancelled.OrderId.Value,
                cancelled.PreviousStatus.ToString(),
                cancelled.Reason),

            _ => throw new InvalidOperationException(
                $"Za domenski događaj '{domainEvent.GetType().Name}' ne postoji integracioni događaj."),
        };
    }
}