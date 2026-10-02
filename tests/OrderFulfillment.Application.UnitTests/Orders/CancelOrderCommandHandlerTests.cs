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
    public async Task Handle_WhenOrderIsPlaced_ShouldCancelAndSaveOnce()
    {
        var repository = new FakeOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        var order = TestOrders.CreatePlaced();
        repository.Orders.Add(order);
        var handler = new CancelOrderCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(
            new CancelOrderCommand(order.Id.Value, "Kupac se predomislio"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.IsType<OrderCancelledDomainEvent>(Assert.Single(order.DomainEvents));
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenOrderDoesNotExist_ShouldReturnNotFoundAndNotSave()
    {
        var unitOfWork = new FakeUnitOfWork();
        var handler = new CancelOrderCommandHandler(new FakeOrderRepository(), unitOfWork);

        var result = await handler.Handle(
            new CancelOrderCommand(Guid.NewGuid(), "Razlog"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenOrderIsShipped_ShouldThrowAndNotSave()
    {
        var repository = new FakeOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        var order = TestOrders.CreateShipped();
        repository.Orders.Add(order);
        var handler = new CancelOrderCommandHandler(repository, unitOfWork);

        await Assert.ThrowsAsync<DomainException>(
            () => handler.Handle(
                new CancelOrderCommand(order.Id.Value, "Razlog"),
                CancellationToken.None));

        Assert.Equal(OrderStatus.Shipped, order.Status);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }
}