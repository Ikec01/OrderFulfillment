using OrderFulfillment.Application.Orders.Commands.PlaceOrder;
using OrderFulfillment.Domain.Orders;
using Xunit;

namespace OrderFulfillment.Application.UnitTests.Orders;

public sealed class PlaceOrderCommandValidatorTests
{
    private readonly PlaceOrderCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        var result = _validator.Validate(CreateValidCommand());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("EU")]
    [InlineData("EURO")]
    [InlineData("12E")]
    public void Validate_WithInvalidCurrency_ShouldFail(string currency)
    {
        var result = _validator.Validate(CreateValidCommand() with { Currency = currency });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithNullAddress_ShouldFailWithoutThrowing()
    {
        var result = _validator.Validate(CreateValidCommand() with { ShippingAddress = null! });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithoutItems_ShouldFail()
    {
        var result = _validator.Validate(CreateValidCommand() with { Items = [] });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithTooManyItems_ShouldFail()
    {
        var items = Enumerable
            .Range(0, Order.MaxItems + 1)
            .Select(_ => new PlaceOrderItemDto(Guid.NewGuid(), "Product", 1m, 1))
            .ToList();

        var result = _validator.Validate(CreateValidCommand() with { Items = items });

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1001)]
    public void Validate_WithInvalidQuantity_ShouldFail(int quantity)
    {
        var command = CreateValidCommand() with
        {
            Items = [new PlaceOrderItemDto(Guid.NewGuid(), "Laptop stand", 10m, quantity)],
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1000)]
    public void Validate_WithQuantityAtBoundary_ShouldPass(int quantity)
    {
        var command = CreateValidCommand() with
        {
            Items = [new PlaceOrderItemDto(Guid.NewGuid(), "Laptop stand", 10m, quantity)],
        };

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithNegativePrice_ShouldFail()
    {
        var command = CreateValidCommand() with
        {
            Items = [new PlaceOrderItemDto(Guid.NewGuid(), "Laptop stand", -1m, 1)],
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithSeveralProblems_ShouldReportAllOfThem()
    {
        var command = CreateValidCommand() with
        {
            Currency = string.Empty,
            Items = [],
        };

        var result = _validator.Validate(command);

        Assert.True(result.Errors.Count >= 2);
    }

    private static PlaceOrderCommand CreateValidCommand() =>
        new(
            Currency: "EUR",
            ShippingAddress: new AddressDto("Knez Mihailova 1", "Beograd", "11000", "Srbija"),
            Items: [new PlaceOrderItemDto(Guid.NewGuid(), "Laptop stand", 10m, 2)]);
}