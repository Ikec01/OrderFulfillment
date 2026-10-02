using OrderFulfillment.Application.Common;
using OrderFulfillment.Application.Orders.Commands.ShipOrder;
using OrderFulfillment.Application.UnitTests.Fakes;
using OrderFulfillment.Domain.Common;
using OrderFulfillment.Domain.Orders;
using OrderFulfillment.Domain.Orders.Events;
using Xunit;

namespace OrderFulfillment.Application.UnitTests.Orders;

public sealed class ShipOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenOrderIsPaid_ShouldShipAndSaveOnce()
    {
        var repository = new FakeOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        var order = TestOrders.CreatePaid();
        repository.Orders.Add(order);
        var handler = new ShipOrderCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new ShipOrderCommand(order.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Shipped, order.Status);
        Assert.IsType<OrderShippedDomainEvent>(Assert.Single(order.DomainEvents));
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenOrderDoesNotExist_ShouldReturnNotFoundAndNotSave()
    {
        var unitOfWork = new FakeUnitOfWork();
        var handler = new ShipOrderCommandHandler(new FakeOrderRepository(), unitOfWork);

        var result = await handler.Handle(new ShipOrderCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenOrderIsNotPaid_ShouldThrowAndNotSave()
    {
        var repository = new FakeOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        var order = TestOrders.CreatePlaced();
        repository.Orders.Add(order);
        var handler = new ShipOrderCommandHandler(repository, unitOfWork);

        await Assert.ThrowsAsync<DomainException>(
            () => handler.Handle(new ShipOrderCommand(order.Id.Value), CancellationToken.None));

        Assert.Equal(OrderStatus.Placed, order.Status);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }
}