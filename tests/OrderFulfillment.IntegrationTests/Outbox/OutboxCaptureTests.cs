using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using OrderFulfillment.Contracts.Orders;
using OrderFulfillment.Domain.Orders;
using OrderFulfillment.Infrastructure.Persistence;
using OrderFulfillment.Infrastructure.Persistence.Outbox;
using OrderFulfillment.IntegrationTests.Support;
using Xunit;

namespace OrderFulfillment.IntegrationTests.Outbox;

[Collection(PostgresCollectionDefinition.Name)]
public sealed class OutboxCaptureTests(PostgresFixture fixture)
{
    [Fact]
    public async Task SavingOrder_ShouldWriteOutboxMessageForItsEvent()
    {
        var customerId = Guid.NewGuid();
        var order = TestData.CreatePlacedOrder(customerId);
        var eventIds = order.DomainEvents.Select(domainEvent => domainEvent.EventId).ToArray();
        Assert.Single(eventIds);

        await fixture.SaveOrderAsync(order);

        var message = Assert.Single(await LoadMessagesAsync(eventIds));
        Assert.Equal(eventIds[0], message.Id);
        Assert.Equal(typeof(OrderPlacedIntegrationEvent).FullName, message.Type);
        Assert.Null(message.ProcessedOnUtc);
        Assert.Equal(0, message.Attempts);
        Assert.Null(message.Error);

        var integrationEvent = Assert.IsType<OrderPlacedIntegrationEvent>(
            OutboxSerializer.Deserialize(message.Type, message.Content));
        Assert.Equal(order.Id.Value, integrationEvent.OrderId);
        Assert.Equal(customerId, integrationEvent.CustomerId);
        Assert.Equal("EUR", integrationEvent.Currency);
        Assert.Equal(20m, integrationEvent.TotalAmount);
        Assert.Equal(2, Assert.Single(integrationEvent.Items).Quantity);
    }

    [Fact]
    public async Task SavingOrder_ShouldClearDomainEventsFromAggregate()
    {
        var order = TestData.CreatePlacedOrder(Guid.NewGuid());
        Assert.NotEmpty(order.DomainEvents);

        await fixture.SaveOrderAsync(order);

        Assert.Empty(order.DomainEvents);
    }

    [Fact]
    public async Task SavingOrderWithoutEvents_ShouldNotWriteOutboxMessages()
    {
        var order = TestData.CreateDraftOrder(Guid.NewGuid());
        Assert.Empty(order.DomainEvents);

        await fixture.SaveOrderAsync(order);

        Assert.Empty(order.DomainEvents);
    }

    [Fact]
    public async Task FailedSave_ShouldNotLeaveOutboxMessagesBehind()
    {
        var order = TestData.CreatePlacedOrder(Guid.NewGuid());
        await fixture.SaveOrderAsync(order);

        await using var firstContext = fixture.CreateDbContext();
        await using var secondContext = fixture.CreateDbContext();

        var firstCopy = await new OrderRepository(firstContext).GetByIdAsync(order.Id);
        var secondCopy = await new OrderRepository(secondContext).GetByIdAsync(order.Id);
        Assert.NotNull(firstCopy);
        Assert.NotNull(secondCopy);

        firstCopy.MarkAsPaid(firstCopy.TotalAmount);
        await firstContext.SaveChangesAsync();

        secondCopy.Cancel("Kupac se predomislio");
        var cancelEventIds = secondCopy.DomainEvents.Select(domainEvent => domainEvent.EventId).ToArray();
        Assert.Single(cancelEventIds);

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => secondContext.SaveChangesAsync());

        // Promena stanja i poruka su jedna celina: ako pukne snimanje, poruke nema.
        Assert.Empty(await LoadMessagesAsync(cancelEventIds));
        Assert.Single(secondCopy.DomainEvents);
    }

    [Fact]
    public async Task OrderLifecycle_ShouldProduceOutboxMessagesInOrder()
    {
        var order = TestData.CreatePlacedOrder(Guid.NewGuid());
        var eventIds = order.DomainEvents.Select(domainEvent => domainEvent.EventId).ToList();
        await fixture.SaveOrderAsync(order);

        await UpdateOrderAsync(order.Id, loaded => loaded.MarkAsPaid(loaded.TotalAmount), eventIds);
        await UpdateOrderAsync(order.Id, loaded => loaded.Ship(), eventIds);
        await UpdateOrderAsync(order.Id, loaded => loaded.Deliver(), eventIds);

        var messages = await LoadMessagesAsync(eventIds.ToArray());

        Assert.Collection(
            messages,
            message => Assert.Equal(typeof(OrderPlacedIntegrationEvent).FullName, message.Type),
            message => Assert.Equal(typeof(OrderPaidIntegrationEvent).FullName, message.Type),
            message => Assert.Equal(typeof(OrderShippedIntegrationEvent).FullName, message.Type),
            message => Assert.Equal(typeof(OrderDeliveredIntegrationEvent).FullName, message.Type));
    }

    private async Task UpdateOrderAsync(OrderId id, Action<Order> action, List<Guid> eventIds)
    {
        await using var context = fixture.CreateDbContext();

        var order = await new OrderRepository(context).GetByIdAsync(id);
        Assert.NotNull(order);

        action(order);
        eventIds.AddRange(order.DomainEvents.Select(domainEvent => domainEvent.EventId));

        await context.SaveChangesAsync();
    }

    private async Task<List<OutboxMessage>> LoadMessagesAsync(Guid[] ids)
    {
        await using var context = fixture.CreateDbContext();

        return await context.OutboxMessages
            .Where(message => ids.Contains(message.Id))
            .OrderBy(message => message.OccurredOnUtc)
            .ToListAsync();
    }
}