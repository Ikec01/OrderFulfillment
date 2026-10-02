using OrderFulfillment.Application.Common;
using OrderFulfillment.Application.Orders.Commands.PayOrder;
using OrderFulfillment.Application.UnitTests.Fakes;
using OrderFulfillment.Domain.Common;
using OrderFulfillment.Domain.Orders;
using OrderFulfillment.Domain.Orders.Events;
using OrderFulfillment.Domain.ValueObjects;
using Xunit;

namespace OrderFulfillment.Application.UnitTests.Orders;

public sealed class PayOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenOrderExists_ShouldMarkAsPaidAndSaveOnce()
    {
        var repository = new FakeOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        var order = CreatePlacedOrder();
        repository.Orders.Add(order);
        var handler = new PayOrderCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(
            new PayOrderCommand(order.Id.Value, 20m, "EUR"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.IsType<OrderPaidDomainEvent>(Assert.Single(order.DomainEvents));
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenOrderDoesNotExist_ShouldReturnNotFoundAndNotSave()
    {
        var unitOfWork = new FakeUnitOfWork();
        var handler = new PayOrderCommandHandler(new FakeOrderRepository(), unitOfWork);
        var missingId = Guid.NewGuid();

        var result = await handler.Handle(
            new PayOrderCommand(missingId, 20m, "EUR"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithWrongAmount_ShouldThrowAndNotSave()
    {
        var repository = new FakeOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        var order = CreatePlacedOrder();
        repository.Orders.Add(order);
        var handler = new PayOrderCommandHandler(repository, unitOfWork);

        await Assert.ThrowsAsync<DomainException>(
            () => handler.Handle(
                new PayOrderCommand(order.Id.Value, 1m, "EUR"),
                CancellationToken.None));

        Assert.Equal(OrderStatus.Placed, order.Status);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenOrderAlreadyPaid_ShouldThrowAndNotSave()
    {
        var repository = new FakeOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        var order = CreatePlacedOrder();
        order.MarkAsPaid(order.TotalAmount);
        repository.Orders.Add(order);
        var handler = new PayOrderCommandHandler(repository, unitOfWork);

        await Assert.ThrowsAsync<DomainException>(
            () => handler.Handle(
                new PayOrderCommand(order.Id.Value, 20m, "EUR"),
                CancellationToken.None));

        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    private static Order CreatePlacedOrder()
    {
        var order = Order.Create(
            CustomerId.New(),
            Address.Create("Knez Mihailova 1", "Beograd", "11000", "Srbija"),
            "EUR");
        order.AddItem(ProductId.New(), "Laptop stand", Money.Create(10m, "EUR"), 2);
        order.Place();
        order.ClearDomainEvents();

        return order;
    }
}