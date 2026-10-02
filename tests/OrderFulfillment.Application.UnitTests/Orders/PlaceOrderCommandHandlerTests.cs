using OrderFulfillment.Application.Abstractions;
using OrderFulfillment.Application.Orders.Commands.PlaceOrder;
using OrderFulfillment.Domain.Common;
using OrderFulfillment.Domain.Orders;
using OrderFulfillment.Domain.Orders.Events;
using OrderFulfillment.Domain.ValueObjects;
using OrderFulfillment.Application.UnitTests.Fakes;
using Xunit;

namespace OrderFulfillment.Application.UnitTests.Orders;

public sealed class PlaceOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_ShouldStorePlacedOrderAndSaveOnce()
    {
        var repository = new FakeOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new PlaceOrderCommandHandler(repository, unitOfWork);
        var command = CreateValidCommand();

        var orderId = await handler.Handle(command, CancellationToken.None);

        var order = Assert.Single(repository.Orders);
        Assert.Equal(order.Id.Value, orderId);
        Assert.Equal(OrderStatus.Placed, order.Status);
        Assert.Equal(new CustomerId(command.CustomerId), order.CustomerId);
        Assert.Equal(Money.Create(20m, "EUR"), order.TotalAmount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldRaiseOrderPlacedEvent()
    {
        var repository = new FakeOrderRepository();
        var handler = new PlaceOrderCommandHandler(repository, new FakeUnitOfWork());

        await handler.Handle(CreateValidCommand(), CancellationToken.None);

        var order = Assert.Single(repository.Orders);
        Assert.IsType<OrderPlacedDomainEvent>(Assert.Single(order.DomainEvents));
    }

    [Fact]
    public async Task Handle_WithoutItems_ShouldThrowDomainExceptionAndNotSave()
    {
        var repository = new FakeOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new PlaceOrderCommandHandler(repository, unitOfWork);
        var command = CreateValidCommand() with { Items = [] };

        await Assert.ThrowsAsync<DomainException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Empty(repository.Orders);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithInvalidAddress_ShouldThrowDomainExceptionAndNotSave()
    {
        var repository = new FakeOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new PlaceOrderCommandHandler(repository, unitOfWork);
        var command = CreateValidCommand() with
        {
            ShippingAddress = new AddressDto("   ", "Beograd", "11000", "Srbija"),
        };

        await Assert.ThrowsAsync<DomainException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Empty(repository.Orders);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    private static PlaceOrderCommand CreateValidCommand() =>
        new(
            CustomerId: Guid.NewGuid(),
            Currency: "EUR",
            ShippingAddress: new AddressDto("Knez Mihailova 1", "Beograd", "11000", "Srbija"),
            Items: [new PlaceOrderItemDto(Guid.NewGuid(), "Laptop stand", 10m, 2)]);

    

    
}