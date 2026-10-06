using OrderFulfillment.Contracts.Orders;
using OrderFulfillment.Domain.Common;
using OrderFulfillment.Domain.Orders;
using OrderFulfillment.Domain.Orders.Events;
using OrderFulfillment.Domain.ValueObjects;
using OrderFulfillment.Infrastructure.Persistence.Outbox;
using Xunit;

namespace OrderFulfillment.IntegrationTests.Outbox;

public sealed class IntegrationEventMapperTests
{
    [Fact]
    public void Map_OrderPlaced_ShouldCopyAllData()
    {
        var domainEvent = new OrderPlacedDomainEvent(
            OrderId.New(),
            CustomerId.New(),
            Money.Create(20m, "EUR"),
            [new OrderPlacedItem(ProductId.New(), 2)]);

        var result = Assert.IsType<OrderPlacedIntegrationEvent>(IntegrationEventMapper.Map(domainEvent));

        Assert.Equal(domainEvent.EventId, result.EventId);
        Assert.Equal(domainEvent.OccurredOnUtc, result.OccurredOnUtc);
        Assert.Equal(domainEvent.OrderId.Value, result.OrderId);
        Assert.Equal(domainEvent.CustomerId.Value, result.CustomerId);
        Assert.Equal("EUR", result.Currency);
        Assert.Equal(20m, result.TotalAmount);

        var item = Assert.Single(result.Items);
        Assert.Equal(domainEvent.Items.Single().ProductId.Value, item.ProductId);
        Assert.Equal(2, item.Quantity);
    }

    [Fact]
    public void Map_OrderPaid_ShouldCopyAllData()
    {
        var domainEvent = new OrderPaidDomainEvent(OrderId.New(), Money.Create(20m, "EUR"));

        var result = Assert.IsType<OrderPaidIntegrationEvent>(IntegrationEventMapper.Map(domainEvent));

        Assert.Equal(domainEvent.EventId, result.EventId);
        Assert.Equal(domainEvent.OrderId.Value, result.OrderId);
        Assert.Equal("EUR", result.Currency);
        Assert.Equal(20m, result.Amount);
    }

    [Fact]
    public void Map_OrderShipped_ShouldCopyAllData()
    {
        var domainEvent = new OrderShippedDomainEvent(OrderId.New());

        var result = Assert.IsType<OrderShippedIntegrationEvent>(IntegrationEventMapper.Map(domainEvent));

        Assert.Equal(domainEvent.EventId, result.EventId);
        Assert.Equal(domainEvent.OrderId.Value, result.OrderId);
    }

    [Fact]
    public void Map_OrderDelivered_ShouldCopyAllData()
    {
        var domainEvent = new OrderDeliveredDomainEvent(OrderId.New());

        var result = Assert.IsType<OrderDeliveredIntegrationEvent>(IntegrationEventMapper.Map(domainEvent));

        Assert.Equal(domainEvent.EventId, result.EventId);
        Assert.Equal(domainEvent.OrderId.Value, result.OrderId);
    }

    [Fact]
    public void Map_OrderCancelled_ShouldCopyAllDataAndPreviousStatusAsText()
    {
        var domainEvent = new OrderCancelledDomainEvent(OrderId.New(), OrderStatus.Paid, "Kupac se predomislio");

        var result = Assert.IsType<OrderCancelledIntegrationEvent>(IntegrationEventMapper.Map(domainEvent));

        Assert.Equal(domainEvent.EventId, result.EventId);
        Assert.Equal(domainEvent.OrderId.Value, result.OrderId);
        Assert.Equal("Paid", result.PreviousStatus);
        Assert.Equal("Kupac se predomislio", result.Reason);
    }

    [Fact]
    public void Map_UnknownDomainEvent_ShouldFailLoudly()
    {
        Assert.Throws<InvalidOperationException>(() => IntegrationEventMapper.Map(new UnknownDomainEvent()));
    }

    private sealed record UnknownDomainEvent : DomainEvent;
}