using OrderFulfillment.Application.Common;
using OrderFulfillment.Application.Orders.Commands.DeliverOrder;
using OrderFulfillment.Application.UnitTests.Fakes;
using OrderFulfillment.Domain.Common;
using OrderFulfillment.Domain.Orders;
using OrderFulfillment.Domain.Orders.Events;
using Xunit;

namespace OrderFulfillment.Application.UnitTests.Orders;

public sealed class DeliverOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenOrderIsShipped_ShouldDeliverAndSaveOnce()
    {
        var repository = new FakeOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        var order = TestOrders.CreateShipped();
        repository.Orders.Add(order);
        var handler = new DeliverOrderCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new DeliverOrderCommand(order.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Delivered, order.Status);
        Assert.IsType<OrderDeliveredDomainEvent>(Assert.Single(order.DomainEvents));
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenOrderDoesNotExist_ShouldReturnNotFoundAndNotSave()
    {
        var unitOfWork = new FakeUnitOfWork();
        var handler = new DeliverOrderCommandHandler(new FakeOrderRepository(), unitOfWork);

        var result = await handler.Handle(new DeliverOrderCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenOrderIsNotShipped_ShouldThrowAndNotSave()
    {
        var repository = new FakeOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        var order = TestOrders.CreatePaid();
        repository.Orders.Add(order);
        var handler = new DeliverOrderCommandHandler(repository, unitOfWork);

        await Assert.ThrowsAsync<DomainException>(
            () => handler.Handle(new DeliverOrderCommand(order.Id.Value), CancellationToken.None));

        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }
}