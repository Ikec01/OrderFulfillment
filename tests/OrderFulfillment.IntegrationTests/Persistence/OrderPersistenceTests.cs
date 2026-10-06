using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using OrderFulfillment.Domain.Orders;
using OrderFulfillment.Domain.ValueObjects;
using OrderFulfillment.Infrastructure.Persistence;
using OrderFulfillment.IntegrationTests.Support;
using Xunit;

namespace OrderFulfillment.IntegrationTests.Persistence;

[Collection(PostgresCollectionDefinition.Name)]
public sealed class OrderPersistenceTests(PostgresFixture fixture)
{
    [Fact]
    public async Task SaveOrder_ThenReload_ShouldRestoreWholeAggregate()
    {
        var customerId = Guid.NewGuid();
        var order = TestData.CreatePlacedOrder(customerId);

        await fixture.SaveOrderAsync(order);
        var loaded = await fixture.LoadOrderAsync(order.Id);

        Assert.NotNull(loaded);
        Assert.Equal(order.Id, loaded.Id);
        Assert.Equal(new CustomerId(customerId), loaded.CustomerId);
        Assert.Equal(OrderStatus.Placed, loaded.Status);
        Assert.Equal("EUR", loaded.Currency);
        Assert.Equal(order.ShippingAddress, loaded.ShippingAddress);
        Assert.Equal(order.TotalAmount, loaded.TotalAmount);
        Assert.Equal(order.CreatedAtUtc, loaded.CreatedAtUtc, TimeSpan.FromMilliseconds(1));

        var item = Assert.Single(loaded.Items);
        var original = Assert.Single(order.Items);
        Assert.Equal(original.Id, item.Id);
        Assert.Equal(original.ProductId, item.ProductId);
        Assert.Equal("Laptop stand", item.ProductName);
        Assert.Equal(Money.Create(10m, "EUR"), item.UnitPrice);
        Assert.Equal(2, item.Quantity);
    }

    [Fact]
    public async Task SaveOrder_ShouldNotRestoreDomainEvents()
    {
        var order = TestData.CreatePlacedOrder(Guid.NewGuid());
        Assert.NotEmpty(order.DomainEvents);

        await fixture.SaveOrderAsync(order);
        var loaded = await fixture.LoadOrderAsync(order.Id);

        Assert.NotNull(loaded);
        Assert.Empty(loaded.DomainEvents);
    }

    [Fact]
    public async Task AddItem_ToAlreadyPersistedDraftOrder_ShouldInsertNewItem()
    {
        var order = TestData.CreateDraftOrder(Guid.NewGuid());
        await fixture.SaveOrderAsync(order);

        await using (var context = fixture.CreateDbContext())
        {
            var tracked = await new OrderRepository(context).GetByIdAsync(order.Id);
            Assert.NotNull(tracked);

            tracked.AddItem(ProductId.New(), "Mouse pad", Money.Create(5m, "EUR"), 1);
            await context.SaveChangesAsync();
        }

        var reloaded = await fixture.LoadOrderAsync(order.Id);

        Assert.NotNull(reloaded);
        Assert.Equal(2, reloaded.Items.Count);
        Assert.Equal(Money.Create(25m, "EUR"), reloaded.TotalAmount);
    }

    [Fact]
    public async Task RemoveItem_FromPersistedDraftOrder_ShouldDeleteItemRow()
    {
        var order = TestData.CreateDraftOrder(Guid.NewGuid());
        order.AddItem(ProductId.New(), "Mouse pad", Money.Create(5m, "EUR"), 1);
        await fixture.SaveOrderAsync(order);

        await using (var context = fixture.CreateDbContext())
        {
            var tracked = await new OrderRepository(context).GetByIdAsync(order.Id);
            Assert.NotNull(tracked);

            var mousePad = tracked.Items.First(item => item.ProductName == "Mouse pad");
            tracked.RemoveItem(mousePad.Id);
            await context.SaveChangesAsync();
        }

        var reloaded = await fixture.LoadOrderAsync(order.Id);

        Assert.NotNull(reloaded);
        var remaining = Assert.Single(reloaded.Items);
        Assert.Equal("Laptop stand", remaining.ProductName);
    }

    [Fact]
    public async Task StatusChange_ShouldBePersisted()
    {
        var order = TestData.CreatePlacedOrder(Guid.NewGuid());
        await fixture.SaveOrderAsync(order);

        await using (var context = fixture.CreateDbContext())
        {
            var tracked = await new OrderRepository(context).GetByIdAsync(order.Id);
            Assert.NotNull(tracked);

            tracked.MarkAsPaid(tracked.TotalAmount);
            await context.SaveChangesAsync();
        }

        var reloaded = await fixture.LoadOrderAsync(order.Id);

        Assert.NotNull(reloaded);
        Assert.Equal(OrderStatus.Paid, reloaded.Status);
    }

    [Fact]
    public async Task ConcurrentUpdates_ShouldRejectTheSecondWriter()
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

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => secondContext.SaveChangesAsync());

        var stored = await fixture.LoadOrderAsync(order.Id);
        Assert.NotNull(stored);
        Assert.Equal(OrderStatus.Paid, stored.Status);
    }

    [Fact]
    public async Task LoadOrder_ThatDoesNotExist_ShouldReturnNull()
    {
        var loaded = await fixture.LoadOrderAsync(OrderId.New());

        Assert.Null(loaded);
    }
}