using FluentValidation;
using MediatR;
using OrderFulfillment.Application.Behaviors;
using OrderFulfillment.Application.Orders.Commands.PlaceOrder;
using Xunit;

namespace OrderFulfillment.Application.UnitTests.Behaviors;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WithInvalidRequest_ShouldThrowAndNotCallNext()
    {
        var behavior = new ValidationBehavior<PlaceOrderCommand, Guid>(
            [new PlaceOrderCommandValidator()]);
        var invalidCommand = CreateValidCommand() with { Items = [] };
        var nextCalled = false;

        RequestHandlerDelegate<Guid> next = _ =>
        {
            nextCalled = true;

            return Task.FromResult(Guid.NewGuid());
        };

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(invalidCommand, next, CancellationToken.None));

        Assert.False(nextCalled);
        Assert.NotEmpty(exception.Errors);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCallNextAndReturnItsResult()
    {
        var behavior = new ValidationBehavior<PlaceOrderCommand, Guid>(
            [new PlaceOrderCommandValidator()]);
        var expected = Guid.NewGuid();

        RequestHandlerDelegate<Guid> next = _ => Task.FromResult(expected);

        var result = await behavior.Handle(CreateValidCommand(), next, CancellationToken.None);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task Handle_WithoutValidators_ShouldCallNext()
    {
        var behavior = new ValidationBehavior<PlaceOrderCommand, Guid>([]);
        var expected = Guid.NewGuid();

        RequestHandlerDelegate<Guid> next = _ => Task.FromResult(expected);

        var result = await behavior.Handle(CreateValidCommand(), next, CancellationToken.None);

        Assert.Equal(expected, result);
    }

    private static PlaceOrderCommand CreateValidCommand() =>
    new(
        Currency: "EUR",
        ShippingAddress: new AddressDto("Knez Mihailova 1", "Beograd", "11000", "Srbija"),
        Items: [new PlaceOrderItemDto(Guid.NewGuid(), "Laptop stand", 10m, 2)]);
}