using OrderFulfillment.Application.Orders.Queries.ListMyOrders;
using OrderFulfillment.Application.UnitTests.Fakes;
using Xunit;

namespace OrderFulfillment.Application.UnitTests.Orders;

public sealed class ListMyOrdersQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnOnlyOwnOrdersNewestFirst()
    {
        var readService = new FakeOrderReadService();
        var user = FakeCurrentUser.Customer();
        var mine = user.UserId!.Value;
        var newest = TestOrderDtos.Create(mine, createdAtUtc: DateTime.UtcNow);
        var oldest = TestOrderDtos.Create(mine, createdAtUtc: DateTime.UtcNow.AddDays(-2));
        var someoneElses = TestOrderDtos.Create(Guid.NewGuid());
        readService.Orders.AddRange([oldest, newest, someoneElses]);
        var handler = new ListMyOrdersQueryHandler(readService, user);

        var result = await handler.Handle(new ListMyOrdersQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(newest.Id, result[0].Id);
        Assert.Equal(oldest.Id, result[1].Id);
    }

    [Fact]
    public async Task Handle_ShouldApplyPaging()
    {
        var readService = new FakeOrderReadService();
        var user = FakeCurrentUser.Customer();
        var mine = user.UserId!.Value;
        var first = TestOrderDtos.Create(mine, createdAtUtc: DateTime.UtcNow);
        var second = TestOrderDtos.Create(mine, createdAtUtc: DateTime.UtcNow.AddDays(-1));
        var third = TestOrderDtos.Create(mine, createdAtUtc: DateTime.UtcNow.AddDays(-2));
        readService.Orders.AddRange([first, second, third]);
        var handler = new ListMyOrdersQueryHandler(readService, user);

        var result = await handler.Handle(new ListMyOrdersQuery(Page: 2, PageSize: 2), CancellationToken.None);

        var item = Assert.Single(result);
        Assert.Equal(third.Id, item.Id);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoOrders_ShouldReturnEmptyList()
    {
        var handler = new ListMyOrdersQueryHandler(new FakeOrderReadService(), FakeCurrentUser.Customer());

        var result = await handler.Handle(new ListMyOrdersQuery(), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_WhenUserIsAnonymous_ShouldThrow()
    {
        var handler = new ListMyOrdersQueryHandler(new FakeOrderReadService(), FakeCurrentUser.Anonymous());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new ListMyOrdersQuery(), CancellationToken.None));
    }
}