using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using OrderFulfillment.Domain.Orders;
using OrderFulfillment.Domain.ValueObjects;
using OrderFulfillment.IntegrationTests.Support;
using Xunit;

namespace OrderFulfillment.IntegrationTests.Queries;

[Collection(PostgresCollectionDefinition.Name)]
public sealed class OrderReadServiceTests(PostgresFixture fixture)
{
    [Fact]
    public async Task GetById_ShouldReturnWholeOrderDetails()
    {
        var customerId = Guid.NewGuid();
        var order = TestData.CreateDraftOrder(customerId);
        order.AddItem(ProductId.New(), "Mouse pad", Money.Create(5m, "EUR"), 1);
        order.Place();
        await fixture.SaveOrderAsync(order);

        var dto = await fixture.CreateReadService().GetByIdAsync(order.Id.Value);

        Assert.NotNull(dto);
        Assert.Equal(order.Id.Value, dto.Id);
        Assert.Equal(customerId, dto.CustomerId);
        Assert.Equal(OrderStatus.Placed, dto.Status);
        Assert.Equal("EUR", dto.Currency);
        Assert.Equal(25m, dto.TotalAmount);
        Assert.Equal(order.CreatedAtUtc, dto.CreatedAtUtc, TimeSpan.FromMilliseconds(1));

        Assert.Equal(order.ShippingAddress.Street, dto.ShippingAddress.Street);
        Assert.Equal(order.ShippingAddress.City, dto.ShippingAddress.City);
        Assert.Equal(order.ShippingAddress.PostalCode, dto.ShippingAddress.PostalCode);
        Assert.Equal(order.ShippingAddress.Country, dto.ShippingAddress.Country);

        Assert.Equal(2, dto.Items.Count);
        var laptopStand = Assert.Single(dto.Items, item => item.ProductName == "Laptop stand");
        Assert.Equal(10m, laptopStand.UnitPrice);
        Assert.Equal(2, laptopStand.Quantity);
        Assert.Equal(20m, laptopStand.TotalPrice);
    }

    [Fact]
    public async Task GetById_ForUnknownOrder_ShouldReturnNull()
    {
        var dto = await fixture.CreateReadService().GetByIdAsync(Guid.NewGuid());

        Assert.Null(dto);
    }

    [Fact]
    public async Task ListByCustomer_ShouldReturnOnlyThatCustomersOrdersNewestFirst()
    {
        var customerId = Guid.NewGuid();
        var older = TestData.CreatePlacedOrder(customerId);
        await fixture.SaveOrderAsync(older);

        await Task.Delay(20);

        var newer = TestData.CreatePlacedOrder(customerId);
        await fixture.SaveOrderAsync(newer);

        await fixture.SaveOrderAsync(TestData.CreatePlacedOrder(Guid.NewGuid()));

        var result = await fixture.CreateReadService().ListByCustomerAsync(customerId, 1, 20);

        Assert.Equal(
            [newer.Id.Value, older.Id.Value],
            result.Select(summary => summary.Id));
    }

    [Fact]
    public async Task ListByCustomer_ShouldCalculateTotalAndItemCount()
    {
        var customerId = Guid.NewGuid();
        var order = TestData.CreateDraftOrder(customerId);
        order.AddItem(ProductId.New(), "Mouse pad", Money.Create(5m, "EUR"), 1);
        order.Place();
        await fixture.SaveOrderAsync(order);

        var result = await fixture.CreateReadService().ListByCustomerAsync(customerId, 1, 20);

        var summary = Assert.Single(result);
        Assert.Equal(OrderStatus.Placed, summary.Status);
        Assert.Equal("EUR", summary.Currency);
        Assert.Equal(25m, summary.TotalAmount);
        Assert.Equal(2, summary.ItemCount);
    }

    [Fact]
    public async Task ListByCustomer_ShouldIncludeOrdersWithoutItems()
    {
        var customerId = Guid.NewGuid();
        await fixture.SaveOrderAsync(TestData.CreateDraftOrder(customerId, withItem: false));

        var result = await fixture.CreateReadService().ListByCustomerAsync(customerId, 1, 20);

        var summary = Assert.Single(result);
        Assert.Equal(OrderStatus.Draft, summary.Status);
        Assert.Equal(0m, summary.TotalAmount);
        Assert.Equal(0, summary.ItemCount);
    }

    [Fact]
    public async Task ListByCustomer_ShouldApplyPagingWithoutOverlap()
    {
        var customerId = Guid.NewGuid();

        for (var i = 0; i < 3; i++)
        {
            await fixture.SaveOrderAsync(TestData.CreatePlacedOrder(customerId));
            await Task.Delay(20);
        }

        var service = fixture.CreateReadService();

        var firstPage = await service.ListByCustomerAsync(customerId, 1, 2);
        var secondPage = await service.ListByCustomerAsync(customerId, 2, 2);

        Assert.Equal(2, firstPage.Count);
        Assert.Single(secondPage);

        var allIds = firstPage.Concat(secondPage).Select(summary => summary.Id).ToList();
        Assert.Equal(3, allIds.Distinct().Count());
    }

    [Fact]
    public async Task ListByCustomer_ForCustomerWithoutOrders_ShouldReturnEmptyList()
    {
        var result = await fixture.CreateReadService().ListByCustomerAsync(Guid.NewGuid(), 1, 20);

        Assert.Empty(result);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    public async Task ListByCustomer_WithInvalidPaging_ShouldThrow(int page, int pageSize)
    {
        var service = fixture.CreateReadService();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.ListByCustomerAsync(Guid.NewGuid(), page, pageSize));
    }
}