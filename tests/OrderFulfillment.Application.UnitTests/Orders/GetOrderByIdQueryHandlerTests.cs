using OrderFulfillment.Application.Common;
using OrderFulfillment.Application.Orders.Queries.GetOrderById;
using OrderFulfillment.Application.UnitTests.Fakes;
using Xunit;

namespace OrderFulfillment.Application.UnitTests.Orders;

public sealed class GetOrderByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenOwnerRequests_ShouldReturnOrder()
    {
        var readService = new FakeOrderReadService();
        var owner = FakeCurrentUser.Customer();
        var order = TestOrderDtos.Create(owner.UserId!.Value);
        readService.Orders.Add(order);
        var handler = new GetOrderByIdQueryHandler(readService, owner);

        var result = await handler.Handle(new GetOrderByIdQuery(order.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(order, result.Value);
    }

    [Fact]
    public async Task Handle_WhenAdministratorRequestsSomeoneElsesOrder_ShouldReturnOrder()
    {
        var readService = new FakeOrderReadService();
        var order = TestOrderDtos.Create(Guid.NewGuid());
        readService.Orders.Add(order);
        var handler = new GetOrderByIdQueryHandler(readService, FakeCurrentUser.Administrator());

        var result = await handler.Handle(new GetOrderByIdQuery(order.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenAnotherCustomerRequests_ShouldReturnNotFound()
    {
        var readService = new FakeOrderReadService();
        var order = TestOrderDtos.Create(Guid.NewGuid());
        readService.Orders.Add(order);
        var handler = new GetOrderByIdQueryHandler(readService, FakeCurrentUser.Customer());

        var result = await handler.Handle(new GetOrderByIdQuery(order.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WhenUserIsAnonymous_ShouldReturnNotFound()
    {
        var readService = new FakeOrderReadService();
        var order = TestOrderDtos.Create(Guid.NewGuid());
        readService.Orders.Add(order);
        var handler = new GetOrderByIdQueryHandler(readService, FakeCurrentUser.Anonymous());

        var result = await handler.Handle(new GetOrderByIdQuery(order.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WhenOrderDoesNotExist_ShouldReturnNotFound()
    {
        var handler = new GetOrderByIdQueryHandler(new FakeOrderReadService(), FakeCurrentUser.Customer());

        var result = await handler.Handle(new GetOrderByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_ForUnknownAndForeignOrder_ShouldReturnIdenticalErrorType()
    {
        var readService = new FakeOrderReadService();
        var foreignOrder = TestOrderDtos.Create(Guid.NewGuid());
        readService.Orders.Add(foreignOrder);
        var handler = new GetOrderByIdQueryHandler(readService, FakeCurrentUser.Customer());

        var foreign = await handler.Handle(new GetOrderByIdQuery(foreignOrder.Id), CancellationToken.None);
        var unknown = await handler.Handle(new GetOrderByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(foreign.Error.Code, unknown.Error.Code);
        Assert.Equal(foreign.Error.Type, unknown.Error.Type);
    }
}