using OrderFulfillment.Application.Common;
using OrderFulfillment.Application.Orders.Commands.CancelOrder;
using OrderFulfillment.Application.UnitTests.Fakes;
using OrderFulfillment.Domain.Common;
using OrderFulfillment.Domain.Orders;
using OrderFulfillment.Domain.Orders.Events;
using Xunit;

namespace OrderFulfillment.Application.UnitTests.Orders;

public sealed class CancelOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenOwnerCancelsPlacedOrder_ShouldCancelAndSaveOnce()
    {
        var repository = new FakeOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        var owner = FakeCurrentUser.Customer();
        var order = TestOrders.CreatePlaced(OwnerIdOf(owner));
        repository.Orders.Add(order);
        var handler = new CancelOrderCommandHandler(repository, unitOfWork, owner);

        var result = await handler.Handle(
            new CancelOrderCommand(order.Id.Value, "Kupac se predomislio"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.IsType<OrderCancelledDomainEvent>(Assert.Single(order.DomainEvents));
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenAdministratorCancelsSomeoneElsesOrder_ShouldSucceed()
    {
        var repository = new FakeOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        var order = TestOrders.CreatePlaced();
        repository.Orders.Add(order);
        var handler = new CancelOrderCommandHandler(repository, unitOfWork, FakeCurrentUser.Administrator());

        var result = await handler.Handle(
            new CancelOrderCommand(order.Id.Value, "Odluka podrške"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public async Task Handle_WhenAnotherCustomerTriesToCancel_ShouldReturnNotFoundAndNotChangeOrder()
    {
        var repository = new FakeOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        var order = TestOrders.CreatePlaced();
        repository.Orders.Add(order);
        var handler = new CancelOrderCommandHandler(repository, unitOfWork, FakeCurrentUser.Customer());

        var result = await handler.Handle(
            new CancelOrderCommand(order.Id.Value, "Razlog"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(OrderStatus.Placed, order.Status);
        Assert.Empty(order.DomainEvents);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenUserIsAnonymous_ShouldReturnNotFound()
    {
        var repository = new FakeOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        var order = TestOrders.CreatePlaced();
        repository.Orders.Add(order);
        var handler = new CancelOrderCommandHandler(repository, unitOfWork, FakeCurrentUser.Anonymous());

        var result = await handler.Handle(
            new CancelOrderCommand(order.Id.Value, "Razlog"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenOrderDoesNotExist_ShouldReturnNotFoundAndNotSave()
    {
        var unitOfWork = new FakeUnitOfWork();
        var handler = new CancelOrderCommandHandler(
            new FakeOrderRepository(),
            unitOfWork,
            FakeCurrentUser.Customer());

        var result = await handler.Handle(
            new CancelOrderCommand(Guid.NewGuid(), "Razlog"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenOwnerCancelsShippedOrder_ShouldThrowAndNotSave()
    {
        var repository = new FakeOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        var owner = FakeCurrentUser.Customer();
        var order = TestOrders.CreateShipped(OwnerIdOf(owner));
        repository.Orders.Add(order);
        var handler = new CancelOrderCommandHandler(repository, unitOfWork, owner);

        await Assert.ThrowsAsync<DomainException>(
            () => handler.Handle(
                new CancelOrderCommand(order.Id.Value, "Razlog"),
                CancellationToken.None));

        Assert.Equal(OrderStatus.Shipped, order.Status);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    private static CustomerId OwnerIdOf(FakeCurrentUser user) => new(user.UserId!.Value);
}